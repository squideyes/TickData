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
using TickData.Common.Helpers;
using System.Collections.ObjectModel;

namespace TickData.Common.Trading
{
    public static class WellKnown
    {
        public static class BaseDate
        {
            private static int minYear = 2012;

            public static int MinYear
            {
                get
                {
                    return minYear;
                }
                set
                {
                    if (value < 2010 || value > GetMaxValue().Year)
                        throw new ArgumentOutOfRangeException(nameof(MinYear));

                    minYear = value;
                }
            }

            public static DateTime GetMinValue()
            {
                var minValue = new DateTime(
                    MinYear, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

                while (!minValue.CanTradeOn())
                    minValue = minValue.AddDays(1);

                return minValue;
            }

            public static DateTime GetMaxValue()
            {
                var maxValue = DateTime.UtcNow.ToEstFromUtc().Date;

                while (!maxValue.CanTradeOn())
                    maxValue = maxValue.AddDays(-1);

                return maxValue;
            }
        }

        public static class TickOn
        {
            public const int FirstHour = 17;

            public static DateTime MinValue = BaseDate.GetMinValue().AddHours(FirstHour);
        }

        public static ReadOnlyDictionary<Symbol, Asset> Assets { get; } = AssetsFactory.GetAssets();
    }
}
