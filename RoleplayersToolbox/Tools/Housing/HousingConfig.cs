using System;
using System.Collections.Generic;
using RoleplayersToolbox.Tools.Housing;

namespace RoleplayersToolbox.Tools {
    internal partial class ToolConfig {
        public HousingConfig Housing { get; set; } = new();
    }
}

namespace RoleplayersToolbox.Tools.Housing {
    [Serializable]
    internal class HousingConfig {
        public bool PlaceFlagOnSelect = true;
        public bool CloseMapOnApproach = true;
        public bool ClearFlagOnApproach = true;
        public List<Bookmark> Bookmarks = new();
    }
}
