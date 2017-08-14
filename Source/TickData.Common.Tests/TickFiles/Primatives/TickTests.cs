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
using TickData.Common.Helpers;
using static TickData.Common.Trading.WellKnown;

namespace TickData.Common.Trading.Tests
{
    [TestClass]
    public class TickTests
    {
        [TestMethod]
        public void EverySymbolCanBeConstructed() =>
            new EnumList<Symbol>().ForEach(s => GetGoodTick(s));

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullAssetThrowsError() =>
            new Tick(null, TickOn.MinValue, EurUsd.MinValue, EurUsd.MaxValue);

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TickOnLessThanMinValueThrowsError()
        {
            var tickOn = TickOn.MinValue.AddMilliseconds(-1);

            new Tick(EurUsd, tickOn, EurUsd.MinValue, EurUsd.MaxValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TickOnKindNotUnspecifiedThrowsError()
        {
            var tickOn = TickOn.MinValue.ToUniversalTime();

            new Tick(EurUsd, tickOn, EurUsd.MinValue, EurUsd.MaxValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void LessThanMinRateBidRateThrowsError()
        {
            var rate = EurUsd.Round(EurUsd.MinValue - EurUsd.OneTick);

            new Tick(EurUsd, TickOn.MinValue, rate, EurUsd.MaxValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void LessThanMinRateAskRateThrowsError()
        {
            var rate = EurUsd.Round(EurUsd.MinValue - EurUsd.OneTick);

            new Tick(EurUsd, TickOn.MinValue, EurUsd.MinValue, rate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MoreThanMaxRateBidRateThrowsError()
        {
            var rate = EurUsd.Round(EurUsd.MaxValue + EurUsd.OneTick);

            new Tick(EurUsd, TickOn.MinValue, rate, EurUsd.MaxValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MoreThanMaxRateAskRateThrowsError()
        {
            var rate = EurUsd.Round(EurUsd.MaxValue + EurUsd.OneTick);

            new Tick(EurUsd, TickOn.MinValue, EurUsd.MinValue, rate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void UnroundedEurUsdBidRateThrowsError() =>
            new Tick(EurUsd, TickOn.MinValue, EurUsd.MinValue + 0.000001, EurUsd.MaxValue);

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void UnroundedEurUsdAskRateThrowsError() =>
            new Tick(EurUsd, TickOn.MinValue, EurUsd.MinValue, EurUsd.MinValue + 0.000001);

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void UnroundedUsdJpyBidRateThrowsError() =>
            new Tick(UsdJpy, TickOn.MinValue, UsdJpy.MinValue + 0.0001, UsdJpy.MaxValue);

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void UnroundedUsdJpyAskRateThrowsError() =>
            new Tick(UsdJpy, TickOn.MinValue, UsdJpy.MinValue, UsdJpy.MinValue + 0.0001);

        private Asset EurUsd => Assets[Symbol.EURUSD];

        private Asset UsdJpy => Assets[Symbol.USDJPY];

        private Tick GetGoodTick(Symbol symbol)
        {
            var asset = Assets[symbol];

            return new Tick(asset, TickOn.MinValue, asset.MinValue, asset.MaxValue);
        }
    }
}
