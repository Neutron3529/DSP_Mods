/*
compile instructions: put this file in `steamapps/common/Dyson Sphere Program/CustomMod` folder, open a terminal in the same folder, and execute:
dotnet /usr/share/dotnet/sdk/5.0.201/Roslyn/bincore/csc.dll -t:library \
  -r:'../BepInEx/core/BepInEx.dll' \
  -r:'../BepInEx/core/0Harmony.dll' \
  -r:'../DSPGAME_Data/Managed/System.dll' \
  -r:'../DSPGAME_Data/Managed/UnityEngine.dll' \
  -r:'../DSPGAME_Data/Managed/UnityEngine.CoreModule.dll' \
  -r:'../DSPGAME_Data/Managed/mscorlib.dll' \
  -r:'../DSPGAME_Data/Managed/Assembly-CSharp.dll' \
  powerFull.cs \
  -out:'../BepInEx/plugins/powerfull.dll' \
  -optimize \
  -define:DEBUG # optional
 */
using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;


namespace PowerFull
{
    [BepInPlugin("Neutron3529.PowerFull", "PowerFull", "0.1.0")]
    public class PowerFull : BaseUnityPlugin {
        public static float power_mul=10.0f;
        public static double mecha_core_power_gen_mul = 1.0;
        public static float logistic_drone_speed_mul = 1.0f;
        public static float logistic_ship_sail_speed_mul = 1.0f;
        public static float logistic_ship_warp_speed_mul = 1.0f;
        public static float max_mining_cost = 1.0f;
        public static int extra_sand_mul = 1;//额外沙土数量，垃圾桶用
        public static Player player;//用于各种神奇操作
        public static Traverse sand_count;
        public static bool enable1=true;
        public static bool enable2=true;
        public static bool enable3=true;
        public static double mechaCorePowerGen = 18000.0;
        public static float logisticDroneSpeed = 8.0f;
        public static float logisticShipSailSpeed = 400.0f;
        public static float logisticShipWarpSpeed = 400000.0f;
#if DEBUG
        public static Action<string> logger;
#endif
        void Start() {
            mechaCorePowerGen = Configs.freeMode.mechaCorePowerGen;
            logisticDroneSpeed = Configs.freeMode.logisticDroneSpeed;
            logisticShipSailSpeed = Configs.freeMode.logisticShipSailSpeed;
            logisticShipWarpSpeed = Configs.freeMode.logisticShipWarpSpeed;

            enable2 = Config.Bind<bool>("config", "enable_power_mul", false, "开启电力乘数patch").Value;
            power_mul = Config.Bind<float>("config", "power_mul", 10.0f, "电力乘数").Value;

            enable1 = Config.Bind<bool>("config", "enable_tech_modding", false, "开启科技效果修改patch").Value;
            mecha_core_power_gen_mul = Config.Bind<double>("config", "mecha_core_power_gen_mul", 1000.0f, "机甲核心生成能量的速度乘数，会影响存档！").Value;
            logistic_drone_speed_mul = Config.Bind<float>("config", "logistic_drone_speed_mul", 10.0f, "行星内运输机速度乘数，会影响存档！").Value;
            logistic_ship_sail_speed_mul = Config.Bind<float>("config", "logistic_ship_sail_speed_mul", 3000.0f, "星际运输机速度乘数，会影响存档！").Value;
            logistic_ship_warp_speed_mul = Config.Bind<float>("config", "logistic_ship_warp_speed_mul", 5.0f, "星际运输机翘曲速度乘数，会影响存档！").Value;
            max_mining_cost = Config.Bind<float>("config", "max_mining_cost", 0.0f, "挖矿消耗矿物比例的最大值，可以设为0以阻止采矿机消耗矿物，会影响存档！").Value;
            if (!enable1){/*此时注入仍然存在，但已经失活*/
                mecha_core_power_gen_mul=1.0;
                logistic_drone_speed_mul=1.0f;
                logistic_ship_sail_speed_mul=1.0f;
                logistic_ship_warp_speed_mul=1.0f;
                max_mining_cost=1.0f;
            }
            enable3 = Config.Bind<bool>("config", "enable_extra_sand_mul", false, "开启垃圾箱patch").Value;
            extra_sand_mul = Config.Bind<int>("config", "extra_sand_mul", 1, "垃圾箱中每物品转化为沙土的数量").Value;
            var harmony=new Harmony("Neutron3529.Powerful");
#if DEBUG
            logger=Logger.LogInfo;
#endif
            if (true){// 采矿无消耗，必须打开，因为需要在这里初始化几个反射的委托
                var original = typeof(GameData).GetMethod("Import", AccessTools.all);
                var postfix = typeof(GameDataImportPatch).GetMethod("Postfix");
                harmony.Patch(original, null, new HarmonyMethod(postfix));
#if DEBUG
                logger("PowerFull-GameData.Import-注入完成");
                if (enable1){
                    logger("PowerFull-科技效果修改-加载完成");
                }
#endif
            }
            if (enable2){// 电力x10
                var original = typeof(PowerSystem).GetMethod("GameTick", AccessTools.all);
                var postfix = typeof(PowerSystemGameTickPatch).GetMethod("Postfix");
                harmony.Patch(original, null, new HarmonyMethod(postfix));
#if DEBUG
                logger("PowerFull-电力x10-加载完成");
#endif
            }
            if (enable3){// 垃圾桶
                var original = typeof(StorageComponent).GetMethod("AddItem", new [] { typeof(int),typeof(int), typeof(bool) });
                var prefix = typeof(StorageComponentAddItemPatch).GetMethod("Prefix");
                harmony.Patch(original, new HarmonyMethod(prefix));
#if DEBUG
                logger("PowerFull-垃圾桶-加载完成");
#endif
            }
#if DEBUG
            logger("PowerFull加载完成");
#endif
        }
        [HarmonyPatch(typeof(GameData), "Import")]
        class GameDataImportPatch{
            public static void Postfix(GameData __instance){
                player = __instance.mainPlayer;
                sand_count = Traverse.Create(player).Field("<sandCount>k__BackingField");
                if (__instance.mainPlayer.mecha.corePowerGen != mechaCorePowerGen * mecha_core_power_gen_mul && enable1){
#if DEBUG
                    logger(string.Format("mechaCorePowerGen的原始值为{0:N5}，触发修改，新值为{1:N5}", __instance.mainPlayer.mecha.corePowerGen, mechaCorePowerGen * mecha_core_power_gen_mul));
#endif
                    __instance.mainPlayer.mecha.corePowerGen = mechaCorePowerGen * mecha_core_power_gen_mul;
#if DEBUG
                }else{
                    logger(string.Format("mechaCorePowerGen的原始值为{0:N5}，新值为{1:N5}，未触发修改", __instance.mainPlayer.mecha.corePowerGen, mechaCorePowerGen * mecha_core_power_gen_mul));
#endif
                }
                if (__instance.history.logisticDroneSpeed != logisticDroneSpeed * logistic_drone_speed_mul && enable1){
#if DEBUG
                    logger(string.Format("logisticDroneSpeed的原始值为{0:N5}，触发修改，新值为{1:N5}", __instance.history.logisticDroneSpeed, logisticDroneSpeed * logistic_drone_speed_mul));
#endif
                    __instance.history.logisticDroneSpeed = logisticDroneSpeed * logistic_drone_speed_mul;
#if DEBUG
                }else{
                    logger(string.Format("logisticDroneSpeed的原始值为{0:N5}，新值为{1:N5}，未触发修改", __instance.history.logisticDroneSpeed, logisticDroneSpeed * logistic_drone_speed_mul));
#endif
                }
                if (__instance.history.logisticShipSailSpeed != logisticShipSailSpeed * logistic_ship_sail_speed_mul && enable1){
#if DEBUG
                    logger(string.Format("logisticShipSailSpeed的原始值为{0:N5}，触发修改，新值为{1:N5}", __instance.history.logisticShipSailSpeed, logisticShipSailSpeed * logistic_ship_sail_speed_mul));
#endif
                    __instance.history.logisticShipSailSpeed = logisticShipSailSpeed * logistic_ship_sail_speed_mul;
#if DEBUG
                }else{
                    logger(string.Format("logisticShipSailSpeed的原始值为{0:N5}，新值为{1:N5}，未触发修改", __instance.history.logisticShipSailSpeed, logisticShipSailSpeed * logistic_ship_sail_speed_mul));
#endif
                }
                if (__instance.history.logisticShipWarpSpeed != logisticShipWarpSpeed * logistic_ship_warp_speed_mul && enable1){
#if DEBUG
                    logger(string.Format("logisticShipWarpSpeed的原始值为{0:N5}，触发修改，新值为{1:N5}", __instance.history.logisticShipWarpSpeed, logisticShipWarpSpeed * logistic_ship_warp_speed_mul));
#endif
                    __instance.history.logisticShipWarpSpeed = logisticShipWarpSpeed * logistic_ship_warp_speed_mul;
#if DEBUG
                }else{
                    logger(string.Format("logisticShipWarpSpeed的原始值为{0:N5}，新值为{1:N5}，未触发修改", __instance.history.logisticShipWarpSpeed, logisticShipWarpSpeed * logistic_ship_warp_speed_mul));
#endif
                }
                if (__instance.history.miningCostRate > max_mining_cost && enable1){
#if DEBUG
                    logger(string.Format("miningCostRate的原始值为{0:N5}，触发修改，新值为{1:N5}", __instance.history.miningCostRate, max_mining_cost));
#endif
                    __instance.history.miningCostRate = max_mining_cost;
#if DEBUG
                }else{
                    logger(string.Format("miningCostRate的原始值为{0:N5}，不大于设定的阈值{1:N5}", __instance.history.miningCostRate, max_mining_cost));
#endif
                }
            }
        }
        [HarmonyPatch(typeof(PowerSystem), "GameTick")]
        class PowerSystemGameTickPatch {
            public static void Postfix(PowerSystem __instance) {
                for(int i=__instance.networkServes.Length-1;i>=0;i--) {
                    __instance.networkServes[i]*=power_mul;
                }
            }
        }
        [HarmonyPatch(typeof(StorageComponent), "AddItem",new Type[] { typeof(int),typeof(int), typeof(bool) })]
        class StorageComponentAddItemPatch{ // 来自https://github.com/dsp-mod/Trash/blob/main/Trash.cs的垃圾箱，省略掉了弹出提示的步骤，或有加速。
            public static bool Prefix(ref int __result, StorageComponent __instance,int itemId, int count, bool useBan = false){
                if (useBan && __instance.size==__instance.bans && itemId!=0 && count!=0){
                    ulong tmp = (ulong)count * (ulong)extra_sand_mul + (ulong) sand_count.GetValue<int>();
                    if (tmp > 1000000000){
                        sand_count.SetValue(1000000000);
                    }else{
                        sand_count.SetValue((int) tmp);
                    }
                    __result = count;
                    return false;
                } else {
                    return true;
                }
            }
        }
        /*
        [HarmonyPatch(typeof(Player), "sandCount", MethodType.Getter)]
        class PlayersandCountPatch{ // 来自https://github.com/dsp-mod/Trash/blob/main/Trash.cs的垃圾箱，省略掉了弹出提示的步骤，或有加速。
            public static void Postfix(Player __instance, ref int __result){
                __result += extra_sand_count;
                if (__result > 1000000000){
                    __result = 1000000000;
                }
//                __instance.set_sandCount(__result);
#if DEBUG
                logger(string.Format("触发沙土getter，应满足{0}>{1}或不等式两边均为0",extra_sand_count, __result - __instance.sandCount));
#endif
            }
        }
        [HarmonyPatch(typeof(Player), "sandCount", MethodType.Setter)]
        class PlayersandCountPatch{ // 来自https://github.com/dsp-mod/Trash/blob/main/Trash.cs的垃圾箱，省略掉了弹出提示的步骤，或有加速。
            public static void Prefix(Player __instance, ref int __result){
                __result += extra_sand_count;
                if (__result > 1000000000){
                    __result = 1000000000;
                }
//                __instance.set_sandCount(__result);
#if DEBUG
                logger(string.Format("触发沙土getter，应满足{0}>{1}或不等式两边均为0",extra_sand_count, __result - __instance.sandCount));
#endif
            }
        }*/
    }
}