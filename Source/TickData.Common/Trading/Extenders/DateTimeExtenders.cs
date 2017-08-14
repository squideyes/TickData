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

namespace TickData.Common.Trading
{
    public static class DateTimeExtenders
    {
        public static bool IsBaseDate(this DateTime value)
        {
            if (value.Kind != DateTimeKind.Unspecified)
                return false;

            if (value.TimeOfDay != TimeSpan.Zero)
                return false;

            if (value < WellKnown.BaseDate.GetMinValue())
                return false;

            if (value > WellKnown.BaseDate.GetMaxValue())
                return false;

            return value.CanTradeOn();
        }

        public static bool CanTradeOn(this DateTime value)
        {
            switch (value.DayOfWeek)
            {
                case DayOfWeek.Friday:
                case DayOfWeek.Saturday:
                    return false;
            }

            if (value.Month == 1)
            {
                switch (value.Day)
                {
                    case 1:
                    case 2:
                        return false;
                }
            }
            else if (value.Month == 12)
            {
                switch (value.Day)
                {
                    case 24:
                    case 25:
                    case 26:
                    case 31:
                        return false;
                }
            }

            return true;
        }

        public static DateTime ToBaseDate(this DateTime value)
        {
            return value.Hour >= WellKnown.TickOn.FirstHour ?
                value.Date : value.Date.AddDays(-1);
        }

        public static bool IsTickOn(this DateTime value)
        {
            if (!value.ToBaseDate().IsBaseDate())
                return false;

            switch (value.DayOfWeek)
            {
                case DayOfWeek.Friday:
                    return value.Hour < WellKnown.TickOn.FirstHour;
                case DayOfWeek.Saturday:
                    return false;
                case DayOfWeek.Sunday:
                    return value.Hour >= WellKnown.TickOn.FirstHour;
                default:
                    return true;
            }
        }
    }
}
