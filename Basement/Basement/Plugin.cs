using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using RoloPogo.Utils;
using UnityEngine;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Managers;
using Jotunn.Entities;


namespace Basement
{
    [BepInPlugin("com.rolopogo.Basement", "Basement", "1.0.2")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; private set; }
        public static Plugin instance;
        public static GameObject basementPrefab { get; private set; }
        public static ConfigEntry<int> NestedBasementLimit { get; private set; }
        public static ConfigEntry<bool> EnabledConfig { get; private set; }

        private void Awake()
        {
            instance = this;
            Log = Logger;
            EnabledConfig = Config.Bind("General", "Enabled", true, "Enables Basement");
            NestedBasementLimit= Config.Bind("General", "NestingLimit", 3, "Limit for how many nested levels of basements you can have (0 = no nesting)");

            if (EnabledConfig.Value)
            {
                PrefabManager.OnPrefabsRegistered += AddPieceToTool;
            }
        }
        
        public static void AddPieceToTool()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "com.rolopogo.Basement");

            MaterialReplacer.GetAllMaterials();

            GameObject prefabRoot = new GameObject("BasementPrefabRoot");
            DontDestroyOnLoad(prefabRoot);
            prefabRoot.SetActive(false);
            // Load from assetbundle
            var bundle = AssetBundle.LoadFromMemory(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "Basement.Resources.basement"));
            var basementAsset = bundle.LoadAsset<GameObject>("Basement");
            basementPrefab = Instantiate(basementAsset, prefabRoot.transform);
            basementPrefab.AddComponent<Basement>();
            bundle.Unload(false);

            // Force enable objects in prefab?
            foreach(Transform t in basementPrefab.GetComponentsInChildren<Transform>())
            {
                t.gameObject.SetActive(true);
            }

            basementPrefab.name = "basement.basementprefab";

            // update material references
            MaterialReplacer.ReplaceAllMaterialsWithOriginal(basementPrefab);

            var CP = new CustomPiece(basementPrefab, new PieceConfig()
            {
                AllowedInDungeons = false,
                Category = "Basement",
                CraftingStation = "piece_stonecutter",
                PieceTable = "Hammer",
                Requirements = new RequirementConfig[]
                {
                    new RequirementConfig {Amount = 100, Item = "Wood", Recover = true},
                    new RequirementConfig {Amount = 100, Item = "Stone", Recover = true}
                }
            });
            var piece = basementPrefab.GetComponent<Piece>();
            piece.m_clipEverything = true;
            // Add spawn effect
            piece.m_placeEffect = PrefabManager.Cache.GetPrefab<GameObject>("piece_stonecutter").GetComponent<Piece>().m_placeEffect;
            piece.m_repairPiece = false;

            piece.FixReferences();

            PieceManager.Instance.AddPiece(CP);
        }
    }
}
