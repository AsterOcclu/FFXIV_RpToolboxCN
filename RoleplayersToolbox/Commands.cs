using System;
using Dalamud.Game.Command;

namespace RoleplayersToolbox {
    internal class Commands : IDisposable {
        private Plugin Plugin { get; }

        internal Commands(Plugin plugin) {
            this.Plugin = plugin;

            this.Plugin.CommandManager.AddHandler("/rptools", new CommandInfo(this.OnCommand) {
                HelpMessage = "打开 Roleplayer 工具箱",
            });
        }

        public void Dispose() {
            this.Plugin.CommandManager.RemoveHandler("/rptools");
        }

        private void OnCommand(string command, string arguments) {
            this.Plugin.Ui.ShowInterface ^= true;
        }
    }
}
