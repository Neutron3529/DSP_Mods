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
__MODE_VERBOSE=56 # may be modified, check it carefully.
__MODE_DEBUG__=57
__MODE_RELEASE=58

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

( yes "" | head -n $_MODE__SELECT_ | head -n-1  ; tail $FILE_NAME -n+$_MODE__SELECT_ ) | $__DOTNET_CSC -nologo -t:library \
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
using System.Reflection.Emit;
using System.Collections.Generic;

namespace VoidItem
{
    [BepInPlugin("Neutron3529.VoidItem", "VoidItem", "0.1.1")]
    public class VoidItem : BaseUnityPlugin {
#if DEBUG
        public static Action<string> logger;
#endif
        void Start() {
            var harmony=new Harmony("Neutron3529.VoidItem");
            //harmony.PatchAll(typeof(StorageComponentTakeTailItemsPatch));
            var m=typeof(StorageComponent).GetMethods();
            MethodInfo method_to_patch=m[0];
            foreach(var i in m)if(i.Name=="TakeTailItems" && i.ReturnType==typeof(void)){
                method_to_patch=i;
                break;
            }
            if(method_to_patch.Name!="TakeTailItems" || method_to_patch.ReturnType!=typeof(void)){return;}
            var prefix = typeof(StorageComponentTakeTailItemsPatch).GetMethod("Prefix");
            harmony.Patch(method_to_patch, new HarmonyMethod(prefix));
#if DEBUG
            logger=Logger.LogInfo;
            logger("VoidItem加载完成");
#if VERBOSE
            //这是一个关于随机数的测试，因为我发现mono自带的System.Random生成的随机数跟我从游戏里复制的随机数函数不一样
            System.Random random = new System.Random(123);
            for(int i=0;i<100;i++)logger(String.Format("{0}",random.Next()));
#endif
#endif
        }

        //[HarmonyPatch(typeof(StorageComponent), "TakeTailItems",new Type[]{typeof(int) ,ref typeof(int),typeof(bool)})]
        class StorageComponentTakeTailItemsPatch{
            public static bool Prefix(int count, int itemId) {
                return count==0 || itemId==0;
            }
        }

    }
}
