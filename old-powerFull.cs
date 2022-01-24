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
    export __DOTNET_CSC="`find /usr/share/dotnet -type f -name dotnet` `find /usr/share/dotnet -name csc.dll`"
    echo '$'"__DOTNET_CSC not set yet, you should execute"
    echo "export __DOTNET_CSC='$__DOTNET_CSC'"
    echo "manually, or this alert will occur each time you execute this script."
fi

__MODE_VERBOSE=75 # is the line number of "#define VERBOSE", may be modified
__MODE_DEBUG__=$((__MODE_VERBOSE+1))
__MODE_RELEASE=$((__MODE_DEBUG__+1))

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

export GAME_NAME="DSPGAME"
export FILE_NAME="$0"

( yes "" | head -n $_MODE__SELECT_ | head -n-1  ; tail $FILE_NAME -n+$_MODE__SELECT_ ) | $__DOTNET_CSC -nologo -t:library \
  -r:'../BepInEx/core/BepInEx.dll' \
  -r:'../BepInEx/core/0Harmony.dll' \
  -r:'../BepInEx/core/BepInEx.Harmony.dll' \
  -r:"../${GAME_NAME}_Data/Managed/netstandard.dll" \
  -r:"../${GAME_NAME}_Data/Managed/System.dll" \
  -r:"../${GAME_NAME}_Data/Managed/System.Core.dll" \
  -r:"../${GAME_NAME}_Data/Managed/UnityEngine.dll" \
  -r:"../${GAME_NAME}_Data/Managed/UnityEngine.CoreModule.dll" \
  -r:"../${GAME_NAME}_Data/Managed/mscorlib.dll" \
  -r:"../${GAME_NAME}_Data/Managed/Assembly-CSharp.dll" \
  `[ -e "../${GAME_NAME}_Data/Managed/Assembly-CSharp-firstpass.dll" ] && echo "-r:\"../${GAME_NAME}_Data/Managed/Assembly-CSharp-firstpass.dll\""` \
  -out:'../BepInEx/plugins/'"${FILE_NAME%.*}".dll \
  -optimize \
  - && ( rm "../BepInEx/config/Neutron3529.Cheat.cfg" 2>/dev/null )

if [ -n "$2" ]; then
    git add ${FILE_NAME}
    case $2 in
        R) git commit -am "`curl -s http://whatthecommit.com/index.txt`" ;;
        r) git commit -am "`curl -s http://whatthecommit.com/index.txt`" ;;
        RANDOM) git commit -am "`curl -s http://whatthecommit.com/index.txt`" ;;
        random) git commit -am "`curl -s http://whatthecommit.com/index.txt`" ;;
        U) git commit -am "`curl -s http://whatthecommit.com/index.txt`" ;;
        u) git commit -am "`curl -s http://whatthecommit.com/index.txt`" ;;
        UPLOAD) git commit -am "`curl -s http://whatthecommit.com/index.txt`" ;;
        upload) git commit -am "`curl -s http://whatthecommit.com/index.txt`" ;;
        *) git commit -am "$2" ;;
    esac
    git push
fi
exit

#define VERBOSE // the line of __MODE_VERBOSE
#define DEBUG



using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace PowerFull
{
    [BepInPlugin("Neutron3529.Cheat", "PowerFull", "0.2.0")]
    public class PowerFull : BaseUnityPlugin {
        public static float power_mul=10.0f;/*
        public static double mecha_core_power_gen_mul = 1.0;
        public static float logistic_drone_speed_mul = 1.0f;
        public static float logistic_ship_sail_speed_mul = 1.0f;
        public static float logistic_ship_warp_speed_mul = 1.0f;
        public static float max_mining_cost = 1.0f;
        public static int extra_sand_mul = 1;//额外沙土数量，垃圾桶用
        public static float mechaReplicatePower_add = 1.0f;
        public static Player player;//用于各种神奇操作
        public static Traverse sand_count;
        public static bool enable1=false;
        public static bool enable2=false;
        public static bool enable3=false;
        public static bool enable4=false;
        public static bool enable5=false;
        public static bool enable6=false;
        public static bool game_data_import_need_patch=false;
        public static double mechaCorePowerGen = 18000.0;
        public static float logisticDroneSpeed = 8.0f;
        public static float logisticShipSailSpeed = 400.0f;
        public static float logisticShipWarpSpeed = 400000.0f;
        public static double belt_speed_mul = 2.0;
        public static double mechaReplicatePower=1.0;
        public static int ArequireCounts=5;
        public static int LrequireCounts=36000;
        public static int Lspeed=3600;*/
#if DEBUG
        public static Action<string> logger;
#endif
        void Start() {
            var harmony=new Harmony("Neutron3529.Powerful");
#if DEBUG
            logger=Logger.LogInfo;
#endif
            if ((power_mul = Config.Bind<float>("config", "power_mul", 180.0f, "电力乘数，大于0时开启").Value)>0f){
                var original = typeof(PowerSystem).GetMethod("GameTick", AccessTools.all);
                var postfix = typeof(PowerSystemGameTickPatch).GetMethod("Postfix");
                harmony.Patch(original, null, new HarmonyMethod(postfix));
                harmony.PatchAll(typeof(LabComponentInternalUpdateResearch_Fix)); // fix: 实验室可以正确应用增产剂而不触发异常检测。
#if DEBUG
                logger("PowerFull-电力x10-加载完成");
#endif
            }
            if ((Config.Bind<bool>("config", "mining_no_cost", true, "挖矿不消耗资源，且星际站点挖矿加速").Value)){
                harmony.PatchAll(typeof(PlanetTransportGameTick));
                harmony.PatchAll(typeof(FactorySystemGameTick));
#if DEBUG
                logger("PowerFull-采矿修改-加载完成");
#endif
            }
#if DEBUG
            logger("PowerFull-加载完成");
#endif
            /*
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

            enable5 = Config.Bind<bool>("config", "enable_ArequireCounts_mul", false, "自动化工厂需求数量").Value;
            ArequireCounts = Config.Bind<int>("config", "arequireCounts", 5, "设置自动化工厂的爪子堆叠数量，默认为5，不影响存档").Value;

            enable6 = Config.Bind<bool>("config", "enable_RrequireCounts_mul", false, "矩阵研究站需求数量").Value;
            LrequireCounts = Config.Bind<int>("config", "lrequireCounts", 36000, "设置矩阵研究站的爪子堆叠数量，以3600为一个矩阵，需求矩阵的个数不必是整数，不影响存档").Value;
            Lspeed = Config.Bind<int>("config", "lsp", 3600, "每次下层矩阵研究站向上层矩阵研究站传递矩阵块的数量，不建议高于lrequireCounts（因为游戏不会额外检查矩阵是否足够），建议改成lrequireCounts的一半或者3/4，不影响存档").Value;

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
            if (enable4){
                harmony.PatchAll(typeof(CargoTrafficNewBeltComponentPatch));
                harmony.PatchAll(typeof(CargoTrafficUpgradeBeltComponentPatch));
#if DEBUG
                logger("PowerFull-传送带速度乘数-加载完成");
#endif
            }
            if (enable5){
                harmony.PatchAll(typeof(AssemblerComponentUpdateNeedsPatch));
#if DEBUG
                logger("PowerFull-自动化工厂需求数量-加载完成");
#endif
            }
            if (enable6){
                harmony.PatchAll(typeof(LabComponentUpdateNeedsResearchPatch));
                harmony.PatchAll(typeof(LabComponentUpdateOutputToNextPatch));
#if DEBUG
                logger("PowerFull-矩阵研究站需求数量-加载完成");
#endif
            }*/
#if DEBUG
            logger("PowerFull加载完成");
#endif
        }
        [HarmonyPatch(typeof(LabComponent), "InternalUpdateResearch")]
        class LabComponentInternalUpdateResearch_Fix {
            public static bool Prefix(ref float power,float speed) {
                power = (float)((int)(speed + 2f))/speed;
                return true;
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
        [HarmonyPatch(typeof(PlanetTransport), "GameTick")] // https://github.com/BepInEx/HarmonyX/wiki/Transpiler-helpers
        public static class PlanetTransportGameTick
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                int counter=0;
                var codes = new List<CodeInstruction>(instructions);
                foreach (var instruction in instructions)
                {
                    yield return instruction;
                    if (instruction.opcode == OpCodes.Ldfld && ((FieldInfo)instruction.operand).Name == "miningSpeedScale" )
                    {
                        counter+=1;
                        yield return new CodeInstruction(OpCodes.Ldc_R4,10f);
                        yield return new CodeInstruction(OpCodes.Mul);
                    }
                }
#if DEBUG
                logger(string.Format("将对miningSpeedScale的读取pop掉之后填0，共修改了{0:N5}处IL代码", counter));
#endif
                yield break;
            }
        }

        [HarmonyPatch(typeof(FactorySystem), "GameTick", new Type[]{ typeof(long), typeof(bool) })]
        [HarmonyPatch(typeof(FactorySystem), "GameTick", new Type[]{ typeof(long), typeof(bool), typeof(int), typeof(int), typeof(int) })]
        public static class FactorySystemGameTick// used for 2  FactorySystem functions.
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                int counter=0;
                var codes = new List<CodeInstruction>(instructions);
                foreach (var instruction in instructions)
                {
                    yield return instruction;
                    if (instruction.opcode == OpCodes.Ldfld && ((FieldInfo)instruction.operand).Name == "miningCostRate" )
                    {
                        counter+=1;
                        yield return new CodeInstruction(OpCodes.Pop);
                        yield return new CodeInstruction(OpCodes.Ldc_R4,0f);
                    }
                }
#if DEBUG
                logger(string.Format("将对miningCostRate的读取pop掉之后填0，共修改了{0:N5}处IL代码", counter));
#endif
                yield break;
            }
        }/*
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
        [HarmonyPatch(typeof(AssemblerComponent), "UpdateNeeds")]
        class AssemblerComponentUpdateNeedsPatch{
            public static bool Prefix(AssemblerComponent __instance) {
                int i=0;
                int l=__instance.requires.Length;
                for(;i<l;i++)
                    __instance.needs[i] = (( __instance.served[i] >= __instance.requireCounts[i] * ArequireCounts) ? 0 : __instance.requires[i]);
                for(;i!=6;i++)
                    __instance.needs[i] = 0;
                return false;
            }
        }
        [HarmonyPatch(typeof(LabComponent), "UpdateNeedsResearch")]
        class LabComponentUpdateNeedsResearchPatch{
            public static bool Prefix(LabComponent __instance) {
                for(int i=0;i<6;i++)
                    __instance.needs[i] = (( __instance.matrixServed[i] >= LrequireCounts) ? 0 : 6001+i);
                return false;
            }
        }
        [HarmonyPatch(typeof(LabComponent), "UpdateOutputToNext")]
        public static class LabComponentUpdateOutputToNextPatch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                int counter=0;
                var codes = new List<CodeInstruction>(instructions);
                foreach (var instruction in instructions)
                {
                    if (instruction.opcode == OpCodes.Ldc_I4 && (int)(instruction.operand) == 3600)
                    {
                        counter+=1;
                        instruction.operand = Lspeed;
                    }
                    yield return instruction;
                }
#if DEBUG
                logger(string.Format("将每次传送白糖速度从{0:N5}，修改至{1:N5}，共修改了{2:N5}处IL代码", 3600, Lspeed, counter));
#endif
            }
        }*/
    }
}
