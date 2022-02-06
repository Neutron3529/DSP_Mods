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

__MODE_VERBOSE=77 # is the line number of "#define VERBOSE", may be modified
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
  - && rm -f "../BepInEx/config/Neutron3529.Cheat.cfg";

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
    [BepInPlugin("Neutron3529.Cheat", "PowerFull", "0.2.2")]
    public class PowerFull : BaseUnityPlugin {
        public static float power_mul=10.0f;
        public static int techSpeed=900000000;
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
            if ((techSpeed=Config.Bind<int>("config", "MechaLabtechSpeed", 900000000, "机甲研究室研究速度（CodeMatcher）").Value)>0){
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
            if ((Config.Bind<bool>("config", "CheckBuildConditions_water", true, "允许抽水站抽地下水，允许在水上建造建筑").Value)){
                harmony.PatchAll(typeof(BuildTool_ClickBuildTool_BlueprintPasteCheckBuildConditions));
#if DEBUG
                logger("PowerFull-去除地形限制-加载完成");
#endif
            }
            if ((Config.Bind<bool>("config", "inf_matrix", false, "特定物品的消耗改为虚空创造").Value)){
                harmony.PatchAll(typeof(StorageComponentTakeTailItems));
#if DEBUG
                logger("PowerFull-特定物品的消耗改为虚空创造-加载完成");
#endif
            }else{
#if DEBUG
                logger("PowerFull-特定物品的消耗改为虚空创造-默认不加载");
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
                            new CodeInstruction(OpCodes.Ldc_R4,50f),
                            new CodeInstruction(OpCodes.Mul)
                        )
                    ).InstructionEnumeration();
            }
        }
        [HarmonyPatch]
//         [HarmonyPatch(typeof(FactorySystem), "GameTick", new Type[]{ typeof(long), typeof(bool) })]
//         [HarmonyPatch(typeof(FactorySystem), "GameTick", new Type[]{ typeof(long), typeof(bool), typeof(int), typeof(int), typeof(int) })]
        public static class FactorySystemGameTick {// used for 2  FactorySystem functions.
            static IEnumerable<MethodBase> TargetMethods()
            {
                MethodInfo[] m=typeof(FactorySystem).GetMethods();
                foreach(MethodInfo i in m)if(i.Name=="GameTick"){
                    yield return i;
                }
            }
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
                        OpCodes.Ldc_I4,techSpeed
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
//                     GameHistoryDataGainTechAwards.AddStats(1120,9000000);//氢
//                     GameHistoryDataGainTechAwards.AddStats(1121,2000000);//重氢
//                     GameHistoryDataGainTechAwards.AddStats(1126, 200000);//卡晶
//                     GameHistoryDataGainTechAwards.AddStats(1304, 200000);//阴间过滤器
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
                    GameHistoryDataGainTechAwards.AddStats(1403,     60);//湮灭约束球
                    GameHistoryDataGainTechAwards.AddItems(1803, 120);//燃料棒
                    GameHistoryDataGainTechAwards.AddItems(2001,   1);//黄带，1个，用于治疗强迫症
//
//                     GameHistoryDataGainTechAwards.AddItems(6001,400+1400);//初始升级用矩阵
//                     GameHistoryDataGainTechAwards.AddItems(6002,400+1400);
//                     GameHistoryDataGainTechAwards.AddItems(6003,300+1200);

                    GameHistoryDataGainTechAwards.AddItems(2103,  40);//物流站，40个
                    GameHistoryDataGainTechAwards.AddItems(2104,  10);//物流站，10个
                    GameHistoryDataGainTechAwards.AddItems(2105,  40);//物流站，40个
                    GameHistoryDataGainTechAwards.AddItems(2316,  20);//大型采矿机，20个
                    GameHistoryDataGainTechAwards.AddItems(5001,3000);//小飞机，100个
                    GameHistoryDataGainTechAwards.AddItems(5002, 100);//大飞机，100个
                    GameHistoryDataGainTechAwards.AddItems(2003,3000);//蓝带，3000个



//                     GameHistoryDataGainTechAwards.AddTrashes(6001,1000,156);
//                     GameHistoryDataGainTechAwards.AddTrashes(6002,1000,157);
//                     GameHistoryDataGainTechAwards.AddTrashes(6003,1000,124);
//                     GameHistoryDataGainTechAwards.AddTrashes(6004,1000,90+1);
//                     GameHistoryDataGainTechAwards.AddTrashes(6005,1000,((int)67.5)+1);
                }
            }
        }
        [HarmonyPatch]
        class BuildTool_ClickBuildTool_BlueprintPasteCheckBuildConditions {
            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return typeof(BuildTool_Click).GetMethod("CheckBuildConditions", AccessTools.all);
                yield return typeof(BuildTool_BlueprintPaste).GetMethod("CheckBuildConditions", AccessTools.all);
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                return new CodeMatcher(instructions)
                    .MatchForward(false, // false = move at the start of the match, true = move at the end of the match
                        new CodeMatch(i => CodeInstructionExtensions.LoadsConstant(i,EBuildCondition.NeedWater) || CodeInstructionExtensions.LoadsConstant(i,EBuildCondition.NeedGround)),
                        new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(BuildPreview),"condition"))
                    ).Repeat( matcher => matcher// Do the following for each match
                        .SetOperandAndAdvance(
                            (int)(EBuildCondition.Ok)
                        )
                    ).InstructionEnumeration();
            }
        }
        [HarmonyPatch(typeof(StorageComponent), "TakeTailItems",new Type[]{typeof(int),typeof(int),typeof(int),typeof(bool)},new ArgumentType[]{ArgumentType.Ref,ArgumentType.Ref,ArgumentType.Out,ArgumentType.Normal})]
        class StorageComponentTakeTailItems {
            static PlanetFactory nearestFactory;
            static Mecha mecha;
            static Dictionary<int,int[,]> dict = new Dictionary<int,int[,]> {
                {1101,new int [,]{{1001,1}}},//铁
                {1102,new int [,]{{1001,1}}},//磁铁
                {1104,new int [,]{{1002,1}}},//铜
                {1202,new int [,]{{1102,1},{1104,1}}},//磁线圈，多耗费0.5铜
                {1301,new int [,]{{1101,1},{1104,1}}},//电路板，多耗费0.5铜
                {6001,new int [,]{{1202,1},{1301,1}}},//=磁线圈 电路板
                {1109,new int [,]{{1006,2}}},//高能石墨
                {6002,new int [,]{{1109,2},{1120,2}}},
                {1112,new int [,]{{1109,1}}},//金刚石
                {1106,new int [,]{{1004,2}}},//钛
                {1118,new int [,]{{1106,3},{1117,1}}},//钛晶石
                {6003,new int [,]{{1112,1},{1118,1}}},
                {1105,new int [,]{{1003,2}}},//硅
                {1302,new int [,]{{1105,2},{1104,1}}},//微晶元件
                {1303,new int [,]{{1302,2},{1301,2}}},//处理器
                {1124,new int [,]{{1015,1}}},//碳纳米管
                {1113,new int [,]{{1105,1}}},//晶格硅
                {1114,new int [,]{{1007,1}}},//精炼油，未计算副产物氢
                {1115,new int [,]{{1114,2},{1109,1}}},//塑料
                {1402,new int [,]{{1124,2},{1113,2},{1115,1}}},//粒子宽带
                {6004,new int [,]{{1303,2},{1402,1}}},
                {6005,new int [,]{{1126,1},{1304,1},{1305,1},{1305,1},{1209,1},{1127,1}}},//未完
                {6006,new int [,]{{6001,1},{6002,1},{6003,1},{6004,1},{6005,1},{1122,1},{1208,1}}},
            };
            public static void AddItems(int id,int count){
                if(dict.ContainsKey(id)){
                    int[,] x=dict[id];
                    int len=x.GetLength(0);
                    for (int i = 0; i < len; i++){
                        StorageComponentTakeTailItems.AddItems(x[i,0],count*x[i,1]);
                        mecha.AddConsumptionStat(x[i,0],count*x[i,1],nearestFactory);
                    }
                }else{
                    StorageComponentTakeTailItems.AddStats(id,count);
                }
                mecha.AddProductionStat(id,count,nearestFactory);
            }
            public static void AddStats(int id,int count){
                mecha.AddProductionStat(id,count,nearestFactory);
                mecha.AddConsumptionStat(id,count,nearestFactory);
            }
            public static bool Prefix(ref int itemId, ref int count) {
                switch(itemId){
                    case 6001 :
                    case 6002 :
                    case 6003 :
                    case 6004 :
                    case 6005 :
                    case 6006 :
                        nearestFactory = GameMain.mainPlayer.nearestFactory;
                        mecha = GameMain.mainPlayer.mecha;
                        StorageComponentTakeTailItems.AddItems(itemId,count);
                        return false;
                    default : /* 可选的 */
                    return true;
                }
            }
        }
    }
}
