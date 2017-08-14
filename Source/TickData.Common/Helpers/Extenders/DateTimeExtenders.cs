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

using NodaTime;
using System;

namespace TickData.Common.Helpers
{
    public static class DateTimeExtenders
    {
        private static readonly DateTimeZone estZone =
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("America/New_York");

        public static DateTime ToEstFromUtc(this DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
                throw new ArgumentOutOfRangeException(nameof(value));

            return Instant.FromDateTimeUtc(value)
                .InZone(estZone).ToDateTimeUnspecified();
        }

        public static string ToText(this DateTime value) =>
            value.ToString("MM/dd/yyyy HH:mm:ss.fff");
    }
}
