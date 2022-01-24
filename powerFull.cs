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
            if ((Config.Bind<bool>("config", "mining_no_cost", true, "挖矿不消耗资源，且星际站点挖矿加速").Value)){
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
                    __instance.networkServes[i]*=power_mul;
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
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                return new CodeMatcher(instructions)
                    /*
                    .MatchForward(false, // false = move at the start of the match, true = move at the end of the match
                        new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(ForgeTask),"tick"))
                    ).Repeat( matcher => matcher// Do the following for each match
                        .InsertAndAdvance(
                            new CodeInstruction(OpCodes.Pop),
                            new CodeInstruction(OpCodes.Dup),
                            new CodeInstruction(OpCodes.Ldfld,AccessTools.Field(typeof(ForgeTask),"tickSpend"))
                        )
                    ).InstructionEnumeration();*/
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ForgeTask),"tickSpend"))
                    ).Repeat( matcher => matcher
                        .SetOperandAndAdvance(AccessTools.Field(typeof(ForgeTask),"tick"))
                    ).InstructionEnumeration();
            }
        }
    }
}
