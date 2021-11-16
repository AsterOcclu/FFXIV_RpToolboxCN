using System;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Hooking;
using ImGuiNET;

namespace RoleplayersToolbox.Tools.Targeting {
    internal class TargetingTool : BaseTool, IDisposable {
        private static class Signatures {
            internal const string LeftClickTarget = "E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 74 16";
            internal const string RightClickTarget = "E8 ?? ?? ?? ?? 48 8B CE E8 ?? ?? ?? ?? 48 85 C0 74 1B";
        }

        private unsafe delegate void* ClickTargetDelegate(void** a1, void* a2, bool a3);

        public override string Name => "目标选中";
        private Plugin Plugin { get; }
        private TargetingConfig Config { get; }
        private Hook<ClickTargetDelegate>? LeftClickHook { get; }
        private Hook<ClickTargetDelegate>? RightClickHook { get; }

        internal TargetingTool(Plugin plugin) {
            this.Plugin = plugin;
            this.Config = this.Plugin.Config.Tools.Targeting;

            if (this.Plugin.SigScanner.TryScanText(Signatures.LeftClickTarget, out var leftClickPtr)) {
                unsafe {
                    this.LeftClickHook = new Hook<ClickTargetDelegate>(leftClickPtr, this.LeftClickDetour);
                }

                this.LeftClickHook.Enable();
            }

            if (this.Plugin.SigScanner.TryScanText(Signatures.RightClickTarget, out var rightClickPtr)) {
                unsafe {
                    this.RightClickHook = new Hook<ClickTargetDelegate>(rightClickPtr, this.RightClickDetour);
                }

                this.RightClickHook.Enable();
            }
        }

        public void Dispose() {
            this.LeftClickHook?.Dispose();
            this.RightClickHook?.Dispose();
        }

        public override void DrawSettings(ref bool anyChanged) {
            anyChanged |= ImGui.Checkbox("左键快速调查玩家", ref this.Config.LeftClickExamine);
            anyChanged |= ImGui.Checkbox("右键快速调查玩家", ref this.Config.RightClickExamine);
            anyChanged |= ImGui.Checkbox("锁定当前选中的玩家（请先选中目标再勾选锁定）", ref this.Config.KeepTarget);
        }

        private unsafe void* LeftClickDetour(void** a1, void* clickedOn, bool a3) {
            var target = a1[16];

            if (clickedOn == null) {
                if (this.Config.KeepTarget) {
                    return this.LeftClickHook!.Original(a1, target, a3);
                }

                goto Original;
            }

            if (this.Config.LeftClickExamine) {
                var obj = this.Plugin.ObjectTable.CreateObjectReference((IntPtr) clickedOn);
                if (obj != null && obj.ObjectKind == ObjectKind.Player) {
                    this.Plugin.Common.Functions.Examine.OpenExamineWindow(obj.ObjectId);
                    // tell game current target was left-clicked
                    return this.LeftClickHook!.Original(a1, target, a3);
                }
            }

            if (this.Config.KeepTarget && clickedOn != target) {
                return this.LeftClickHook!.Original(a1, target, a3);
            }

            Original:
            return this.LeftClickHook!.Original(a1, clickedOn, a3);
        }

        private unsafe void* RightClickDetour(void** a1, void* clickedOn, bool a3) {
            if (clickedOn == null) {
                goto Original;
            }

            var target = a1[16];

            if (this.Config.RightClickExamine) {
                if (clickedOn == target) {
                    // allow right-clicking on target
                    goto Original;
                }

                var obj = this.Plugin.ObjectTable.CreateObjectReference((IntPtr) clickedOn);
                if (obj != null && obj.ObjectKind == ObjectKind.Player) {
                    this.Plugin.Common.Functions.Examine.OpenExamineWindow(obj.ObjectId);
                    // tell game nothing was right-clicked
                    return this.RightClickHook!.Original(a1, null, a3);
                }
            }

            if (this.Config.KeepTarget && clickedOn != target) {
                return this.RightClickHook!.Original(a1, null, a3);
            }

            Original:
            return this.RightClickHook!.Original(a1, clickedOn, a3);
        }
    }
}
