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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickData.Common.Helpers;

namespace TickData.Common.Trading
{
    public class TickFile : ListBase<Tick>
    {
        public TickFile(Source source, Asset asset, DateTime baseDate)
        {
            if (!source.IsEnumValue())
                throw new ArgumentOutOfRangeException(nameof(source));

            Asset = asset ?? throw new ArgumentNullException(nameof(asset));

            if (!baseDate.IsBaseDate())
                throw new ArgumentOutOfRangeException(nameof(baseDate));

            Source = source;
            BaseDate = baseDate;

            NameOnly = GetNameOnly(source, asset, baseDate);
            MinTickOn = baseDate.AddHours(WellKnown.TickOn.FirstHour);
            MaxTickOn = MinTickOn.AddDays(1).AddMilliseconds(-1);
        }

        public Source Source { get; }
        public Asset Asset { get; }
        public DateTime BaseDate { get; }
        public string NameOnly { get; }
        public DateTime MinTickOn { get; }
        public DateTime MaxTickOn { get; }

        private static string GetNameOnly(Source source, Asset asset, DateTime baseDate)
        {
            var sb = new StringBuilder();

            sb.Append(source.ToString().ToUpper());
            sb.Append('_');
            sb.Append(asset);
            sb.Append($"_{baseDate.ToString("yyyyMMdd")}");
            sb.Append($"_{WellKnown.TickOn.FirstHour:00}_24_EST");

            return sb.ToString();
        }

        private static string GetFileName(
            Source source, Asset asset, DateTime baseDate, DataKind dataKind)
        {
            var sb = new StringBuilder();

            sb.Append(GetNameOnly(source, asset, baseDate));
            sb.Append('.');
            sb.Append(dataKind.ToString().ToLower());

            return sb.ToString();
        }

        private static string GetBlobName(
            Source source, Asset asset, DateTime baseDate, DataKind dataKind)
        {
            var sb = new StringBuilder();

            sb.Append(source.ToString().ToUpper());
            sb.Append('/');
            sb.Append(dataKind.ToString().ToUpper());
            sb.Append("/");
            sb.Append(asset);
            sb.Append('/');
            sb.Append(baseDate.Year);
            sb.Append('/');
            sb.Append(GetFileName(source, asset, baseDate, dataKind));

            return sb.ToString();
        }

        public void Add(Tick tick)
        {
            if (tick == null)
                throw new ArgumentNullException(nameof(tick));

            if (tick.Symbol != Asset.Symbol)
                throw new ArgumentOutOfRangeException(nameof(tick));

            if (tick.BaseDate != BaseDate)
                throw new ArgumentOutOfRangeException(nameof(tick));

            if (Count >= 1 && tick.TickOn < this[Count - 1].TickOn)
                throw new ArgumentOutOfRangeException(nameof(tick));

            Items.Add(tick);
        }

        public void AddRange(IEnumerable<Tick> ticks) =>
            ticks.ToList().ForEach(tick => Add(tick));

        public override string ToString() => NameOnly;

        private string GetSaveTo(string basePath, DataKind dataKind) => Path.GetFullPath(
            Path.Combine(basePath, GetBlobName(Source, Asset, BaseDate, dataKind)));

        public bool Exists(string basePath, DataKind dataKind) =>
            File.Exists(GetSaveTo(basePath, dataKind));

        private void LoadFromZipStream(Stream stream) =>
            Items = TickStreamIO.LoadFromZipStream(Source, Asset, BaseDate, stream);

        public async Task SaveAsync(string basePath, DataKind dataKind = DataKind.Ticks)
        {
            if (!basePath.IsDirectoryName(false))
                throw new ArgumentOutOfRangeException(nameof(basePath));

            if (!dataKind.IsEnumValue() || dataKind == DataKind.Archive)
                throw new ArgumentOutOfRangeException(nameof(dataKind));

            var saveTo = GetSaveTo(basePath, dataKind);

            saveTo.EnsurePathExists();

            using (var fileStream = File.OpenWrite(saveTo))
                await SaveAsync(fileStream, dataKind);
        }

        public async Task SaveAsync(Stream stream, DataKind dataKind = DataKind.Ticks)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            switch (dataKind)
            {
                case DataKind.CSV:
                    await TickStreamIO.SaveCsvToStreamAsync(this, stream);
                    break;
                case DataKind.Ticks:
                    await TickStreamIO.SaveTicksToStreamAsync(Source, Asset, BaseDate, this, stream);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataKind));
            }
        }

        public void Load(byte[] bytes)
        {
            Items.Clear();

            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            using (var stream = new MemoryStream(bytes))
                LoadFromZipStream(stream);
        }

        public void Load(Stream stream)
        {
            Items.Clear();

            LoadFromZipStream(stream);
        }

        // TODO: validate path!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public void Load(string basePath, DataKind dataKind)
        {
            Items.Clear();

            using (var stream = File.OpenRead(GetSaveTo(basePath, dataKind)))
                LoadFromZipStream(stream);
        }

        public byte[] ToArray()
        {
            byte[] bytes;

            using (var stream = new MemoryStream())
            {
                TickStreamIO.SaveToZipStream(
                    Source, Asset, BaseDate, Items, stream);

                bytes = stream.ToArray();
            }

            return bytes;
        }
    }
}
