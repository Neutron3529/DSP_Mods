#!/bin/bash -e
#   * compile instructions: put this file in
#   *         `steamapps/common/Dyson Sphere Program/CustomMod`
#   * folder, open a terminal in the same folder, and execute:
#   *
#   * ```
#   *     chmod +x powerFull.cs
#   *     ./powerFull.cs
#   * ```
#   *
#   * then the mod will be compiled automatically.
#   *
#   * Here we wrote a shebang like file, which is correct
#   * in my computer (Manjaro XFCE), if such script do not work
#   * in your computer, you could just try the instructions below :

if [ -z "$__DOTNET_CSC" ]; then
    export __DOTNET_CSC="`find /usr/share/dotnet -name dotnet` `find /usr/share/dotnet -name csc.dll`"
    echo '$'"__DOTNET_CSC not set yet, it is automatically set to '$__DOTNET_CSC'"
    echo "This program will exit now, if the path is correct, you could execute $0 again, if not, set __DOTNET_CSC to the correct value."
fi
__MODE_VERBOSE=55 # may be modified, check it carefully.
__MODE_DEBUG__=56
__MODE_RELEASE=57

case $1 in
    V)       _MODE__SELECT_=$__MODE_VERBOSE     ;;
    v)       _MODE__SELECT_=$__MODE_VERBOSE     ;;
    VERBOSE) _MODE__SELECT_=$__MODE_VERBOSE     ;;
    verbose) _MODE__SELECT_=$__MODE_VERBOSE     ;;
    D)       _MODE__SELECT_=$__MODE_DEBUG__     ;;
    d)       _MODE__SELECT_=$__MODE_DEBUG__     ;;
    DEBUG)   _MODE__SELECT_=$__MODE_DEBUG__     ;;
    debug)   _MODE__SELECT_=$__MODE_DEBUG__     ;;
    *)       _MODE__SELECT_=$__MODE_RELEASE     ;;
esac

FILE_NAME=$0

# ( yes "" | head -n $_MODE__SELECT_ | head -n-1  ; tail $FILE_NAME -n+$_MODE__SELECT_ ) | head -n 55

( yes "" | head -n $_MODE__SELECT_ | head -n-1  ; tail $FILE_NAME -n+$_MODE__SELECT_ ) | dotnet /usr/share/dotnet/sdk/5.0.104/Roslyn/bincore/csc.dll -nologo -t:library \
  -r:'../BepInEx/core/BepInEx.dll' \
  -r:'../BepInEx/core/0Harmony.dll' \
  -r:'../DSPGAME_Data/Managed/System.dll' \
  -r:'../DSPGAME_Data/Managed/UnityEngine.dll' \
  -r:'../DSPGAME_Data/Managed/UnityEngine.CoreModule.dll' \
  -r:'../DSPGAME_Data/Managed/mscorlib.dll' \
  -r:'../DSPGAME_Data/Managed/Assembly-CSharp.dll' \
  -out:'../BepInEx/plugins/'"${FILE_NAME%.*}".dll \
  -optimize \
  -
exit

#define VERBOSE
#define DEBUG



using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace PowerFull
{
    [BepInPlugin("Neutron3529.PowerFull", "PowerFull", "0.1.1")]
    public class PowerFull : BaseUnityPlugin {
        public static float power_mul=10.0f;
        public static double mecha_core_power_gen_mul = 1.0;
        public static float logistic_drone_speed_mul = 1.0f;
        public static float logistic_ship_sail_speed_mul = 1.0f;
        public static float logistic_ship_warp_speed_mul = 1.0f;
        public static float max_mining_cost = 1.0f;
        public static int extra_sand_mul = 1;//额外沙土数量，垃圾桶用
        public static float mechaReplicatePower_add = 1.0f;
        public static Player player;//用于各种神奇操作
        public static Traverse sand_count;
        public static bool enable1=true;
        public static bool enable2=true;
        public static bool enable3=true;
        public static bool enable4=true;
        public static bool game_data_import_need_patch=true;
        public static double mechaCorePowerGen = 18000.0;
        public static float logisticDroneSpeed = 8.0f;
        public static float logisticShipSailSpeed = 400.0f;
        public static float logisticShipWarpSpeed = 400000.0f;
        public static double belt_speed_mul = 2.0;
        public static double mechaReplicatePower=1.0;
#if DEBUG
        public static Action<string> logger;
#endif
        void Start() {
            mechaCorePowerGen = Configs.freeMode.mechaCorePowerGen;
            logisticDroneSpeed = Configs.freeMode.logisticDroneSpeed;
            logisticShipSailSpeed = Configs.freeMode.logisticShipSailSpeed;
            logisticShipWarpSpeed = Configs.freeMode.logisticShipWarpSpeed;
            mechaReplicatePower = Configs.freeMode.mechaReplicatePower;

            enable1 = Config.Bind<bool>("config", "enable_tech_modding", false, "开启存档科技效果修改patch（读档时生效，影响整个游戏）").Value;
            mecha_core_power_gen_mul = Config.Bind<double>("config", "mecha_core_power_gen_mul", 1000.0f, "机甲核心生成能量的速度乘数，会影响存档！").Value;
            logistic_drone_speed_mul = Config.Bind<float>("config", "logistic_drone_speed_mul", 10.0f, "行星内运输机速度乘数，会影响存档！").Value;
            logistic_ship_sail_speed_mul = Config.Bind<float>("config", "logistic_ship_sail_speed_mul", 3000.0f, "星际运输机速度乘数，会影响存档！").Value;
            logistic_ship_warp_speed_mul = Config.Bind<float>("config", "logistic_ship_warp_speed_mul", 5.0f, "星际运输机翘曲速度乘数，会影响存档！").Value;
            max_mining_cost = Config.Bind<float>("config", "max_mining_cost", 0.0f, "挖矿消耗矿物比例的最大值，可以设为0以阻止采矿机消耗矿物，会影响存档！").Value;
            mechaReplicatePower_add = Config.Bind<float>("config", "mechaReplicatePower_add", 1.0f, "机甲合成物品速度乘数，会显著影响制作速度的同时略微影响合成物品时候的能耗，会影响存档！").Value;

            enable2 = Config.Bind<bool>("config", "enable_power_mul", false, "开启电力乘数patch").Value;
            power_mul = Config.Bind<float>("config", "power_mul", 10.0f, "电力乘数").Value;

            enable3 = Config.Bind<bool>("config", "enable_extra_sand_mul", false, "开启垃圾箱patch").Value;
            extra_sand_mul = Config.Bind<int>("config", "extra_sand_mul", 1, "垃圾箱中每物品转化为沙土的数量").Value;

            enable4 = Config.Bind<bool>("config", "enable_extra_speed", false, "传送带加速").Value;
            belt_speed_mul = Config.Bind<double>("config", "belt_speed_mul", 2.0, "传送带速度乘数，会影响新建传送带，新建传送带的速度不会因读档而重置").Value;

            game_data_import_need_patch = enable1 || enable3 ;
            var harmony=new Harmony("Neutron3529.Powerful");
#if DEBUG
            logger=Logger.LogInfo;
#endif
            if (game_data_import_need_patch){// 采矿无消耗，必须打开，因为需要在这里初始化几个反射的委托
                if (!enable1){//此时注入代码仍然存在，但强制失活，这里的更改只会在log中显示，无论这里如何修改这几个变量，只要不打开enable1，后续代码将不会生效
                    mecha_core_power_gen_mul=1.0;
                    logistic_drone_speed_mul=1.0f;
                    logistic_ship_sail_speed_mul=1.0f;
                    logistic_ship_warp_speed_mul=1.0f;
                    max_mining_cost=1.0f;
                }
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
            if (enable4){// 传送带速度乘数
                //var original = typeof(CargoTraffic).GetMethod("UpgradeBeltComponent", AccessTools.all);
                //var original2 = typeof(CargoTraffic).GetMethod("NewBeltComponent", AccessTools.all);
                //var prefix = typeof(CargoTrafficNewBeltComponentPatch).GetMethod("Prefix");
                //harmony.Patch(original, new HarmonyMethod(prefix));
                //harmony.Patch(original2, new HarmonyMethod(prefix));
                harmony.PatchAll(typeof(CargoTrafficNewBeltComponentPatch));
                harmony.PatchAll(typeof(CargoTrafficUpgradeBeltComponentPatch));
#if DEBUG
                logger("PowerFull-传送带速度乘数-加载完成");
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
                    logger(string.Format("miningCostRate的原始值为{0:N5}，新值为{1:N5}，未触发修改", __instance.history.miningCostRate, max_mining_cost));
#endif
                }
                if (player.mecha.replicatePower != mechaReplicatePower+(double)mechaReplicatePower_add && enable1){
#if DEBUG
                    logger(string.Format("mecha.replicatePower的原始值为{0:N5}，触发修改，新值为{1:N5}，同时mecha.replicateSpeed由{2:N5}修改为{3:N5}", player.mecha.replicatePower,mechaReplicatePower+mechaReplicatePower_add,player.mecha.replicateSpeed,player.mecha.replicateSpeed+(float)(mechaReplicatePower+(double)mechaReplicatePower_add-player.mecha.replicatePower)));
#endif
                    player.mecha.replicateSpeed+=(float)(mechaReplicatePower+(double)mechaReplicatePower_add-player.mecha.replicatePower);
                    player.mecha.replicatePower = mechaReplicatePower+mechaReplicatePower_add;
#if DEBUG
                }else{
                    logger(string.Format("mecha.replicatePower的原始值为{0:N5}，新值为{1:N5}，未触发修改，保持mecha.replicateSpeed的原始值{2:N5}不变", player.mecha.replicatePower,mechaReplicatePower+(double)mechaReplicatePower_add, player.mecha.replicateSpeed));
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
        [HarmonyPatch(typeof(CargoTraffic), "NewBeltComponent")]
        class CargoTrafficNewBeltComponentPatch{
            public static bool Prefix(ref int speed){
#if DEBUG
#if VERBOSE
                logger(string.Format("将传送带速度从{0:N5}，修改至{1:N5}", speed, (int)(((double)speed) * belt_speed_mul)));
#endif
#endif
                speed=(int)(((double)speed) * belt_speed_mul);
                return true;
            }
        }
        [HarmonyPatch(typeof(CargoTraffic), "UpgradeBeltComponent")]
        class CargoTrafficUpgradeBeltComponentPatch{
            public static bool Prefix(ref int speed){
#if DEBUG
#if VERBOSE
                logger(string.Format("将传送带速度从{0:N5}，修改至{1:N5}", speed, (int)(((double)speed) * belt_speed_mul)));
#endif
#endif
                speed=(int)(((double)speed) * belt_speed_mul);
                return true;
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
