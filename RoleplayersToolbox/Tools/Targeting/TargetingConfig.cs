using System;
using RoleplayersToolbox.Tools.Targeting;

namespace RoleplayersToolbox.Tools {
    internal partial class ToolConfig {
        public TargetingConfig Targeting { get; set; } = new();
    }
}

namespace RoleplayersToolbox.Tools.Targeting {
    [Serializable]
    internal class TargetingConfig {
        public bool LeftClickExamine;
        public bool RightClickExamine;
        public bool KeepTarget;
    }
}
