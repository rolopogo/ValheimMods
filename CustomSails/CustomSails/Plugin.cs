using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace CustomSails
{
    [BepInPlugin("com.rolopogo.CustomSails", "CustomSails", "1.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance;
        public ConfigEntry<bool> enabledConfig;
        private ConfigEntry<bool> requireKeyConfig;
        private ConfigEntry<string> keyConfig;

        private void Awake()
        {
            instance = this;
            enabledConfig = Config.Bind("General", "Enabled", true, "Enables CustomSails");
            requireKeyConfig = Config.Bind("General", "RequireKeyPress", true, "Require holding <urlKey> to allow interaction");
            keyConfig = Config.Bind("General", "EditKey", "LeftControl", "Key to be held to allow interaction");

            if (enabledConfig.Value)
                Harmony.CreateAndPatchAll(typeof(Ship_Patch));
        }

        public bool AllowInput()
        {
            if (!requireKeyConfig.Value) return true;
            if (Enum.TryParse(keyConfig.Value, out KeyCode key))
            { 
                if (Input.GetKey(key))
                {
                    return true;
                }
            }
            return false;
        }
         
    }
}
