using System;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace RoleplayersToolbox.Tools.Housing {
    internal class BookmarksUi {
        private Plugin Plugin { get; }
        private HousingTool Tool { get; }
        private HousingConfig Config { get; }
        internal (Bookmark editing, int index)? Editing;
        private int _dragging = -1;

        internal bool ShouldDraw;

        internal BookmarksUi(Plugin plugin, HousingTool tool, HousingConfig config) {
            this.Plugin = plugin;
            this.Tool = tool;
            this.Config = config;
        }

        internal void Draw() {
            if (!this.ShouldDraw) {
                return;
            }

            this.AddEditWindow();

            if (!ImGui.Begin("住宅收藏夹", ref this.ShouldDraw)) {
                ImGui.End();
                return;
            }

            if (Util.IconButton(FontAwesomeIcon.Plus)) {
                this.Editing = (new Bookmark(string.Empty), -1);
            }

            Util.Tooltip("添加书签");

            ImGui.SameLine();

            ImGui.TextUnformatted("添加住宅书签");

            ImGui.Spacing();

            var toDelete = -1;

            (int src, int dst)? drag = null;
            if (ImGui.BeginChild("bookmark-list", new Vector2(-1, -1))) {
                for (var i = 0; i < this.Config.Bookmarks.Count; i++) {
                    var bookmark = this.Config.Bookmarks[i];
                    var hash = bookmark.GetHashCode().ToString();

                    if (ImGui.TreeNode($"{bookmark.Name}##{hash}")) {
                        var worldName = this.Plugin.DataManager.GetExcelSheet<World>()!.GetRow(bookmark.WorldId)?.Name;
                        ImGui.TextUnformatted($"{worldName}/{bookmark.Area.Name()}/{bookmark.Ward}区/{bookmark.Plot}号");

                        if (Util.IconButton(FontAwesomeIcon.MapMarkerAlt, hash)) {
                            this.Tool.FlagHouseOnMap(bookmark.Area, bookmark.Plot);
                        }

                        Util.Tooltip("展示地图");

                        ImGui.SameLine();

                        if (Util.IconButton(FontAwesomeIcon.Route, hash)) {
                            this.Tool.Destination = new DestinationInfo(
                                this.Tool.Info,
                                this.Plugin.DataManager.GetExcelSheet<World>()!.GetRow(bookmark.WorldId),
                                bookmark.Area,
                                bookmark.Ward,
                                bookmark.Plot
                            );
                        }

                        Util.Tooltip("打开传送路径");

                        ImGui.SameLine();

                        if (Util.IconButton(FontAwesomeIcon.PencilAlt, hash)) {
                            this.Editing = (bookmark.Clone(), i);
                        }

                        Util.Tooltip("编辑");

                        ImGui.SameLine();

                        if (Util.IconButton(FontAwesomeIcon.Trash, hash)) {
                            toDelete = i;
                        }

                        Util.Tooltip("删除");

                        ImGui.TreePop();
                    }

                    if (ImGui.IsItemActive() || this._dragging == i) {
                        this._dragging = i;
                        var step = 0;
                        if (ImGui.GetIO().MouseDelta.Y < 0 && ImGui.GetMousePos().Y < ImGui.GetItemRectMin().Y) {
                            step = -1;
                        }

                        if (ImGui.GetIO().MouseDelta.Y > 0 && ImGui.GetMousePos().Y > ImGui.GetItemRectMax().Y) {
                            step = 1;
                        }

                        if (step != 0) {
                            drag = (i, i + step);
                        }
                    }

                    ImGui.Separator();
                }

                ImGui.EndChild();
            }

            if (!ImGui.IsMouseDown(ImGuiMouseButton.Left) && this._dragging != -1) {
                this._dragging = -1;
                this.Plugin.SaveConfig();
            }

            if (drag != null && drag.Value.dst < this.Config.Bookmarks.Count && drag.Value.dst >= 0) {
                this._dragging = drag.Value.dst;
                var temp = this.Config.Bookmarks[drag.Value.src];
                this.Config.Bookmarks[drag.Value.src] = this.Config.Bookmarks[drag.Value.dst];
                this.Config.Bookmarks[drag.Value.dst] = temp;
            }

            if (toDelete > -1) {
                this.Config.Bookmarks.RemoveAt(toDelete);
                this.Plugin.SaveConfig();
            }

            ImGui.End();
        }

        private void AddEditWindow() {
            if (this.Editing == null) {
                return;
            }

            if (!ImGui.Begin("编辑书签", ImGuiWindowFlags.AlwaysAutoResize)) {
                ImGui.End();
                return;
            }

            var (bookmark, index) = this.Editing.Value;

            ImGui.InputText("名称", ref bookmark.Name, 255);

            var world = bookmark.WorldId == 0
                ? null
                : this.Plugin.DataManager.GetExcelSheet<World>()!.GetRow(bookmark.WorldId);
            if (ImGui.BeginCombo("服务器", world?.Name?.ToString() ?? string.Empty)) {
                var homeWorld = this.Plugin.ClientState.LocalPlayer?.HomeWorld;
                var allWorlds = ExtraWorld.GetAllWorldsInSameDc(homeWorld!.GameData, this.Plugin);
                foreach (var item in allWorlds) {
                    if (!ImGui.Selectable(item.Name.ToString())) {
                        continue;
                    }
                    bookmark.WorldId = item.RowId;
                }
                ImGui.EndCombo();
            }

            var area = bookmark.Area;
            if (ImGui.BeginCombo("住宅区", area != 0 ? area.Name() : string.Empty)) {
                foreach (var housingArea in (HousingArea[]) Enum.GetValues(typeof(HousingArea))) {
                    if (!ImGui.Selectable(housingArea.Name(), area == housingArea)) {
                        continue;
                    }

                    bookmark.Area = housingArea;
                }

                ImGui.EndCombo();
            }

            var ward = (int) bookmark.Ward;
            if (ImGui.InputInt("区", ref ward)) {
                bookmark.Ward = (uint) Math.Max(1, Math.Min(24, ward));
            }

            var plot = (int) bookmark.Plot;
            if (ImGui.InputInt("号", ref plot)) {
                bookmark.Plot = (uint) Math.Max(1, Math.Min(60, plot));
            }

            if (ImGui.Button("保存") && !bookmark.AnyZero()) {
                if (index < 0) {
                    this.Config.Bookmarks.Add(bookmark);
                } else if (index < this.Config.Bookmarks.Count) {
                    this.Config.Bookmarks[index] = bookmark;
                }

                this.Plugin.SaveConfig();
                this.Editing = null;
            }

            ImGui.SameLine();

            if (ImGui.Button("取消")) {
                this.Editing = null;
            }

            ImGui.End();
        }
    }
}
