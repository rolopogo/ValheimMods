using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using RoloPogo.Utils;
using UnityEngine;
using JotunnLib;
using JotunnLib.Entities;
using JotunnLib.Managers;


namespace Basement
{
    [BepInPlugin("com.rolopogo.Basement", "Basement", "1.0.2")]
    [BepInDependency(JotunnLib.JotunnLib.ModGuid)]

    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; private set; }
        public static Plugin instance;
        public static GameObject BasementPrefab { get; private set; }
        public static ConfigEntry<int> NestedBasementLimit { get; private set; }
        public static ConfigEntry<bool> EnabledConfig { get; private set; }



        private void Awake()
        {
            instance = this;
            Log = Logger;
            EnabledConfig = Config.Bind("General", "Enabled", true, "Enables Basement");
            NestedBasementLimit= Config.Bind("General", "NestingLimit", 3, "Limit for how many nested levels of basements you can have (0 = no nesting)");
            basementshit();
                //put jotunn do stuff here
                PrefabManager.Instance.PrefabRegister += registerPrefabs;
                ObjectManager.Instance.ObjectRegister += registerObjects;              
                PieceManager.Instance.PieceRegister += registerPieces;
        }

        private void registerPrefabs(object sender, EventArgs e)
        {
            PrefabManager.Instance.RegisterPrefab(Plugin.BasementPrefab, "Basement");
        }
        private void registerPieces(object sender, EventArgs e)
        {
            PieceManager.Instance.RegisterPiece("Hammer", "Basement");
        }

        private void registerObjects(object sender, EventArgs e)
        {
            // Items
            ObjectManager.Instance.RegisterItem("Basement");
        }

        private static void basementshit()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "com.rolopogo.Basement");

            

            GameObject prefabRoot = new GameObject("BasementPrefabRoot");
            prefabRoot.SetActive(false);
            // Load from assetbundle
            AssetBundle assetBundle = AssetBundle.LoadFromMemory(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "Basement.Resources.basement"));
            Plugin.BasementPrefab = assetBundle.LoadAsset<GameObject>("Basement");
            assetBundle.Unload(false);

            // Force enable objects in prefab?
            foreach (Transform t in Plugin.BasementPrefab.GetComponentsInChildren<Transform>())
            {
                t.gameObject.SetActive(true);
            }

            BasementPrefab.name = "Basement.prefab";
           
        }
        public static bool IsObjectDBReady()
        {
            // Hack, just making sure the built-in items and prefabs have loaded
            return ObjectDB.instance != null && ObjectDB.instance.m_items.Count != 0 && ObjectDB.instance.GetItemPrefab("Amber") != null;
        }

        public static void Replacemats()
        {
            Log.LogDebug("Loading Material Replacements");
            // update material references
            if (!IsObjectDBReady())
            {
                return;
            }
            if (IsObjectDBReady())
            {
                MaterialReplacer.GetAllMaterials();
                MaterialReplacer.ReplaceAllMaterialsWithOriginal(Plugin.BasementPrefab);
            }
        }

    }
}
