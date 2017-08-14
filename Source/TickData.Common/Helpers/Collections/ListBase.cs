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
using System.Collections;
using System.Collections.Generic;

namespace TickData.Common.Helpers
{
    public abstract class ListBase<T> : IEnumerable<T>
    {
        protected List<T> Items = new List<T>();

        public int Count => Items.Count;

        public T this[int index] => Items[index];

        public bool HasElements(Func<T, bool> isValid = null) =>
            Items.HasElements(isValid);

        public void ForEach(Action<T> action) => 
            Items.ForEach(i => action(i));

        public IEnumerator<T> GetEnumerator() => 
            Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
