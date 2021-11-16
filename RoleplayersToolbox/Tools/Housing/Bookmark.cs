using System;

namespace RoleplayersToolbox.Tools.Housing {
    [Serializable]
    internal class Bookmark {
        public string Name;
        public uint WorldId;
        public HousingArea Area;
        public uint Ward;
        public uint Plot;

        public Bookmark(string name) {
            this.Name = name;
        }

        internal Bookmark Clone() {
            return new(this.Name) {
                WorldId = this.WorldId,
                Area = this.Area,
                Ward = this.Ward,
                Plot = this.Plot,
            };
        }

        internal bool AnyZero() {
            return string.IsNullOrWhiteSpace(this.Name)
                   || this.WorldId == 0
                   || this.Area == 0
                   || this.Ward == 0
                   || this.Plot == 0;
        }
    }
}
