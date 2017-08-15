// Copyright 2017 Louis S.Berman.
//
// This file is part of TickData.
//
// TickData is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation, either version 3 of the License, 
// or (at your option) any later version.
//
// TickData is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with TickData.  If not, see <http://www.gnu.org/licenses/>.

using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickData.Common.Helpers;
using TickData.Common.Trading;
using static TickData.HistDataFetch.Properties.Settings;

namespace TickData.HistDataFetch
{
    class Program
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to cancel...");
            Console.WriteLine();

            var cts = new CancellationTokenSource();

            var client = new HttpClient();

            try
            {
                ValidateAndLogSettings();

                PurgeUnwantedTickFiles(DataKind.CSV);
                PurgeUnwantedTickFiles(DataKind.Ticks);

                var wasCancelled = false;

                var readKey = new TaskFactory(cts.Token).StartNew(() =>
                {
                    while (!Console.KeyAvailable)
                        Thread.Sleep(100);

                    cts.Cancel();

                    Console.ReadKey(true);

                    wasCancelled = true;
                });

                FetchArchives(readKey, cts.Token);

                if (!wasCancelled)
                    ProcessArchives(readKey, cts.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (AggregateException errors)
            {
                cts.Cancel();

                foreach (var error in errors.InnerExceptions)
                    logger.Error(error.Message.ToSingleLine());
            }
            catch (Exception error)
            {
                cts.Cancel();

                logger.Error(error.Message.ToSingleLine());
            }

            Thread.Sleep(500);

            Console.WriteLine();
            Console.Write("Press any key to terminate...");

            Console.ReadKey(true);
        }

        private static void PurgeUnwantedTickFiles(DataKind dataKind)
        {
            var basePath = Path.Combine(Default.TickDataPath,
                Source.HistData.ToString(), dataKind.ToString()) + "\\";

            basePath.EnsurePathExists();

            var folders = Directory.GetDirectories(basePath, "*.*",
                SearchOption.AllDirectories).Where(f => CanPurge(f)).ToList();

            foreach (var folder in folders)
            {
                Directory.Delete(folder, true);

                logger.Info("PURGED" + folder);
            }
        }

        private static bool CanPurge(string folder)
        {
            if (!int.TryParse(Funcify.Lambda(folder,
                f => f.Split(Path.DirectorySeparatorChar).Last()), out int year))
            {
                return false;
            }

            return year < Default.FirstYearToFetch;
        }

        private static void ValidateAndLogSettings()
        {
            WellKnown.BaseDate.MinYear = Default.FirstYearToFetch;

            Default.TickDataPath.EnsurePathExists();

            if (AssetsToFetch.Count == 0)
                throw new ArgumentOutOfRangeException(nameof(WellKnown.Assets));

            logger.Info($"AssetsToFetch={string.Join(",", AssetsToFetch)}; MinYear={WellKnown.BaseDate.MinYear}; TickDataPath={Default.TickDataPath}");
        }

        private static void ProcessArchives(Task readKey, CancellationToken token)
        {
            var processor = new ActionBlock<Asset>(
                async asset =>
                {
                    var processJobs = GetArchiveJobs(new List<Asset> { asset });

                    logger.Info($"PROCESS-QUEUE: {processJobs.Count} {asset} archives");

                    TickFile tickFile = null;

                    foreach (var archiveJob in processJobs)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        using (var stream = File.OpenRead(archiveJob.FullPath))
                        {
                            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                            {
                                var entry = archive.GetEntry(archiveJob.EntryName);

                                if (token.IsCancellationRequested)
                                    return;

                                tickFile = await ProcessEntryAsync(asset, tickFile, entry, token);
                            }
                        }
                    }
                },
                new ExecutionDataflowBlockOptions()
                {
                    CancellationToken = token,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                });

            AssetsToFetch.ForEach(asset => processor.Post(asset));

            processor.Complete();

            Task.WaitAny(new Task[] { processor.Completion, readKey });
        }

        private static async Task SaveTickFileAsync(
            TickFile tickFile, DataKind dataKind, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            await tickFile.SaveAsync(Default.TickDataPath, dataKind);

            logger.Info($"SAVED {tickFile}.{dataKind.ToString().ToLower()}...{tickFile.Count:N0} Ticks");
        }

        private static async Task<TickFile> ProcessEntryAsync(
            Asset asset, TickFile tickFile, ZipArchiveEntry entry, CancellationToken token)
        {
            string line;

            var skipSave = false;

            using (var reader = new StreamReader(entry.Open()))
            {
                if (token.IsCancellationRequested)
                    return null;

                while ((line = reader.ReadLine()) != null)
                {
                    var fields = line.Split(',');

                    var estTickOn = DateTime.ParseExact(fields[0], "yyyyMMdd HHmmssfff", null);

                    // HistData does not make DST adjustments, hence the "fixed" 5-hour adjustment
                    var utcTickOn = new DateTime(estTickOn.AddHours(5).Ticks, DateTimeKind.Utc);

                    var tickOn = utcTickOn.ToEstFromUtc();

                    if (!tickOn.IsTickOn())
                        continue;

                    var bidRate = asset.Round(double.Parse(fields[1]));

                    var askRate = asset.Round(double.Parse(fields[2]));

                    var tick = new Tick(asset, tickOn, bidRate, askRate);

                    if (tickFile == null)
                    {
                        skipSave = true;

                        tickFile = new TickFile(Source.HistData, asset, tick.BaseDate);
                    }
                    else if (tick.BaseDate > tickFile.BaseDate)
                    {
                        if (skipSave)
                        {
                            logger.Debug($"Skipped {tickFile}");
                        }
                        else
                        {
                            await SaveTickFileAsync(tickFile, DataKind.CSV, token);

                            if (token.IsCancellationRequested)
                                return null;

                            await SaveTickFileAsync(tickFile, DataKind.Ticks, token);

                            if (token.IsCancellationRequested)
                                return null;
                        }

                        skipSave = false;

                        tickFile = new TickFile(Source.HistData, asset, tick.BaseDate);
                    }
                    else if (tick.BaseDate < tickFile.BaseDate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(tickOn));
                    }

                    tickFile.Add(tick);
                }
            }

            return tickFile;
        }

        private static void FetchArchives(Task readKey, CancellationToken token)
        {
            var (fetchJobs, skipped) = GetFetchJobs();

            logger.Info($"FETCH-QUEUE: {fetchJobs.Count:N0} HistData Archives ({skipped:N0} Skipped)");

            var fetcher = new ActionBlock<FetchJob>(
                async job =>
                {
                    var (cancelled, bytesFetched) = await job.FetchAsync(token);

                    if (cancelled)
                        logger.Info($"FETCHED {job.ArchiveName} (Cancelled)");
                    else
                        logger.Info($"FETCHED {job.ArchiveName} ({bytesFetched:N0} bytes)");
                },
                new ExecutionDataflowBlockOptions()
                {
                    CancellationToken = token,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                });

            fetchJobs.ForEach(job => fetcher.Post(job));

            fetcher.Complete();

            Task.WaitAny(new Task[] { fetcher.Completion, readKey });
        }

        private static string BasePath => Path.Combine(
            Default.TickDataPath, Source.HistData.ToString(), "ARCHIVE") + "\\";

        private static List<ArchiveJob> GetArchiveJobs(List<Asset> assetsToFetch)
        {
            var minBaseDate = new DateTime(WellKnown.BaseDate.GetMinValue().Year, 1, 1);

            var date = DateTime.UtcNow.AddDays(-2).AddMonths(-1).ToEstFromUtc().Date;

            var maxBaseDate = date.AddDays(DateTime.DaysInMonth(date.Year, date.Month));

            var jobs = new List<ArchiveJob>();

            foreach (var asset in assetsToFetch)
            {
                for (var baseData = minBaseDate;
                    baseData <= maxBaseDate; baseData = baseData.AddMonths(1))
                {
                    jobs.Add(new ArchiveJob(asset, baseData.Year, baseData.Month));
                }
            }

            return jobs;
        }

        private static List<Asset> AssetsToFetch =>
            Default.AssetsToFetch.Split(',').Select(a => WellKnown.Assets[a.ToEnum<Symbol>()]).ToList();

        private static (List<FetchJob>, int) GetFetchJobs()
        {
            BasePath.EnsurePathExists();

            var archiveNames = new HashSet<string>(
                Directory.GetFiles(BasePath, ArchiveJob.SearchPattern,
                SearchOption.AllDirectories).Select(f => Path.GetFileName(f)));

            int skipped = 0;

            var fetchJobs = new List<FetchJob>();

            foreach (var archiveJob in GetArchiveJobs(AssetsToFetch))
            {
                if (archiveNames.Contains(archiveJob.ArchiveName))
                    skipped++;
                else
                    fetchJobs.Add(new FetchJob(archiveJob));
            }

            return (fetchJobs, skipped);
        }
    }
}
