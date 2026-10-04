using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.Imaging;
using PaintDotNet.Rendering;
using System.Drawing;

namespace pyrochild.effects.smudge
{
    [PluginSupportInfo(typeof(PluginSupportInfo))]
    public sealed class Smudge : BitmapEffect<ConfigToken>
    {
        Surface resultSurface;

        public Smudge() : base(StaticName, StaticIcon, StaticSubMenu, BitmapEffectOptions.Create() with { IsConfigurable = true }) { }

        internal static string RawName { get { return "Smudge"; } }
        public static string StaticName
        {
            get
            {
                string name = RawName;
#if DEBUG
                name += " BETA";
#endif
                return name;
            }
        }
        public static string StaticDialogName
        {
            get { return StaticName + " by pyrochild"; }
        }
        public static Bitmap StaticIcon = new Bitmap(typeof(Smudge), "images.icon.png");
        public static string StaticSubMenu
        {
            get
            {
                return "Tools";
            }
        }

        protected override IEffectConfigForm OnCreateConfigForm()
        {
            return new ConfigDialog();
        }

        protected override void OnSetToken(ConfigToken newToken)
        {
            base.OnSetToken(newToken);

            if (newToken != null && newToken.surface != null)
            {
                resultSurface = newToken.surface;
            }
        }

        protected override unsafe void OnRender(IBitmapEffectOutput output)
        {
            if (resultSurface == null)
            {
                return;
            }

            RectInt32 bounds = output.Bounds;

            // "Repeat <effect>" on a differently-sized image would otherwise read past the surface.
            if (bounds.X < 0 || bounds.Y < 0 || bounds.X + bounds.Width > resultSurface.Width || bounds.Y + bounds.Height > resultSurface.Height)
            {
                return;
            }

            using (IBitmapLock<ColorBgra32> dstLock = output.LockBgra32())
            {
                RegionPtr<ColorBgra32> dstRegion = new RegionPtr<ColorBgra32>(dstLock.Buffer, dstLock.Size, dstLock.BufferStride);
                RegionPtr<ColorBgra> srcRegion = new RegionPtr<ColorBgra>(resultSurface.GetPointPointer(bounds.X, bounds.Y), bounds.Width, bounds.Height, resultSurface.Stride);
                srcRegion.Cast<ColorBgra32>().CopyTo(dstRegion);
            }
        }
    }
}
