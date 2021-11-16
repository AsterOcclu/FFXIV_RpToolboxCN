using System;
using Dalamud.Configuration;
using RoleplayersToolbox.Tools;

namespace RoleplayersToolbox {
    [Serializable]
    internal class Configuration : IPluginConfiguration {
        public int Version { get; set; } = 1;

        public ToolConfig Tools { get; set; } = new();
    }
}
