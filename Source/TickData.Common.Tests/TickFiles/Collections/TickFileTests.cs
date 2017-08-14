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
using System.IO;
using System.Threading.Tasks;
using static TickData.Common.Trading.WellKnown;
using PR = TickData.Common.Tests.Properties.Resources;

namespace TickData.Common.Trading.Tests
{
    [TestClass]
    public class TickFileTests
    {
        private TickFile GetGoodTickFile(Symbol symbol, DateTime baseDate) =>
            new TickFile(Source.HistData, Assets[symbol], baseDate);

        [TestMethod]
        public async Task EurUsdRoundtrip() => await LoadTickSetFromResource(
            Symbol.EURUSD, PR.HISTDATA_EURUSD_20120104_17_24_EST, 73622);

        [TestMethod]
        public async Task UsdJpyRoundtrip() => await LoadTickSetFromResource(
            Symbol.USDJPY, PR.HISTDATA_USDJPY_20120104_17_24_EST, 22087);

        private async Task LoadTickSetFromResource(Symbol symbol, byte[] data, int count)
        {
            var asset = WellKnown.Assets[symbol];
            var baseDate = new DateTime(2012, 1, 4, 0, 0, 0, DateTimeKind.Unspecified);

            var source = new TickFile(Source.HistData, asset, baseDate);
            var target = new TickFile(Source.HistData, asset, baseDate);

            Assert.AreEqual(source.Source, Source.HistData);
            Assert.AreEqual(source.Asset, asset);
            Assert.AreEqual(source.BaseDate, baseDate);
            Assert.AreEqual(source.MinTickOn, baseDate.AddHours(17));
            Assert.AreEqual(source.MaxTickOn,
                baseDate.AddDays(1).AddHours(17).AddMilliseconds(-1));
            Assert.AreEqual(source.NameOnly,
                $"HISTDATA_{symbol}_20120104_17_24_EST");

            source.Load(data);

            Assert.AreEqual(source.Count, count);

            using (var stream = new MemoryStream())
            {
                await source.SaveAsync(stream, DataKind.Ticks);

                stream.Position = 0;

                target.Load(stream);
            }

            Assert.AreEqual(source.Source, target.Source);
            Assert.AreEqual(source.Asset, target.Asset);
            Assert.AreEqual(source.BaseDate, target.BaseDate);
            Assert.AreEqual(source.MinTickOn, target.MinTickOn);
            Assert.AreEqual(source.MaxTickOn, target.MaxTickOn);
            Assert.AreEqual(source.NameOnly, target.NameOnly);
        }

        [TestMethod]
        public void PropertiesSetOnGoodConstruct()
        {
            var asset = Assets[Symbol.EURUSD];
            var baseDate = TickOn.MinValue.Date;

            var tickFile = GetGoodTickFile(asset.Symbol, baseDate);

            Assert.AreEqual(tickFile.Source, Source.HistData);
            Assert.AreEqual(tickFile.Asset, asset);
            Assert.AreEqual(tickFile.BaseDate, baseDate);
        }

        [TestMethod]
        public void AddFiveGoodTicks()
        {
            var tickFile = GetGoodTickFile(Symbol.EURUSD, TickOn.MinValue.Date);

            Tick tick = null;

            for (int i = 0; i < 5; i++)
            {
                tickFile.Add(tick = new Tick(tickFile.Asset,
                    tickFile.MinTickOn.AddMilliseconds(i),
                    tickFile.Asset.MinValue, tickFile.Asset.MaxValue));
            }

            Assert.AreEqual(tickFile.Count, 5);
            Assert.AreEqual(tickFile[4], tick);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTickThrowsErrorOnAdd() =>
            GetGoodTickFile(Symbol.EURUSD, TickOn.MinValue.Date).Add(null);

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BadBaseDateTickThrowsErrorOnAdd()
        {
            var tickFile = GetGoodTickFile(Symbol.EURUSD, TickOn.MinValue.Date);

            var tick = new Tick(tickFile.Asset, tickFile.MinTickOn.AddDays(1),
                tickFile.Asset.MinValue, tickFile.Asset.MaxValue);

            tickFile.Add(tick);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WrongSymbolTickThrowsErrorOnAdd()
        {
            var tickFile = GetGoodTickFile(Symbol.EURUSD, TickOn.MinValue.Date);

            var usdJpy = Assets[Symbol.USDJPY];

            var tick = new Tick(usdJpy,
                tickFile.MinTickOn, usdJpy.MinValue, usdJpy.MaxValue);

            tickFile.Add(tick);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void OutOfOrderTickThrowErrorOnAdd()
        {
            var tickFile = GetGoodTickFile(Symbol.EURUSD, TickOn.MinValue.Date);

            for (int i = 0; i < 2; i++)
            {
                tickFile.Add(new Tick(tickFile.Asset,
                    tickFile.MinTickOn.AddMilliseconds(2 - i),
                    tickFile.Asset.MinValue, tickFile.Asset.MaxValue));
            }
        }

        [TestMethod]
        public void RoundTripViaStream()
        {
            var source = GetGoodTickFile(Symbol.EURUSD, TickOn.MinValue.Date);
            var target = GetGoodTickFile(source.Asset.Symbol, source.BaseDate);

            for (int i = 0; i < 5; i++)
            {
                source.Add(new Tick(source.Asset,
                    source.MinTickOn.AddMilliseconds(i),
                    source.Asset.MinValue, source.Asset.MaxValue));
            }

            using (var stream = new MemoryStream())
            {
                source.SaveAsync(stream).Wait();

                stream.Position = 0;

                target.Load(stream);
            };

            Assert.AreEqual(source.Source, target.Source);
            Assert.AreEqual(source.Asset, target.Asset);
            Assert.AreEqual(source.BaseDate, target.BaseDate);
            Assert.AreEqual(source.NameOnly, target.NameOnly);
            Assert.AreEqual(source.ToString(), target.ToString());
            Assert.AreEqual(source.Count, target.Count);
            Assert.AreEqual(source.MinTickOn, target.MinTickOn);
            Assert.AreEqual(source.MaxTickOn, target.MaxTickOn);

            for (int i = 0; i < source.Count; i++)
                Assert.IsTrue(source[i].Equals(target[i]));
        }
    }
}
