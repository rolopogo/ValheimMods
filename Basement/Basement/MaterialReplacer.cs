using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basement
{
    static class MaterialReplacer
    {
        static Dictionary<string, Material> originalMaterials;

        public static void GetAllMaterials()
        {
            var allmats = Resources.FindObjectsOfTypeAll<Material>();
            originalMaterials = new Dictionary<string, Material>();
            foreach (var item in allmats)
            {
                originalMaterials[item.name] = item;
            }
        }

        public static void ReplaceAllMaterialsWithOriginal(GameObject go)
        {
            if (originalMaterials == null) GetAllMaterials();

            foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    if (renderer.materials[i].name.StartsWith("_REPLACE_"))
                    {
                        var matName = renderer.material.name.Replace(" (Instance)", string.Empty).Replace("_REPLACE_", "");

                        if (originalMaterials.ContainsKey(matName))
                        {
                            renderer.material = originalMaterials[matName];
                        }
                        else
                        {
                            Plugin.Log.LogInfo("No suitable material found to replace: " + matName);
                            // Skip over this material in future
                            originalMaterials[matName] = renderer.material;
                        }
                    }
                }
            }
        }
    }
}
