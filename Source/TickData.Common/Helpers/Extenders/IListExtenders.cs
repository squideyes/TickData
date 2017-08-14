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
using System.Linq;

namespace TickData.Common.Helpers
{
    public static class IListExtenders
    {
        public static bool HasElements<T>(
            this IList<T> items, Func<T, bool> isValid = null)
        {
            if (items == null)
                return false;

            if (items.Count == 0)
                return false;

            if (items.Any(i => i.Equals(default(T))))
                return false;

            if (isValid != null && !items.All(i => isValid(i)))
                return false;

            return true;
        }
    }
}
