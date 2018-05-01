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

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TickData.Common.Helpers;
using TickData.Common.Trading;
using System.Linq;

namespace TickData.HistDataFetch
{
    internal class FetchJob : ArchiveJob
    {
        public FetchJob(ArchiveJob archiveJob)
            : base(archiveJob.Asset, archiveJob.Year, archiveJob.Month)
        {
        }
        private static async Task<string> GetTkAsync(
            HttpClient client, Uri pageUri, CancellationToken token)
        {
            var response = await client.GetAsync(pageUri, token);

            if (token.IsCancellationRequested)
                return null;

            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();

            doc.LoadHtml(html);

            var tk = doc.DocumentNode.SelectNodes("//input")
                .Where(x => x.Attributes["id"].Value == "tk")
                .Select(x => x.Attributes["value"].Value)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(tk))
                throw new Exception("A \"tk\" form-data value could not be found!");

            return tk;
        }

        public async Task<(bool, int)> FetchAsync(CancellationToken token)
        {
            const string BASEURI = "http://www.histdata.com";

            const string PREFIX =
                "download-free-forex-historical-data/?/ascii/tick-data-quotes";

            var pageUri = new Uri($"{BASEURI}/{PREFIX}/{Asset}/{Year}/{Month}");

            var client = new HttpClient();

            client.DefaultRequestHeaders.Referrer = pageUri;

            var entryName = Path.GetFileName(FullPath);

            FullPath.EnsurePathExists();

            if (File.Exists(FullPath))
                throw new Exception($"The \"{entryName}\" file unexpectedly exists!");

            var tk = await GetTkAsync(client, pageUri, token);

            if (token.IsCancellationRequested)
                return (true, -1);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("tk", tk),
                new KeyValuePair<string, string>("date", Year.ToString()),
                new KeyValuePair<string, string>("datemonth", $"{Year}{Month:00}"),
                new KeyValuePair<string, string>("platform", "ASCII"),
                new KeyValuePair<string, string>("timeframe", "T"),
                new KeyValuePair<string, string>("fxpair", Asset.ToString())
            });

            var response = await client.PostAsync(
                new Uri($"{BASEURI}/get.php"), content, token);

            if (token.IsCancellationRequested)
                return (true, -1);

            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync();

            if (token.IsCancellationRequested)
                return (true, -1);

            using (var fileStream = File.OpenWrite(FullPath))
                await fileStream.WriteAsync(bytes, 0, bytes.Length);

            return (false, bytes.Length);
        }
    }
}
