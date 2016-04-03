using PaintDotNet;
using System;
using System.Drawing;
using System.IO;

namespace pyrochild.effects.common
{
    public class PngBrush
    {
        public static bool operator ==(PngBrush left, PngBrush right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PngBrush left, PngBrush right)
        {
            return !(left == right);
        }

        public PngBrush(string brushName)
        {
            name = brushName;
            thumbnail = null;
            thumbnailalphaonly = null;
            nativesize = Size.Empty;
            thumbnail = GetSurface(32);
            thumbnailalphaonly = GetSurfaceAlphaOnly(32);
        }

        public override bool Equals(object obj)
        {
            return ((PngBrush)obj).name == this.name;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        private Surface thumbnail;
        public Surface Thumbnail { get { return thumbnail; } }

        private Surface thumbnailalphaonly;
        public Surface ThumbnailAlphaOnly { get { return thumbnailalphaonly; } }

        private Size nativesize;
        public Size NativeSize { get { return nativesize; } }

        public string NativeSizePrettyString { get { return "(" + nativesize.Width.ToString() + "x" + nativesize.Height.ToString() + ")"; } }

        private string name;
        public string Name { get { return name; } }

        public Surface GetSurface(int maxsidelength)
        {
            var source = GetSurface();
            Size size;

            if (source.Width > source.Height)
            {
                size = new Size(
                    maxsidelength,
                    Math.Max((maxsidelength * source.Height) / source.Width, 1));
            }
            else
            {
                size = new Size(
                    Math.Max((maxsidelength * source.Width) / source.Height, 1),
                    maxsidelength);
            }

            var ret = new Surface(size);
            if (ret.Width <= source.Width
                && ret.Height <= source.Height)
            {
                ret.FitSurface(ResamplingAlgorithm.SuperSampling, source);
            }
            else
            {
                ret.FitSurface(ResamplingAlgorithm.Bicubic, source);
            }
            return ret;
        }

        public Surface GetSurface()
        {
            Surface brushsource = null;
            string brushesdir = PngBrushCollection.BrushesPath;
            if (Directory.Exists(brushesdir))
            {
                string filename = Path.Combine(brushesdir, name + ".png");
                if (File.Exists(filename))
                {
                    try
                    {
                        using (Bitmap b = new Bitmap(filename))
                            brushsource = Surface.CopyFromBitmap(b);
                    }
                    catch { }
                }
            }
            if (brushsource == null)// can't find? default to a soft brush
            {
                brushsource = RoundBrush.CreateSurface(500, 1, 0, 0);
            }
            nativesize = brushsource.Size;
            return brushsource;
        }

        public Surface GetSurface(Size size)
        {
            var ret = new Surface(size);
            var source = GetSurface();

            if (source.Width <= ret.Width
                && source.Height <= ret.Height)
            {
                ret.FitSurface(ResamplingAlgorithm.SuperSampling, source);
            }
            else
            {
                ret.FitSurface(ResamplingAlgorithm.Bicubic, source);
            }
            return ret;
        }

        public unsafe Surface GetSurfaceAlphaOnly(Size size)
        {
            Surface retval = GetSurface(size);
            AlphaOnly(retval);
            return retval;
        }

        unsafe private static void AlphaOnly(Surface retval)
        {
            ColorBgra* ptr = retval.GetRowAddressUnchecked(0);
            for (int y = 0; y < retval.Height; y++)
            {
                for (int x = 0; x < retval.Width; x++)
                {
                    ptr->Bgra = (uint)ptr->A << 24;
                    ptr++;
                }
            }
        }

        public unsafe Surface GetSurfaceAlphaOnly(int maxsidelength)
        {
            Surface retval = GetSurface(maxsidelength);
            AlphaOnly(retval);
            return retval;
        }

        public unsafe Surface GetSurfaceAlphaOnly()
        {
            Surface retval = GetSurface();
            AlphaOnly(retval);
            return retval;
        }
    }
}