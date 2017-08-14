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
using System.IO;
using System.Text;

namespace TickData.Common.Helpers
{
    public static class StringExtenders
    {
        public static T ToEnum<T>(this string value) => (T)Enum.Parse(typeof(T), value, true);

        public static string ToSingleLine(this string value, string delimiter = "; ")
        {
            var sb = new StringBuilder();

            var reader = new StringReader(value);

            string line;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                if (!string.IsNullOrEmpty(line))
                {
                    if (sb.Length > 0)
                        sb.Append(delimiter);

                    sb.Append(line);
                }
            }

            return sb.ToString();
        }

        public static void EnsurePathExists(this string path)
        {
            if (Path.IsPathRooted(path))
                path = Path.GetDirectoryName(path);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static bool IsDirectoryName(this string value, bool mustBeRooted = true)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            try
            {
                var dummy = new DirectoryInfo(value);

                return !mustBeRooted || Path.IsPathRooted(value);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (PathTooLongException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }
    }
}
