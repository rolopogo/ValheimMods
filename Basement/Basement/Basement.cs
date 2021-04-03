using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Basement
{
    class Basement : MonoBehaviour
    {
        Bounds interiorBounds;
        Collider[] localColliders;

        public static List<Basement> allBasements;

        GameObject b;

        void Awake()
        {
            if (allBasements == null) allBasements = new List<Basement>();
            allBasements.Add(this);

            localColliders = gameObject.GetComponentsInChildren<Collider>();

            b = transform.Find("Interior/Bounds").gameObject;
            b.layer = 4; // Allows building without disabling zone detection, idk what this layer is actually for
            interiorBounds = b.GetComponent<BoxCollider>().bounds;
        }

        // Fun boat test
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {

                var nview = GetComponent<ZNetView>();
                var zdo = nview.GetZDO();
                zdo.SetPrefab(nview.GetPrefabName().GetStableHashCode());
                Debug.Log(zdo.GetPrefab());
                Debug.Log(zdo.m_uid);
                Debug.Log(nview.GetPrefabName().GetStableHashCode());
                Debug.Log(ZNetScene.instance.GetPrefab(zdo.GetPrefab()).name);
        //        var ships = Resources.FindObjectsOfTypeAll<Ship>().First();
        //        var exterior = transform.Find("exterior");
        //        exterior.SetParent(ships.m_sailObject.transform.parent.parent);
        //        exterior.localPosition = Vector3.right;
            }
        }

        void OnDestroy()
        {
            allBasements.Remove(this);
        }

        public bool CanBeRemoved()
        {
            var ol = Physics.OverlapBox(interiorBounds.center, interiorBounds.extents).Where(x => !localColliders.Contains(x));
            foreach (var item in ol)
            {
                Plugin.Log.LogInfo(item.name + " is preventing basement from being destroyed");
            }
            return !ol.Any();            
        } 
    }
}