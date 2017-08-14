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

using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using TickData.Common.Helpers;

namespace TickData.Common.Trading
{
    public static class AssetsFactory
    {
        public static ReadOnlyDictionary<Symbol, Asset> GetAssets()
        {
            var doc = XDocument.Parse(Properties.Resources.Assets);

            var q = from a in doc.Element("assets").Elements("asset")
                    select new
                    {
                        Symbol = a.Attribute("symbol").Value.ToEnum<Symbol>(),
                        Kind = a.Attribute("kind").Value.ToEnum<AssetKind>(),
                        Precision = (int)a.Attribute("precision"),
                        Description = a.Attribute("description").Value
                    };

            var assets = q.Select(a => new Asset(
                a.Symbol, a.Kind, a.Precision, a.Description)).ToList();

            return new ReadOnlyDictionary<Symbol, Asset>(
                assets.ToDictionary(a => a.Symbol));
        }
    }
}
