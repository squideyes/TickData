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
using System.IO;

namespace TickData.Common.Trading
{
    public class Tick : IEquatable<Tick>
    {
        public Tick(Asset asset, DateTime tickOn, double bidRate, double askRate)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            if (!tickOn.IsTickOn())
                throw new ArgumentOutOfRangeException(nameof(tickOn));

            if (!asset.IsRate(bidRate))
                throw new ArgumentOutOfRangeException(nameof(bidRate));

            if (!asset.IsRate(askRate))
                throw new ArgumentOutOfRangeException(nameof(askRate));

            Symbol = asset.Symbol;
            TickOn = tickOn;
            BidRate = bidRate;
            AskRate = askRate;
            MidRate = asset.Round((bidRate + askRate) / 2);
        }

        public Symbol Symbol { get; }
        public DateTime TickOn { get; }
        public double BidRate { get; }
        public double MidRate { get; }
        public double AskRate { get; }

        public DateTime BaseDate => TickOn.ToBaseDate();

        public void Write(BinaryWriter writer)
        {
            writer.Write(TickOn.Ticks);
            writer.Write((float)BidRate);
            writer.Write((float)AskRate);
        }

        public static Tick Read(Asset asset, BinaryReader reader)
        {
            var tickOn = new DateTime(reader.ReadInt64(), DateTimeKind.Unspecified);
            var bidRate = asset.Round(reader.ReadSingle());
            var askRate = asset.Round(reader.ReadSingle());

            return new Tick(asset, tickOn, bidRate, askRate);
        }

        public bool Equals(Tick other)
        {
            if (ReferenceEquals(null, other))
                return false;

            return Symbol == other.Symbol
                && TickOn.Equals(other.TickOn)
                && BidRate.Equals(other.BidRate)
                && MidRate.Equals(other.MidRate)
                && AskRate.Equals(other.AskRate);
        }

        public override bool Equals(object other)
        {
            if (!ReferenceEquals(null, this))
                return false;

            if (!ReferenceEquals(other, this))
                return false;

            if (other.GetType() != typeof(Tick))
                return false;

            return Equals((Tick)other);
        }

        public override int GetHashCode()
        {
            return Symbol.GetHashCode()
                ^ TickOn.GetHashCode()
                ^ BidRate.GetHashCode()
                ^ MidRate.GetHashCode()
                ^ AskRate.GetHashCode();
        }

        public override string ToString() => ToCsvString();

        public string ToCsvString()
        {
            var asset = WellKnown.Assets[Symbol];

            return $"{Symbol},{TickOn.ToText()},{asset.Format(BidRate)},{asset.Format(AskRate)}";
        }

        public static bool operator ==(Tick a, Tick b) => Equals(a, b);

        public static bool operator !=(Tick a, Tick b) => !Equals(a, b);
    }
}
