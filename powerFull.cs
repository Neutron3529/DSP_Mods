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
    [BepInPlugin("Neutron3529.Cheat", "PowerFull", "0.2.1")]
    public class PowerFull : BaseUnityPlugin {
        public static float power_mul=10.0f;
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
            if ((Config.Bind<bool>("config", "mining_no_cost", true, "挖矿不消耗资源，且星际站点挖矿加速（CodeMatcher）").Value)){
                harmony.PatchAll(typeof(PlanetTransportGameTick));
                harmony.PatchAll(typeof(FactorySystemGameTick));
#if DEBUG
                logger("PowerFull-采矿修改-加载完成");
#endif
            }
            if ((Config.Bind<bool>("config", "mecha_craft_very_fast", true, "机甲瞬间合成").Value)){
                harmony.PatchAll(typeof(MechaForgeGameTick));
#if DEBUG
                logger("PowerFull-机甲瞬间合成-加载完成");
#endif
            }
            if ((Config.Bind<bool>("config", "free_sail", true, "机甲自由曲速（CodeMatcher）").Value)){
                harmony.PatchAll(typeof(PlayerMove_SailGameTick));
                harmony.PatchAll(typeof(MechaUseWarper));
                harmony.PatchAll(typeof(PlayerMove_SailUseWarpEnergy));
#if DEBUG
                logger("PowerFull-机甲自由曲速-加载完成");
#endif
            }
            if ((Config.Bind<bool>("config", "MechaLabGameTick", true, "机甲研究室加速研究（CodeMatcher）").Value)){
                harmony.PatchAll(typeof(MechaLabGameTick));
#if DEBUG
                logger("PowerFull-机甲研究室加速研究-加载完成");
#endif
            }
            if ((Config.Bind<bool>("config", "after_finished_the_game", true, "开启二周目矩阵支援，在研究电磁学技术之后，获得成吨的矩阵，适量的解锁物品，几个矩阵实验室，和物流站").Value)){
                harmony.PatchAll(typeof(GameHistoryDataGainTechAwards));
#if DEBUG
                logger("PowerFull-二周目矩阵支援-加载完成");
#endif
            }
#if DEBUG
            logger("PowerFull-加载完成");
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
                    __instance.networkServes[i]=power_mul;
                }
            }
        }
        [HarmonyPatch(typeof(PlanetTransport), "GameTick")] // https://github.com/BepInEx/HarmonyX/wiki/Transpiler-helpers
        public static class PlanetTransportGameTick {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                return new CodeMatcher(instructions)
                    .MatchForward(false, // false = move at the start of the match, true = move at the end of the match
                        new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "miningSpeedScale") // match method 1
                    ).Repeat( matcher => // Do the following for each match
                        matcher
                        .Advance(1) // Move cursor to after ldfld
                        .InsertAndAdvance(
                            new CodeInstruction(OpCodes.Ldc_R4,10f),
                            new CodeInstruction(OpCodes.Mul)
                        )
                    ).InstructionEnumeration();
            }
        }

        [HarmonyPatch(typeof(FactorySystem), "GameTick", new Type[]{ typeof(long), typeof(bool) })]
        [HarmonyPatch(typeof(FactorySystem), "GameTick", new Type[]{ typeof(long), typeof(bool), typeof(int), typeof(int), typeof(int) })]
        public static class FactorySystemGameTick {// used for 2  FactorySystem functions.
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                return new CodeMatcher(instructions)
                    .MatchForward(false, // false = move at the start of the match, true = move at the end of the match
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameHistoryData),"miningCostRate")) // match method 2
                    ).Repeat( matcher => // Do the following for each match
                        matcher
                        .Advance(1) // Move cursor to after ldfld
                        .InsertAndAdvance(
                            new CodeInstruction(OpCodes.Pop),
                            new CodeInstruction(OpCodes.Ldc_R4,0f)
                        )
                    ).InstructionEnumeration();
            }
        }
        [HarmonyPatch(typeof(MechaForge), "GameTick")]
        public static class MechaForgeGameTick {
            public static void Prefix(ref MechaForge __instance) {
                if (__instance.tasks.Count > 0) {
                    __instance.tasks[0].tick=__instance.tasks[0].tickSpend;
                }
            }
//             static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
//                 return new CodeMatcher(instructions)
//                     /*
//                     .MatchForward(false, // false = move at the start of the match, true = move at the end of the match
//                         new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(ForgeTask),"tick"))
//                     ).Repeat( matcher => matcher// Do the following for each match
//                         .InsertAndAdvance(
//                             new CodeInstruction(OpCodes.Pop),
//                             new CodeInstruction(OpCodes.Dup),
//                             new CodeInstruction(OpCodes.Ldfld,AccessTools.Field(typeof(ForgeTask),"tickSpend"))
//                         )
//                     ).InstructionEnumeration();*/
//                     .MatchForward(false,
//                         new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ForgeTask),"tickSpend"))
//                     ).Repeat( matcher => matcher
//                         .SetOperandAndAdvance(AccessTools.Field(typeof(ForgeTask),"tick"))
//                     ).InstructionEnumeration();
//             }
        }
        [HarmonyPatch(typeof(PlayerMove_Sail), "GameTick")]
        public static class PlayerMove_SailGameTick {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                return new CodeMatcher(instructions)
                    .MatchForward(false, // false = move at the start of the match, true = move at the end of the match
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Mecha),"thrusterLevel")),
                        new CodeMatch(OpCodes.Ldc_I4_3),
                        new CodeMatch(i => i.opcode == OpCodes.Blt)
                    ).Advance(
                        1
                    ).SetOpcodeAndAdvance(
                        OpCodes.Ldc_I4_0
                    ).InstructionEnumeration();
            }
        }
        [HarmonyPatch(typeof(Mecha), "UseWarper")]
        class MechaUseWarper {
            public static bool Prefix(ref bool __result) {
                __result=true;
                return false;
            }
        }
        [HarmonyPatch(typeof(PlayerMove_Sail), "UseWarpEnergy")]
        class PlayerMove_SailUseWarpEnergy {
            public static bool Prefix(ref bool __result) {
                __result=true;
                return false;
            }
        }
        [HarmonyPatch(typeof(MechaLab), "GameTick")]
        class MechaLabGameTick {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                return new CodeMatcher(instructions)
                    .MatchForward(false, // false = move at the start of the match, true = move at the end of the match
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(MechaLab),"gameHistory")),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameHistoryData),"techSpeed"))
                    ).Advance(1).SetAndAdvance(
                        OpCodes.Pop,null
                    ).SetAndAdvance(
                        OpCodes.Ldc_I4,14400
                    ).InstructionEnumeration();
            }
        }
        [HarmonyPatch(typeof(GameHistoryData), "GainTechAwards")]
        class GameHistoryDataGainTechAwards {
            static PlanetFactory nearestFactory;
            static Mecha mecha;
            static int randSeed=0;
            public static void AddItems(int id,int count){
                mecha.AddProductionStat(id,count,nearestFactory);
                GameMain.mainPlayer.TryAddItemToPackage(id,count, count*4, true, 0);
                UIItemup.Up(id, count);
            }
            public static void AddStats(int id,int count){
                mecha.AddProductionStat(id,count,nearestFactory);
                mecha.AddConsumptionStat(id,count,nearestFactory);
            }
            public static void AddTrashes(int id,int quantity,int count){
                mecha.AddProductionStat(id,count*quantity,nearestFactory);
                VectorLF3 vectorLF = Maths.QRotateLF(GameMain.mainPlayer.uRotation, new VectorLF3(0f, 1f, 0f));
                int nearStarId = (GameMain.data.localStar != null) ? (GameMain.data.localStar.id * 100) : 0;
                double nearStarGravity = (GameMain.data.localStar != null) ? GameMain.data.trashSystem.GetStarGravity(GameMain.data.localStar.id) : 0.0;
                for(int i=0;i<count;i++){
                    TrashObject trashObj = new TrashObject(id, quantity, quantity*4, Vector3.zero, Quaternion.identity);
                    TrashData trashData = default(TrashData);
                    trashData.landPlanetId = 0;
                    trashData.nearPlanetId = 0;
                    trashData.nearStarId = nearStarId;
                    trashData.nearStarGravity = nearStarGravity;
                    trashData.lPos = Vector3.zero;
                    trashData.lRot = Quaternion.identity;
                    trashData.uPos = GameMain.mainPlayer.uPosition + RandomTable.SphericNormal(ref randSeed, 0.8);
                    trashData.uRot = Quaternion.LookRotation(RandomTable.SphericNormal(ref randSeed, 1.0).normalized, vectorLF);
                    trashData.uVel = GameMain.mainPlayer.uVelocity + RandomTable.SphericNormal(ref randSeed, 8.0) + vectorLF * 15.0;
                    trashData.uAgl = RandomTable.SphericNormal(ref randSeed, 0.03);
                    GameMain.data.trashSystem.container.NewTrash(trashObj, trashData);
                }
            }
            public static void Postfix(int itemId) {
                if(itemId==2301){
                    randSeed=0;
                    nearestFactory = GameMain.mainPlayer.nearestFactory;
                    mecha = GameMain.mainPlayer.mecha;
                    GameHistoryDataGainTechAwards.AddStats(1120,9000000);//氢
                    GameHistoryDataGainTechAwards.AddStats(1121,2000000);//重氢
                    GameHistoryDataGainTechAwards.AddStats(1126, 200000);//卡晶
                    GameHistoryDataGainTechAwards.AddStats(1304, 200000);//阴间过滤器
                    GameHistoryDataGainTechAwards.AddItems(1202, 30+150-10);//磁线圈，升级用，有10个多余（因为我们已经支付了这一项科技的成本）
                    GameHistoryDataGainTechAwards.AddItems(1301, 40+220);//电路板，升级用
                    GameHistoryDataGainTechAwards.AddItems(1201, 20+  0);//  齿轮，升级用
                    GameHistoryDataGainTechAwards.AddItems(1101,  0+ 20);//  铁块，升级用
                    GameHistoryDataGainTechAwards.AddItems(1104,  0+ 20);//  铜块，升级用
                    GameHistoryDataGainTechAwards.AddItems(1203,  0+ 60);//电动机，升级用
                    GameHistoryDataGainTechAwards.AddItems(1006,  0+210);//  煤矿，升级用
                    GameHistoryDataGainTechAwards.AddItems(1103,  0+120);//  钢材，升级用
                    GameHistoryDataGainTechAwards.AddItems(1030,  0+ 60);//  木材，升级用
                    GameHistoryDataGainTechAwards.AddItems(1109,  0+ 60);//高能石墨升级用

                    GameHistoryDataGainTechAwards.AddItems(6001,400+1400);//初始升级用矩阵
                    GameHistoryDataGainTechAwards.AddItems(6002,400+1400);
                    GameHistoryDataGainTechAwards.AddItems(6003,300+1200);

                    GameHistoryDataGainTechAwards.AddItems(2104,  10);//物流站，10个
                    GameHistoryDataGainTechAwards.AddItems(2316,  10);//大型采矿机，10个
                    GameHistoryDataGainTechAwards.AddItems(5002, 100);//大飞机，100个
                    GameHistoryDataGainTechAwards.AddItems(2003, 300);//蓝带，300个


                    GameHistoryDataGainTechAwards.AddStats(1403,     60);//湮灭约束球
                    GameHistoryDataGainTechAwards.AddItems(1803, 120);//燃料棒
                    GameHistoryDataGainTechAwards.AddItems(2001,   1);//黄带，1个，用于治疗强迫症

                    GameHistoryDataGainTechAwards.AddTrashes(6001,4000,50);
                    GameHistoryDataGainTechAwards.AddTrashes(6002,4000,50);
                    GameHistoryDataGainTechAwards.AddTrashes(6003,4000,50);
                    GameHistoryDataGainTechAwards.AddTrashes(6004,4000,50);
                    GameHistoryDataGainTechAwards.AddTrashes(6005,4000,50);
                }
            }
        }
    }
}
