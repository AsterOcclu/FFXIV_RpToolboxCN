using System;
using System.Collections.Generic;
using System.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface;
using ImGuiNET;

namespace RoleplayersToolbox {
    internal static class Util {
        public static SeString ReadSeString(IntPtr ptr) {
            var bytes = ReadTerminatedBytes(ptr);
            return SeString.Parse(bytes);
        }

        public static string ReadString(IntPtr ptr) {
            var bytes = ReadTerminatedBytes(ptr);
            return Encoding.UTF8.GetString(bytes);
        }

        private static unsafe byte[] ReadTerminatedBytes(IntPtr ptr) {
            if (ptr == IntPtr.Zero) {
                return Array.Empty<byte>();
            }

            var bytes = new List<byte>();

            var bytePtr = (byte*) ptr;
            while (*bytePtr != 0) {
                bytes.Add(*bytePtr);
                bytePtr += 1;
            }

            return bytes.ToArray();
        }

        internal static bool IconButton(FontAwesomeIcon icon, string? id = null) {
            var label = icon.ToIconString();
            if (id != null) {
                label += $"##{id}";
            }

            ImGui.PushFont(UiBuilder.IconFont);
            var ret = ImGui.Button(label);
            ImGui.PopFont();

            return ret;
        }

        internal static void Tooltip(string tooltip) {
            if (!ImGui.IsItemHovered()) {
                return;
            }

            ImGui.BeginTooltip();
            ImGui.TextUnformatted(tooltip);
            ImGui.EndTooltip();
        }
    }
}
