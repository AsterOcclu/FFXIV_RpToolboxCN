using Lumina.Excel.GeneratedSheets;

namespace RoleplayersToolbox.Tools.Housing {
    internal class DestinationInfo {
        private HousingInfo Info { get; }
        private HousingArea? _area;
        private uint? _plot;

        public World? World { get; set; }

        public HousingArea? Area {
            get => this._area;
            set {
                this._area = value;
                this.CalculateClosest();
            }
        }

        public uint? Ward { get; set; }

        public uint? Plot {
            get => this._plot;
            set {
                this._plot = value;
                this.CalculateClosest();
            }
        }

        public HousingAethernet? ClosestAethernet { get; private set; }

        internal DestinationInfo(HousingInfo info, World? world, HousingArea? area, uint? ward, uint? plot) {
            this.Info = info;

            this.World = world;
            this.Area = area;
            this.Ward = ward;
            this.Plot = plot;
        }

        internal DestinationInfo(HousingInfo info) {
            this.Info = info;
        }

        private void CalculateClosest() {
            if (this.Area == null || this.Plot == null) {
                this.ClosestAethernet = null;
                return;
            }

            this.ClosestAethernet = this.Info.Distances.GetClosest(this.Area.Value, this.Plot.Value);
        }

        internal bool AnyNull() => this.World == null || this.Area == null || this.Ward == null || this.Plot == null;
    }
}
