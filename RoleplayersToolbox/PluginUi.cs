using System;
using System.Numerics;
using Dalamud.Logging;
using ImGuiNET;

namespace RoleplayersToolbox {
    internal class PluginUi : IDisposable {
        private Plugin Plugin { get; }

        private bool _showInterface;

        internal bool ShowInterface {
            get => this._showInterface;
            set => this._showInterface = value;
        }

        internal PluginUi(Plugin plugin) {
            this.Plugin = plugin;

            this.Plugin.Interface.UiBuilder.Draw += this.Draw;
            this.Plugin.Interface.UiBuilder.OpenConfigUi += this.OpenConfig;
        }

        public void Dispose() {
            this.Plugin.Interface.UiBuilder.OpenConfigUi -= this.OpenConfig;
            this.Plugin.Interface.UiBuilder.Draw -= this.Draw;
        }

        private void OpenConfig() {
            this.ShowInterface = true;
        }

        private void Draw() {
            this.DrawSettings();

            foreach (var tool in this.Plugin.Tools) {
                try {
                    tool.DrawAlways();
                } catch (Exception ex) {
                    PluginLog.LogError(ex, $"Error drawing tool: {tool.Name}");
                }
            }
        }

        private void DrawSettings() {
            if (!this.ShowInterface) {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(450, 300), ImGuiCond.FirstUseEver);

            if (!ImGui.Begin("The Roleplayer's Toolbox", ref this._showInterface)) {
                ImGui.End();
                return;
            }

            if (ImGui.BeginTabBar("rp-toolbox-tabs")) {
                var anyChanged = false;

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var tool in this.Plugin.Tools) {
                    if (!ImGui.BeginTabItem($"{tool.Name}")) {
                        continue;
                    }

                    if (ImGui.BeginChild($"{tool.Name} child", new Vector2(-1, -1))) {
                        ImGui.PushTextWrapPos();

                        try {
                            tool.DrawSettings(ref anyChanged);
                        } catch (Exception ex) {
                            PluginLog.LogError(ex, $"Error drawing settings for tool: {tool.Name}");
                        }

                        ImGui.PopTextWrapPos();
                        ImGui.EndChild();
                    }

                    ImGui.EndTabItem();
                }

                if (anyChanged) {
                    this.Plugin.SaveConfig();
                }

                ImGui.EndTabBar();
            }

            ImGui.End();
        }
    }
}
