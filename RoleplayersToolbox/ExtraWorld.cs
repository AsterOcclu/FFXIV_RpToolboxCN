using System;
using System.Collections.Generic;
using Dalamud.Data;
using Dalamud.IoC;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Logging;
using Lumina.Text;


namespace RoleplayersToolbox {

    class WorldInfo
    {

        // public static  

        [PluginService]
        public static DataManager DataManager { get; } = null!;


        public uint id;
        public string name;
        public string localName;
        public uint dcRow;

        //构造函数
        public WorldInfo(uint id, string name, string localName, uint dcRow)
        {
            this.id = id;
            this.name = name;
            this.localName = localName;
            this.dcRow = dcRow;
            ExtraWorld.DataCenters[dcRow].worlds[id] = this;
        }

        public WorldInfo(uint id, string name)
        {
            this.id = id;
            this.name = name;
            this.localName = name;
        }

        public override string ToString()
        {
            return this.localName;
        }
    }

    class DcInfo
    {
        public uint row;
        public string name;
        public string localName;
        public Dictionary<uint, WorldInfo> worlds = new Dictionary<uint, WorldInfo>{};

        //构造函数
        public DcInfo(uint row, string name, string localName)
        {
            this.row = row;
            this.name = name;
            this.localName = localName;
        }

        public override string ToString()
        {
            return this.localName;
        }
    }

    internal static class ExtraWorld {

        public static Dictionary<uint, DcInfo> DataCenters = new Dictionary<uint, DcInfo> {
            {91, new DcInfo(91, "LuXingNiao", "陆行鸟")},
            {92, new DcInfo(92, "MoGuLi", "莫古力")},
            {93, new DcInfo(93, "MaoXiaoPang", "猫小胖")},
        };


        public static Dictionary<uint, WorldInfo> All = new Dictionary<uint, WorldInfo> {
            {1175, new WorldInfo(1175, "ChenXiWangZuo", "晨曦王座", 91)},
            {1174, new WorldInfo(1174, "WoXianXiRan", "沃仙曦染", 91)},
            {1173, new WorldInfo(1173, "YuZhouHeYin", "宇宙和音", 91)},
            {1167, new WorldInfo(1167, "HongYuHai", "红玉海", 91)},
            {1060, new WorldInfo(1060, "MengYaChi", "萌芽池", 91)},
            {1081, new WorldInfo(1081, "ShenYiZhiDi", "神意之地", 91)},
            {1044, new WorldInfo(1044, "HuanYingQunDao", "幻影群岛", 91)},
            {1042, new WorldInfo(1042, "LaNuoXiYa", "拉诺西亚", 91)},

            {1121, new WorldInfo(1121, "FuXiaoZhiJian", "拂晓之间", 92)},
            {1166, new WorldInfo(1166, "Longchaoshendian", "龙巢神殿", 92)},
            {1113, new WorldInfo(1113, "LvRenZhanQiao", "旅人栈桥", 92)},
            {1076, new WorldInfo(1076, "BaiJinHuanXiang", "白金幻象", 92)},
            {1176, new WorldInfo(1176, "MengYuBaoJing", "梦羽宝境", 92)},
            {1171, new WorldInfo(1171, "ShenQuanHen", "神拳痕", 92)},
            {1170, new WorldInfo(1170, "ChaoFengTing", "潮风亭", 92)},
            {1172, new WorldInfo(1172, "BaiYinXiang", "白银乡", 92)},

            {1179, new WorldInfo(1179, "HuPoYuan", "琥珀原", 93)},
            {1178, new WorldInfo(1178, "RouFengHaiWan", "柔风海湾", 93)},
            {1177, new WorldInfo(1177, "HaiMaoChaWu", "海猫茶屋", 93)},
            {1169, new WorldInfo(1169, "YanXia", "延夏", 93)},
            {1106, new WorldInfo(1106, "JingYuZhuangYuan", "静语庄园", 93)},
            {1045, new WorldInfo(1045, "MoDuNa", "摩杜纳", 93)},
            {1043, new WorldInfo(1043, "ZiShuiZhanQiao", "紫水栈桥", 93)},
        };
    


        public static uint GetDCRowByWorld(World world) {
            if (world.DataCenter.Row > 0) {
                return world.DataCenter.Row;
            }
            WorldInfo? exWorld;
            if (ExtraWorld.All.TryGetValue(world.RowId, out exWorld)) {
                return exWorld.dcRow;
            }
            return 0;
        }

        public static World[] GetAllWorldsByDcRow(uint dcRow, DataManager dataManager) {
            var gameWorlds = dataManager.GetExcelSheet<World>()!;
            DcInfo? dc;
            var worlds = new List<World>();
            if (ExtraWorld.DataCenters.TryGetValue(dcRow, out dc)) {
                foreach (var item in dc.worlds) {
                    var world = gameWorlds.GetRow(item.Key)!;
                    world.Name = new SeString(item.Value.localName);
                    worlds.Add(world);
                }
            }
            return worlds.ToArray();
        }


        public static World[] GetAllWorldsInSameDc(World world, Plugin plugin) {
            if (world.DataCenter.Row <= 0) {
                WorldInfo? exWorld;
                if (ExtraWorld.All.TryGetValue(world.RowId, out exWorld)) {
                    return ExtraWorld.GetAllWorldsByDcRow(exWorld.dcRow, plugin.DataManager);
                }
            }
            var worlds = new List<World>();
            foreach (var item in WorldInfo.DataManager.GetExcelSheet<World>()!) {
                if (item.DataCenter.Row == world.DataCenter.Row && item.IsPublic) {
                    worlds.Add(item);
                }
            }
            return worlds.ToArray();
        }


    }

}