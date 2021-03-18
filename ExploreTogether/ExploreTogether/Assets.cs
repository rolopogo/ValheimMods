using System.Linq;
using System.Reflection;
using UnityEngine;
using RoloPogo.Utilities;

namespace ExploreTogether
{
    class Assets
    {
        public static Sprite cartSprite;
        public static Sprite boatSprite;

        public static void Init()
        {
            cartSprite = LoadSpriteFromTexture(LoadTextureRaw(ResourceUtils.GetResource(Assembly.GetCallingAssembly(), "ExploreTogether.Resources.CartIcon.png")));
            boatSprite = LoadSpriteFromTexture(LoadTextureRaw(ResourceUtils.GetResource(Assembly.GetCallingAssembly(), "ExploreTogether.Resources.BoatIcon.png")));
        }

        internal static Texture2D LoadTextureRaw(byte[] file)
        {
            if (file.Count() > 0)
            {
                Texture2D Tex2D = new Texture2D(2, 2);
                if (Tex2D.LoadImage(file))
                    return Tex2D;
            }
            return null;
        }

        public static Sprite LoadSpriteFromTexture(Texture2D SpriteTexture, float PixelsPerUnit = 100.0f)
        {
            if (SpriteTexture)
                return Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit);
            return null;
        }
    }
}
