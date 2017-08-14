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
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using TickData.Common.Helpers;

namespace TickData.Common.Trading
{
    internal static class TickStreamIO
    {
        public const string EntryName = "Ticks.data";

        private const float VERSION = 1.0f;
        private const int SYMBOLPADDING = 8;
        private const int SOURCEPADDING = 8;

        public static async Task SaveCsvToStreamAsync(IEnumerable<Tick> ticks, Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                var writer = new StreamWriter(memoryStream);

                foreach (var tick in ticks)
                    writer.WriteLine(tick.ToCsvString());

                writer.Flush();

                memoryStream.Position = 0;

                await memoryStream.CopyToAsync(stream);
            }
        }

        public static async Task SaveTicksToStreamAsync(Source source, Asset asset, 
            DateTime baseDate, IEnumerable<Tick> ticks, Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                SaveToZipStream(source, asset, baseDate, ticks.ToList(), memoryStream);

                memoryStream.Position = 0;

                await memoryStream.CopyToAsync(stream);
            }
        }

        public static void SaveToZipStream(
            Source source, Asset asset, DateTime baseDate, List<Tick> ticks, Stream stream)
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry(EntryName);

                using (var entryStream = entry.Open())
                {
                    var writer = new BinaryWriter(entryStream);

                    writer.Write(VERSION);
                    writer.Write(DateTime.UtcNow.Ticks);
                    writer.Write(source.ToCharArray(SOURCEPADDING));
                    writer.Write(asset.Symbol.ToCharArray(SYMBOLPADDING));
                    writer.Write(baseDate.Ticks);
                    writer.Write(new byte[24]);
                    writer.Write(ticks.Count);

                    foreach (var tick in ticks)
                        tick.Write(writer);

                    writer.Flush();
                }
            }
        }

        private static void LoadCheck(bool isValid, object kind)
        {
            if (!isValid)
            {
                throw new InvalidDataException(
                   $"The tick collection doesn't contain {kind} data, as expected!");
            }
        }

        public static List<Tick> LoadFromZipStream(
            Source source, Asset asset, DateTime baseDate, Stream stream)
        {
            var ticks = new List<Tick>();

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
            {
                if (archive.Entries.Count != 1)
                {
                    throw new InvalidDataException(
                        "The archive doesn't contain a single entry, as expected!");
                }

                var entry = archive.Entries[0];

                if (entry.Name != EntryName)
                {
                    throw new InvalidDataException(
                        $"The entry-name wasn't \"{EntryName},\" as expected.");
                }

                using (var entryStream = entry.Open())
                {
                    var reader = new BinaryReader(entryStream);

                    LoadCheck(reader.ReadSingle() == VERSION, $"v{VERSION:N1}");

                    var createdOn = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);

                    var sourceString = new string(reader.ReadChars(SOURCEPADDING)).Trim();

                    LoadCheck(Funcify.Lambda(
                        sourceString, s => s.ToEnum<Source>() == source), source);

                    var symbolString = new string(
                        reader.ReadChars(SYMBOLPADDING)).Trim();

                    LoadCheck(Funcify.Lambda(symbolString, s =>
                        WellKnown.Assets.ContainsKey(s.ToEnum<Symbol>())), asset.Symbol);

                    LoadCheck(new DateTime(reader.ReadInt64(),
                        DateTimeKind.Unspecified) == baseDate, $"{baseDate:MM/dd/yyyy}");

                    reader.ReadBytes(24);

                    var count = reader.ReadInt32();

                    for (int i = 0; i < count; i++)
                    {
                        var tick = Tick.Read(asset, reader);

                        if (tick.BaseDate != baseDate)
                            throw new ArgumentOutOfRangeException(nameof(tick));

                        if (ticks.Count >= 1 && tick.TickOn < ticks[ticks.Count - 1].TickOn)
                            throw new ArgumentOutOfRangeException(nameof(tick));

                        ticks.Add(tick);
                    }
                }
            }

            return ticks;
        }
    }
}
