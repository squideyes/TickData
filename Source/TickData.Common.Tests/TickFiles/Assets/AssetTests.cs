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
using System;

namespace TickData.Common.Trading.Tests
{
    [TestClass]
    public class AssetTests
    {
        [DataTestMethod]
        [DataRow(Symbol.EURUSD, 5, AssetKind.Forex, 10000, 0.00001, 9.99999, 0.0001)]
        [DataRow(Symbol.USDJPY, 3, AssetKind.Forex, 100, 0.001, 999.999, 0.01)]

        public void GoodArgsConstructWithoutErrors(Symbol symbol, int precision,
            AssetKind kind, int factor, double minValue, double maxValue, double onePip)
        {
            CheckAsset(symbol, precision, kind, factor, minValue, maxValue, onePip);
        }

        [DataTestMethod]
        [DataRow((Symbol)0, 5, AssetKind.Forex, 10000, 0.00001, 9.99999, 0.0001, "XXX")]
        [DataRow(Symbol.EURUSD, 5, (AssetKind)0, 10000, 0.00001, 9.99999, 0.0001, "XXX")]
        [DataRow(Symbol.EURUSD, 2, AssetKind.Forex, 10000, 0.00001, 9.99999, 0.0001, "XXX")]
        [DataRow(Symbol.EURUSD, 4, AssetKind.Forex, 10000, 0.00001, 9.99999, 0.0001, "XXX")]
        [DataRow(Symbol.EURUSD, 6, AssetKind.Forex, 10000, 0.00001, 9.99999, 0.0001, "XXX")]
        [DataRow(Symbol.EURUSD, 5, AssetKind.Forex, 10000, 0.00001, 9.99999, 0.0001, " ")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BadArgsThrowConstructErrors(Symbol symbol, int precision, AssetKind kind,
            int factor, double minValue, double maxValue, double onePip, string description)
        {
            new Asset(symbol, kind, precision, description);
        }

        private void CheckAsset(Symbol symbol, int precision, AssetKind kind,
            int factor, double minValue, double maxValue, double onePip)
        {
            var asset = new Asset(symbol, kind, precision, "XXX");

            Assert.AreEqual(asset.Description, "XXX");
            Assert.AreEqual(asset.Factor, factor);
            Assert.AreEqual(asset.Kind, kind);
            Assert.AreEqual(asset.MaxValue, maxValue);
            Assert.AreEqual(asset.MinValue, minValue);
            Assert.AreEqual(asset.OnePip, onePip);
            Assert.AreEqual(asset.OneTick, minValue);
            Assert.AreEqual(asset.Precision, precision);
            Assert.AreEqual(asset.Symbol, symbol);
        }
    }
}
