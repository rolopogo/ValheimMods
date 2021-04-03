using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace CustomSails
{
    public class ShipSailUrlSetter : MonoBehaviour, Hoverable, Interactable, TextReceiver
    {
        private string m_url;
        public string m_name = "SailUrl";
        private const int m_characterLimit = 100;

        private SkinnedMeshRenderer sailRenderer;

        private ZNetView m_nview;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();

            sailRenderer = GetComponentsInChildren<SkinnedMeshRenderer>().First(x => x.name == "sail_full");

            if (m_nview.GetZDO() == null)
            {
                return;
            }
            UpdateText();
            InvokeRepeating("UpdateText", 2f, 2f);
        }
        
        public string GetHoverText()
        {
            if (!Plugin.instance.AllowInput()) return string.Empty;

            //if (!PrivateArea.CheckAccess(transform.position, 0f, false))
            //{
            //    return "Restricted" + "\n" + GetText();
            //}
            return Localization.instance.Localize("\n[<color=yellow><b>$KEY_Use</b></color>] Set URL") + "\n" + GetText() ;
        }
        
        public string GetHoverName()
        {
            if (!Plugin.instance.AllowInput()) return string.Empty;
            return "Sail Url";
        }
        
        public bool Interact(Humanoid character, bool hold)
        {
            if (!Plugin.instance.AllowInput()) return false;
            if (hold)
            {
                return false;
            }
            if (!PrivateArea.CheckAccess(transform.position, 0f, true))
            {
                return false;
            }
            TextInput.instance.RequestText(this, "$piece_sign_input", m_characterLimit);
            return true;
        }
        
        private void UpdateText()
        {
            string text = GetText();
            if (m_url == text)
            {
                return;
            }

            SetText(text);
        }
        
        public string GetText()
        {
            return m_nview.GetZDO().GetString("SailUrl", string.Empty);
        }
        
        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        public void SetText(string text)
        {
            if (!PrivateArea.CheckAccess(transform.position, 0f, true))
            {
                return;
            }

            StartCoroutine(DownloadTexture(text, ApplyTexture));
        }

        private void ApplyTexture(string url, Texture2D obj)
        {
            m_nview.ClaimOwnership();
            m_nview.GetZDO().Set("SailUrl", url);

            sailRenderer.material.SetTexture("_MainTex", obj);
        }

        public IEnumerator DownloadTexture(string url, Action<string, Texture2D> callback)
        {
            m_url = url;
            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                yield return uwr.SendWebRequest();
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(uwr.error + "\n" + url);
                }
                else
                {
                    var tex = new Texture2D(2, 2);
                    tex.LoadImage(uwr.downloadHandler.data);
                    callback.Invoke(url, tex);
                }
            }
        }
    }
}