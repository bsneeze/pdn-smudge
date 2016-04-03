using PaintDotNet;
using PaintDotNet.Effects;
using System.Drawing;

namespace pyrochild.effects.smudge
{
    [PluginSupportInfo(typeof(PluginSupportInfo))]
    public sealed class Smudge : Effect
    {
        public Smudge() : base(StaticName, StaticIcon, StaticSubMenu, EffectFlags.Configurable) { }

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

        public override EffectConfigDialog CreateConfigDialog()
        {
            return new ConfigDialog();
        }

        protected unsafe override void OnSetRenderInfo(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            if (((ConfigToken)parameters).surface != null)
            {
                dstArgs.Surface.CopySurface(((ConfigToken)parameters).surface, EnvironmentParameters.GetSelection(srcArgs.Bounds));
            }

            base.OnSetRenderInfo(parameters, dstArgs, srcArgs);
        }

        public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
        { }
    }
}