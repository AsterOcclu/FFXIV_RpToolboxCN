using System;
using System.Linq;
using System.Runtime.InteropServices;
using Dalamud.Data;
using Lumina.Excel.GeneratedSheets;
using RoleplayersToolbox.Tools.Housing;

namespace RoleplayersToolbox {
    public class Teleport {
        private static class Signatures {
            internal const string Teleport = "E8 ?? ?? ?? ?? 48 8B 4B 10 84 C0 48 8B 01 74 2C";
            internal const string TelepoAddress = "48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 74 15 48 8B CB E8 ?? ?? ?? ?? 48 8B CB 48 83 C4 20";
        }

        private delegate bool TeleportDelegate(IntPtr tpStatusPtr, uint aetheryteId, byte subIndex);

        private readonly IntPtr _telepo;
        private readonly TeleportDelegate? _teleport;

        private DataManager Data { get; }

        internal Teleport(Plugin plugin) {
            this.Data = plugin.DataManager;

            plugin.SigScanner.TryGetStaticAddressFromSig(Signatures.TelepoAddress, out this._telepo);

            if (plugin.SigScanner.TryScanText(Signatures.Teleport, out var teleportPtr)) {
                this._teleport = Marshal.GetDelegateForFunctionPointer<TeleportDelegate>(teleportPtr);
            }
        }

        internal void TeleportToHousingArea(HousingArea area) {
            if (this._telepo == IntPtr.Zero || this._teleport == null) {
                return;
            }

            var aetheryte = this.Data.GetExcelSheet<Aetheryte>()!.FirstOrDefault(aeth => aeth.IsAetheryte && aeth.Territory.Row == area.CityStateTerritoryType());
            if (aetheryte == null) {
                return;
            }

            this._teleport(this._telepo, aetheryte.RowId, 0);
        }
    }
}
