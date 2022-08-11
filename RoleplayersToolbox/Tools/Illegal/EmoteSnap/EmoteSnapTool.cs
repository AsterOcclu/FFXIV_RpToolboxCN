#if ILLEGAL

using System;
using Dalamud.Game.Command;
using Dalamud.Hooking;
using ImGuiNET;

namespace RoleplayersToolbox.Tools.Illegal.EmoteSnap {
    internal class EmoteSnapTool : BaseTool, IDisposable {
        private static class Signatures {
            internal const string ShouldSnap = "E8 ?? ?? ?? ?? 84 C0 74 46 4C 8D 6D C7";
        }

        private delegate byte ShouldSnapDelegate(IntPtr a1, IntPtr a2);

        public override string Name => "打盹入睡";

        private Plugin Plugin { get; }
        private EmoteSnapConfig Config { get; }
        private Hook<ShouldSnapDelegate>? ShouldSnapHook { get; }

        internal EmoteSnapTool(Plugin plugin) {
            this.Plugin = plugin;
            this.Config = this.Plugin.Config.Tools.EmoteSnap;

            if (this.Plugin.SigScanner.TryScanText(Signatures.ShouldSnap, out var snapPtr)) {
                this.ShouldSnapHook = Hook<ShouldSnapDelegate>.FromAddress(snapPtr, this.ShouldSnapDetour);
                this.ShouldSnapHook.Enable();
            }

            this.Plugin.CommandManager.AddHandler("/dozesnap", new CommandInfo(this.OnCommand) {
                HelpMessage = "切换在床边 /打盹 后是否躺下入睡",
            });
        }

        public void Dispose() {
            this.Plugin.CommandManager.RemoveHandler("/dozesnap");
            this.ShouldSnapHook?.Dispose();
        }

        public override void DrawSettings(ref bool anyChanged) {
            anyChanged |= ImGui.Checkbox("禁用 /打盹 后躺下入睡", ref this.Config.DisableDozeSnap);

            ImGui.TextUnformatted("勾选以阻止在床边 /打盹 后自动躺下入睡. 若想使用睡眠动作，可利用工具的动作替换功能，把睡眠动作拖入到热键栏。");
            ImGui.TextUnformatted("使用 /dozesnap 命令切换。");
        }

        private byte ShouldSnapDetour(IntPtr a1, IntPtr a2) {
            return this.Config.DisableDozeSnap
                ? (byte) 0
                : this.ShouldSnapHook!.Original(a1, a2);
        }

        private void OnCommand(string command, string arguments) {
            this.Config.DisableDozeSnap ^= true;

            var status = this.Config.DisableDozeSnap ? "off" : "on";
            this.Plugin.ChatGui.Print($"/打盹 强制入睡状态已切换为： {status}.");
        }
    }
}

#endif
