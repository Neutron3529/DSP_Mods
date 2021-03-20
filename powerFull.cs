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
        public static float max_mining_cost=0.0f;
#if DEBUG
        public static Action<string> logger;
#endif
        void Start() {
            power_mul = Config.Bind<float>("config", "power_mul", 10.0f, "电力乘数").Value;
            max_mining_cost = Config.Bind<float>("config", "max_mining_cost", 0.0f, "挖矿消耗的最大值，可以设为0以阻止采矿机消耗矿物").Value;
            new Harmony("Neutron3529.Powerful").PatchAll();
#if DEBUG
            logger=Logger.LogInfo;
            logger("PowerFull加载完成");
#endif
        }
        [HarmonyPatch(typeof(PowerSystem), "GameTick")]
        class PowerSystemGameTickPatch {
            public static void Postfix(PowerSystem __instance) {
                for(int i=__instance.networkServes.Length-1;i>=0;i--) {
                    __instance.networkServes[i]*=power_mul;
                }
            }
        }
        [HarmonyPatch(typeof(GameHistoryData), "Import")]
        class GameHistoryDataImportPatch{
            public static void Postfix(GameHistoryData __instance){
                if (__instance.miningCostRate > max_mining_cost ){
#if DEBUG
                    logger(string.Format("miningCostRate的原始值为{0:N5}，触发修改，新值为{1:N5}", __instance.miningCostRate, max_mining_cost));
#endif
                    __instance.miningCostRate = max_mining_cost;
#if DEBUG
                }else{
                    logger(string.Format("miningCostRate的原始值为{0:N5}，不大于设定的阈值{1:N5}", __instance.miningCostRate, max_mining_cost));
#endif
                }
            }
        }
    }
}