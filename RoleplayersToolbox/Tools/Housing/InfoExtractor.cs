using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Data;
using F23.StringSimilarity;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Logging;


namespace RoleplayersToolbox.Tools.Housing {
    internal static class InfoExtractor {
        private static readonly IReadOnlyDictionary<HousingArea, Regex[]> HousingAreaNames = new Dictionary<HousingArea, Regex[]> {
            [HousingArea.LavenderBeds] = new[] {
                new Regex(@"\blavender beds\b", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"\blb\b", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"\blav\s?beds\b", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"\blav\b", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"\blav\s?b\b", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"ラベンダーベッド", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"薰衣草", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"森都[\s\W]*\d", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"森[\s\W]*\d", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            [HousingArea.Goblet] = new[] {
                new Regex(@"\bgoblet\b", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"\bgob\b", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"ゴブレットビュート", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"高脚孤丘", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"沙都[\s\W]*\d", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"沙[\s\W]*\d", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            [HousingArea.Mist] = new[] {
                new Regex(@"\bmist\b", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"\bmists\b", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"ミスト", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"海雾村", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"海都[\s\W]*\d", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"海[\s\W]*\d", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"水都[\s\W]*\d", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"水[\s\W]*\d", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            [HousingArea.Shirogane] = new[] {
                new Regex(@"\bshirogane\b", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"\bshiro\b", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"シロガネ", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"白银乡", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"白银[\s\W]*\d", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"白[\s\W]*\d", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
        };

        private static readonly JaroWinkler JaroWinkler = new();

        // @"(?<!sf)w(?:ard)?\W{0,2}(\d{1,2})\W{0,2}(?<!r)p(?:lot)?\W{0,2}(\d{1,2})"
        private static readonly Regex CombinedWardPlot = new(@"(\d{1,2})\s*[区/./-]+\s*(\d{1,2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        // @"(?<!sf)w(?:ard)?\W{0,2}(\d{1,2})"
        private static readonly Regex WardOnly = new(@"(\d{1,2})区", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // @"(?<!r)p(?:lot)?\W{0,2}(\d{1,2})"
        private static readonly Regex PlotOnly = new(@"(\d{1,2})号", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex DesperationCombined = new(@"(\d{1,2})\W{1,2}(\d{1,2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static DestinationInfo Extract(string source, uint dataCentre, DataManager data, HousingInfo info) {
            var world = FindWorld(source, dataCentre, data);
            var area = FindHousingArea(source);
            var (ward, plot) = FindWardPlot(source);
            return new DestinationInfo(info, world, area, ward, plot);
        }

        private static readonly Regex NonWord = new(@"\W", RegexOptions.Compiled);

        private static World? FindWorld(string source, uint dataCentre, DataManager data) {
            var worlds = Utils4CN.ChineseServers.GetAllWorldsByDcRow(dataCentre, data);
            foreach (var w in worlds)
            {
                var wName = w.Name.ToString();
                var keyWord = (string)wName.Substring(0, 2);
                if (source.IndexOf(keyWord) >= 0) {
                    return w;
                }
            }

            var words = NonWord.Split(source).Where(word => word.ToLowerInvariant() != "gg").ToArray();
            var mostSimilar = data.Excel.GetSheet<World>()!
                .Where(world => world.DataCenter.Row == dataCentre)
                .SelectMany(world => {
                    var name = world.Name.ToString().ToLowerInvariant();
                    return words.Select(word => (world, JaroWinkler.Similarity(name, word.ToLowerInvariant())));
                })
                .Where(entry => entry.Item2 > 0.75)
                .OrderByDescending(entry => entry.Item2)
                .FirstOrDefault();
            return mostSimilar == default ? null : mostSimilar.world;
        }

        private static HousingArea? FindHousingArea(string source) {
            foreach (var entry in HousingAreaNames) {
                if (entry.Value.Any(regex => regex.IsMatch(source))) {
                    return entry.Key;
                }
            }

            return null;
        }

        private static (uint? ward, uint? plot) FindWardPlot(string source) {
            var combined = CombinedWardPlot.Match(source);

            string? wardStr = null;
            string? plotStr = null;
            if (combined.Groups.Count == 3) {
                wardStr = combined.Groups[1].Captures[0].Value;
                plotStr = combined.Groups[2].Captures[0].Value;
                goto Parse;
            }

            var wardOnly = WardOnly.Match(source);
            if (wardOnly.Groups.Count == 2) {
                wardStr = wardOnly.Groups[1].Captures[0].Value;
            }

            var plotOnly = PlotOnly.Match(source);
            if (plotOnly.Groups.Count == 2) {
                plotStr = plotOnly.Groups[1].Captures[0].Value;
            }

            if (wardStr == null && plotStr == null) {
                var desperation = DesperationCombined.Match(source);
                if (desperation.Groups.Count == 3) {
                    wardStr = desperation.Groups[1].Captures[0].Value;
                    plotStr = desperation.Groups[2].Captures[0].Value;
                }
            }

            Parse:
            uint? ward = null;
            uint? plot = null;

            if (wardStr != null && uint.TryParse(wardStr, out var w)) {
                ward = w;
            }

            if (plotStr != null && uint.TryParse(plotStr, out var p)) {
                plot = p;
            }

            return (ward, plot);
        }
    }
}
