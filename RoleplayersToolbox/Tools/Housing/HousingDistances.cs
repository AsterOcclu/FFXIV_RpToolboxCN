using System.Collections.Generic;
using Dalamud.Data;
using Lumina.Excel.GeneratedSheets;

namespace RoleplayersToolbox.Tools.Housing {
    internal class HousingDistances {
        private static Dictionary<HousingArea, Dictionary<uint, uint>> Overrides { get; } = new() {
            [HousingArea.LavenderBeds] = new() {
                [14] = 1966102, // Lavender East
                [15] = 1966102,
                [44] = 1966110, // Lavender South Subdivision
                [45] = 1966110,
            },
            [HousingArea.Shirogane] = new() {
                [5] = 1966135, // Southern Shirogane
            },
        };

        private DataManager Data { get; }
        private Dictionary<HousingArea, Dictionary<uint, HousingAethernet>> Closest { get; }

        public HousingDistances(DataManager data, Dictionary<HousingArea, Dictionary<uint, HousingAethernet>> closest) {
            this.Data = data;
            this.Closest = closest;
        }

        internal HousingAethernet? GetClosest(HousingArea area, uint plot) {
            if (Overrides.TryGetValue(area, out var overridePlots)) {
                if (overridePlots.TryGetValue(plot, out var overrideId)) {
                    var overrideAethernet = this.Data.GetExcelSheet<HousingAethernet>()!.GetRow(overrideId);
                    if (overrideAethernet != null) {
                        return overrideAethernet;
                    }
                }
            }

            if (!this.Closest.TryGetValue(area, out var plots)) {
                return null;
            }

            return plots.TryGetValue(plot, out var aethernet) ? aethernet : null;
        }
    }
}
