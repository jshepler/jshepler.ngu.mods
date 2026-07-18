using System.IO;
using UnityEngine;

namespace jshepler.ngu.mods
{
    internal static class Assets
    {
        internal static Texture2D Arrow_up;
        internal static Texture2D Arrow_top;
        internal static Texture2D Arrow_down;
        internal static Texture2D Arrow_bottom;

        static Assets()
        {
            var au = Resources.arrow_up;
            using (var ms = new MemoryStream())
            {
                au.Save(ms, au.RawFormat);
                Arrow_up = new Texture2D(au.Width, au.Height);
                Arrow_up.LoadImage(ms.ToArray());
            }

            var at = Resources.arrow_top;
            using (var ms = new MemoryStream())
            {
                at.Save(ms, at.RawFormat);
                Arrow_top = new Texture2D(at.Width, at.Height);
                Arrow_top.LoadImage(ms.ToArray());
            }

            var ad = Resources.arrow_down;
            using (var ms = new MemoryStream())
            {
                ad.Save(ms, ad.RawFormat);
                Arrow_down = new Texture2D(ad.Width, ad.Height);
                Arrow_down.LoadImage(ms.ToArray());
            }

            var ab = Resources.arrow_bottom;
            using (var ms = new MemoryStream())
            {
                ab.Save(ms, ab.RawFormat);
                Arrow_bottom = new Texture2D(ab.Width, ab.Height);
                Arrow_bottom.LoadImage(ms.ToArray());
            }
        }
    }
}
