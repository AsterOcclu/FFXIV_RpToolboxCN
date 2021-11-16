#if ILLEGAL

using System;
using RoleplayersToolbox.Tools.Illegal.EmoteSnap;

namespace RoleplayersToolbox.Tools {
    internal partial class ToolConfig {
        public EmoteSnapConfig EmoteSnap { get; set; } = new();
    }
}

namespace RoleplayersToolbox.Tools.Illegal.EmoteSnap {
    [Serializable]
    internal class EmoteSnapConfig {
        public bool DisableDozeSnap;
    }
}

#endif
