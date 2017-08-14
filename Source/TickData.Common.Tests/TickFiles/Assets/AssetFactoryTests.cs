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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickData.Common.Helpers;

namespace TickData.Common.Trading.Tests
{
    [TestClass]
    public class AssetFactoryTests
    {
        [TestMethod]
        public void EverySymbolHasAndAsset()
        {
            var assets = AssetsFactory.GetAssets();

            foreach (var symbol in new EnumList<Symbol>())
                Assert.AreEqual(WellKnown.Assets[symbol].Symbol, symbol);
        }
    }
}
