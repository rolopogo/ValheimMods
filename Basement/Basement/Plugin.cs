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
    [BepInPlugin("com.rolopogo.Basement", "Basement", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; private set; }
        public static Plugin instance;
        public static GameObject basementPrefab { get; private set; }

        public ConfigEntry<bool> enabledConfig;

        private void Awake()
        {
            instance = this;
            Log = Logger;
            enabledConfig = Config.Bind("General", "Enabled", true, "Enables Basement");

            if (enabledConfig.Value)
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
            basementPrefab = bundle.LoadAsset<GameObject>("Basement");
            basementPrefab.AddComponent<Basement>();
            bundle.Unload(false);

            Plugin.basementPrefab.name = "basement.basementprefab";

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

            piece.FixReferences();

            Prefab.NetworkRegister(basementPrefab);

            // Add to tool
            var hammerPrefab = Prefab.Cache.GetPrefab<GameObject>("Hammer");
            var hammerPieceTable = hammerPrefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_buildPieces;

            hammerPieceTable.m_pieces.Add(basementPrefab.gameObject);
           
        }
    }
}
