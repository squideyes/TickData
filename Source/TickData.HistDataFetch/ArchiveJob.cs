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

using System.IO;
using System.Text;
using TickData.Common.Trading;

namespace TickData.HistDataFetch
{
    internal class ArchiveJob
    {
        public const string SearchPattern = "DAT_ASCII_??????_T_20????.zip";

        public ArchiveJob(Asset asset, int year, int month)
        {
            Asset = asset;
            Year = year;
            Month = month;
        }

        public Asset Asset { get; }
        public int Year { get; }
        public int Month { get; }

        public string ArchiveName => $"DAT_ASCII_{Asset}_T_{Year}{Month:00}.zip";
        public string EntryName => $"DAT_ASCII_{Asset}_T_{Year}{Month:00}.csv";

        public string FullPath
        {
            get
            {
                var sb = new StringBuilder();

                sb.Append(Source.HistData.ToString().ToUpper());
                sb.Append(Path.DirectorySeparatorChar);
                sb.Append(DataKind.Archive.ToString().ToUpper());
                sb.Append(Path.DirectorySeparatorChar);
                sb.Append(Asset);
                sb.Append(Path.DirectorySeparatorChar);
                sb.Append(Year);
                sb.Append(Path.DirectorySeparatorChar);
                sb.Append(ArchiveName);

                return Path.GetFullPath(Path.Combine(
                    Properties.Settings.Default.TickDataPath, sb.ToString()));
            }
        }

        public override string ToString() => $"{Asset} {Month:00}/{Year}";
    }
}
