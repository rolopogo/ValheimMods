using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using RoloPogo.Utils;
using UnityEngine;
using ValheimLib;
using ValheimLib.ODB;

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
                
                ObjectDBHelper.OnAfterInit += AddPieceToTool;
            }
        }
        
        public static void AddPieceToTool()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "com.rolopogo.Basement");

            MaterialReplacer.GetAllMaterials();

            // Load from assetbundle
            var bundle = AssetBundle.LoadFromMemory(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "Basement.Resources.basement"));
            var basementAsset = bundle.LoadAsset<GameObject>("Basement");
            GameObject basementPrefab = basementAsset.InstantiateClone("basement.basementprefab");

            basementPrefab.AddComponent<Basement>();

            // update material references
            MaterialReplacer.ReplaceAllMaterialsWithOriginal(basementPrefab);

            var woodRequirement = MockRequirement.Create("Wood", 100, true);
            woodRequirement.FixReferences();
            var stoneRequirement = MockRequirement.Create("Stone", 100, true);
            stoneRequirement.FixReferences();

            var customRequirements = new Piece.Requirement[]
            {
                woodRequirement,
                stoneRequirement
            };

            var piece = basementPrefab.GetComponent<Piece>();
            piece.m_resources = customRequirements;
            piece.m_category = Piece.PieceCategory.Misc;
            piece.m_craftingStation = Mock<CraftingStation>.Create("piece_stonecutter");
            piece.m_clipEverything = true;
            // Add spawn effect
            piece.m_placeEffect = Prefab.Cache.GetPrefab<GameObject>("piece_stonecutter").GetComponent<Piece>().m_placeEffect;
            piece.m_repairPiece = false;

            piece.FixReferences();

            // Add to tool
            GameObject hammerPrefab = Prefab.Cache.GetPrefab<GameObject>("_HammerPieceTable");
            PieceTable hammerTable = hammerPrefab.GetComponent<PieceTable>();

            hammerTable.m_pieces.Add(basementPrefab.gameObject);

            bundle.Unload(false);
        }
    }
}
