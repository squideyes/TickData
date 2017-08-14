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
    public static class DoubleExtenders
    {
        public static bool IsPips(this double value) =>
            (value >= 0.1 && value <= 999.9 && Math.Round(value, 1) == value);

        public static double PipsToRate(this double value, Asset asset) =>
            asset.Round(value / asset.Factor);

        public static double RateToPips(this double value, Asset asset) =>
            Math.Round(value * asset.Factor, 1);
    }
}
