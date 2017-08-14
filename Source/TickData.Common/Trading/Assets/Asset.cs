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

using TickData.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace TickData.Common.Trading
{
    public class Asset : IEquatable<Asset>
    {
        private string format = null;

        internal Asset(Symbol symbol, AssetKind kind, int precision, string description)
        {
            if (!symbol.IsEnumValue())
                throw new ArgumentOutOfRangeException(nameof(Symbol));

            if (!kind.IsEnumValue())
                throw new ArgumentOutOfRangeException(nameof(Kind));

            if (kind == AssetKind.Forex)
            {
                if (precision < 3 || precision == 4 || precision > 5)
                    throw new ArgumentOutOfRangeException(nameof(precision));
            }
            else
            {
                if (precision < 0 || precision > 5)
                    throw new ArgumentOutOfRangeException(nameof(precision));
            }

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentOutOfRangeException(nameof(description));

            Kind = kind;
            Symbol = symbol;
            Description = description;
            Precision = precision;

            format = "N" + precision;

            OneTick = Math.Round(1.0 / Math.Pow(10.0, precision), precision);
            OnePip = Math.Round(OneTick * 10.0, Math.Max(0, precision - 1));
            Factor = Math.Max(Math.Pow(10.0, precision - 1), 1);
            MaxValue = Math.Round(Math.Pow(10.0, 6 - precision) - OneTick, precision);
        }

        [JsonProperty(PropertyName ="kind")]
        [JsonConverter(typeof(StringEnumConverter))]
        public AssetKind Kind { get; }

        [JsonProperty(PropertyName = "symbol")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Symbol Symbol { get; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; }

        [JsonProperty(PropertyName = "precision")]
        public int Precision { get; }

        [JsonProperty(PropertyName = "oneTick")]
        public double OneTick { get; private set; }

        [JsonProperty(PropertyName = "onePip")]
        public double OnePip { get; private set; }

        [JsonProperty(PropertyName = "factor")]
        public double Factor { get; private set; }

        [JsonProperty(PropertyName = "maxValue")]
        public double MaxValue { get; private set; }

        [JsonProperty(PropertyName = "minValue")]
        public double MinValue => OneTick;

        public override string ToString() => Symbol.ToString();

        public override int GetHashCode() => Symbol.GetHashCode();

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (other.GetType() != GetType())
                return false;

            return Symbol.Equals(((Asset)other).Symbol);
        }

        public bool Equals(Asset other)
        {
            if (ReferenceEquals(null, other))
                return false;

            return Symbol.Equals(other.Symbol);
        }

        public double Round(double rate) => Math.Round(rate, Precision);

        public string Format(double rate) => rate.ToString(format);

        public bool IsRate(double rate)
        {
            return (rate >= MinValue) && (rate <= MaxValue)
                && Math.Round(rate, Precision) == rate;
        }

        public static bool operator ==(Asset a, Asset b) => Equals(a, b);

        public static bool operator !=(Asset a, Asset b) => !Equals(a, b);
    }
}
