using System;
using System.Collections.Generic;
using Dalamud.Data;
using Dalamud.Logging;
using Lumina.Data.Files;
using Lumina.Data.Parsing.Layer;
using Lumina.Excel.GeneratedSheets;

namespace RoleplayersToolbox.Tools.Housing {
    internal class HousingInfo {
        private DataManager Data { get; }
        private Dictionary<uint, LayerCommon.InstanceObject> LgbObjects { get; } = new();
        internal HousingDistances Distances { get; }

        internal HousingInfo(Plugin plugin) {
            this.Data = plugin.DataManager;
            this.Distances = this.PrecalculateClosest();
        }

        private HousingAethernet? CalculateClosest(HousingArea area, uint plot) {
            // subtract 1 from the subrow because Lumina is zero-indexed even though the sheet isn't
            var info = this.Data.GetExcelSheet<HousingMapMarkerInfo>()!.GetRow((uint) area, plot - 1);
            if (info == null) {
                return null;
            }

            var (x, y, z) = (info.X, info.Y, info.Z);

            (HousingAethernet aethernet, double distance)? shortest = null;
            foreach (var aethernet in this.Data.GetExcelSheet<HousingAethernet>()!) {
                if (aethernet.TerritoryType.Row != (uint) area) {
                    continue;
                }

                var level = aethernet.Level.Row;
                if (!this.LgbObjects.TryGetValue(level, out var obj)) {
                    continue;
                }

                var translation = obj.Transform.Translation;
                var xDiff = translation.X - x;
                var yDiff = translation.Y - y;
                var zDiff = translation.Z - z;

                var sumOfSquares = Math.Pow(xDiff, 2) + Math.Pow(yDiff, 2) + Math.Pow(zDiff, 2);
                var distance = Math.Sqrt(sumOfSquares);

                if (shortest == null || shortest.Value.distance > distance) {
                    shortest = (aethernet, distance);
                }
            }

            return shortest?.aethernet;
        }

        private HousingDistances PrecalculateClosest() {
            var allClosest = new Dictionary<HousingArea, Dictionary<uint, HousingAethernet>>();

            foreach (var area in (HousingArea[]) Enum.GetValues(typeof(HousingArea))) {
                this.LoadObjectsFromArea(area);

                for (var plot = 1u; plot < 63; plot++) {
                    var closest = this.CalculateClosest(area, plot);
                    if (closest == null) {
                        continue;
                    }

                    if (!allClosest.ContainsKey(area)) {
                        allClosest[area] = new Dictionary<uint, HousingAethernet>();
                    }

                    allClosest[area][plot] = closest;
                }
            }

            return new HousingDistances(this.Data, allClosest);
        }

        private LgbFile? GetLgbFromPath(string path) {
            try {
                return this.Data.GameData.GetFile<LgbFile>(path);
            } catch (Exception ex) {
                PluginLog.LogError(ex, $"Error reading lgb file: {path}");
                return null;
            }
        }

        private LgbFile? GetLgbFromArea(HousingArea area) {
            var territory = this.Data.GetExcelSheet<TerritoryType>()!.GetRow((uint) area);
            if (territory == null) {
                return null;
            }

            var path = territory.Bg.ToString();
            path = path[..path.LastIndexOf('/')];
            return this.GetLgbFromPath($"bg/{path}/planmap.lgb");
        }

        private void LoadObjectsFromFile(LgbFile lgb) {
            foreach (var layer in lgb.Layers) {
                foreach (var obj in layer.InstanceObjects) {
                    this.LgbObjects[obj.InstanceId] = obj;
                }
            }
        }

        internal void LoadObjectsFromPath(string path) {
            var lgb = this.GetLgbFromPath(path);
            if (lgb == null) {
                return;
            }

            this.LoadObjectsFromFile(lgb);
        }

        private void LoadObjectsFromArea(HousingArea area) {
            var lgb = this.GetLgbFromArea(area);
            if (lgb == null) {
                return;
            }

            this.LoadObjectsFromFile(lgb);
        }
    }
}
