using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace DaisyChain
{
    [BepInPlugin("com.rolopogo.DaisyChain","DaisyChain", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance;
        public ConfigEntry<bool> enabledConfig;

        private void Awake()
        {
            instance = this;
            enabledConfig = Config.Bind("General", "EnablePropogation", true, "Enables crafting stations to propagate their effects to other crafting stations in range");
            Harmony.CreateAndPatchAll(typeof(CraftingStation_Patch));
        }
    }
}
