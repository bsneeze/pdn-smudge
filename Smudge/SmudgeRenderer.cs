using PaintDotNet;
using pyrochild.effects.common;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace pyrochild.effects.smudge
{
    public class SmudgeRenderer : QueuedToolRenderer
    {
        const int B = 0;
        const int G = 1;
        const int R = 2;
        const int A = 3;

        int srfcWidth;
        int srfcHeight;

        int brushwidth;
        int brushheight;
        Size brushsize;
        Size brushcenteroffset;

        float[] brush;
        float[] strengthmask;

        bool mousedown;
        Point lastmouse;

        float spacing;

        Random r;

        public SmudgeRenderer(Surface surface)
            : base(surface)
        {
            srfcWidth = surface.Width;
            srfcHeight = surface.Height;

            r = new Random();
        }

        float remainingSpace; //keeps track of spacing
        protected unsafe override void OnMouseMove(QueuedToolEventArgs le)
        {
            SmudgeEventArgs e = le as SmudgeEventArgs;
            int jittermax = (int)(e.Jitter * e.BrushWidth);

            Rectangle invrect = new Rectangle(lastmouse.X - brushwidth / 2, lastmouse.Y - brushheight / 2, brushwidth, brushheight);
            invrect = Rectangle.Union(invrect, new Rectangle(e.X - brushwidth / 2, e.Y - brushheight / 2, brushwidth, brushheight));
            invrect.Inflate(jittermax, jittermax);

            if (e.Button == MouseButtons.Left && mousedown)
            {
                float dist = Distance(lastmouse, e.Location);

                ColorBgra* ptr0 = (ColorBgra*)Surface.Scan0.VoidStar;

                float f;
                for (f = remainingSpace; f < dist && !IsAborted; f += spacing)
                {
                    PointF currentpoint = Lerp(lastmouse, e.Location, f / dist);

                    if (jittermax > 0)
                    {
                        currentpoint.X += r.Next(-jittermax, jittermax);
                        currentpoint.Y += r.Next(-jittermax, jittermax);
                    }
                    Point dstpt = Point.Subtract(new Point((int)currentpoint.X, (int)currentpoint.Y), brushcenteroffset);
                    Rectangle dstRect = new Rectangle(dstpt, brushsize);
                    Rectangle clippedRect = Rectangle.Intersect(dstRect, Surface.Bounds);
                    
                        fixed (float* brshpin = brush,
                                      _strmask = strengthmask)
                    {
                        float* brshptr = brshpin,
                               strmask = _strmask;

                        int maxy = dstpt.Y + dstRect.Height;
                        int maxx = dstpt.X + dstRect.Width;
                        for (int y = dstpt.Y; y < maxy; ++y)
                        {
                            int dsty = Clamp(y, 0, srfcHeight - 1);
                            ColorBgra* ptrrow = ptr0 + srfcWidth * dsty;
                            for (int x = dstpt.X; x < maxx; ++x)
                            {
                                int dstx = Clamp(x, 0, srfcWidth - 1);
                                ColorBgra* srfcptr = ptrrow + dstx;

                                float invstr = 1.0f - *strmask;

                                //eliminate fringing from 0-alpha
                                //this officially makes us awesomer than Photoshop
                                if (brshptr[A] == 0.0f)
                                {
                                    brshptr[B] = srfcptr->B;
                                    brshptr[G] = srfcptr->G;
                                    brshptr[R] = srfcptr->R;
                                }
                                else if (srfcptr->A == 0)
                                {
                                    srfcptr->B = (byte)(brshptr[B]);
                                    srfcptr->G = (byte)(brshptr[G]);
                                    srfcptr->R = (byte)(brshptr[R]);
                                }

                                //blend the surface into the brush buffer, then copy it back onto the surface
                                brshptr[B] = (brshptr[B] * *strmask + srfcptr->B * invstr);
                                srfcptr->B = (byte)(brshptr[B] + 0.5f);

                                brshptr[G] = (brshptr[G] * *strmask + srfcptr->G * invstr);
                                srfcptr->G = (byte)(brshptr[G] + 0.5f);

                                brshptr[R] = (brshptr[R] * *strmask + srfcptr->R * invstr);
                                srfcptr->R = (byte)(brshptr[R] + 0.5f);

                                brshptr[A] = (brshptr[A] * *strmask + srfcptr->A * invstr);
                                srfcptr->A = (byte)(brshptr[A] + 0.5f);

                                brshptr += 4;
                                ++strmask;
                            }
                        }
                    }
                }
                remainingSpace = f - dist;
            }
            OnInvalidated(invrect);
            lastmouse = e.Location;
        }

        private static float GetSpacing(Size brushsize, float quality)
        {
            float size = Math.Max(brushsize.Width, brushsize.Height);
            return size + quality - size * quality;
        }

        public static PointF Lerp(PointF a, PointF b, float t)
        {
            float x = a.X + t * (b.X - a.X);
            float y = a.Y + t * (b.Y - a.Y);
            return new PointF(x, y);
        }

        public static float Distance(PointF a, PointF b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        protected unsafe override void OnMouseDown(QueuedToolEventArgs le)
        {
            SmudgeEventArgs e = le as SmudgeEventArgs;
            lastmouse = e.Location;
            if (e.Button == MouseButtons.Left)
            {
                mousedown = true;

                using (Surface brushsource = e.Brush.GetSurface(e.BrushWidth))
                {
                    brushheight = brushsource.Height;
                    brushwidth = brushsource.Width;
                    brushsize = new Size(brushwidth, brushheight);
                    brushcenteroffset = new Size(brushwidth / 2, brushheight / 2);
                    spacing = GetSpacing(brushsize, e.Quality);
                    int brusharea = brushwidth * brushheight;

                    brush = new float[brusharea * 4];
                    Point srcpt = Point.Subtract(e.Location, new Size(brushwidth / 2, brushheight / 2));
                    strengthmask = new float[brusharea];

                    float strength = (float)(Math.Pow(e.Strength, .25) / 255.0);
                    
                    fixed (float* _strmask = strengthmask)
                    {
                        float* strmask = _strmask;

                        ColorBgra* voidstar = (ColorBgra*)brushsource.Scan0.VoidStar;
                        for (int i = 0; i < brushsource.Scan0.Length; i += 4)
                        {
                            *strmask = voidstar->A * strength;
                            ++voidstar;
                            ++strmask;
                        }
                    }

                    //fill the brush buffer by copying the surface where we clicked into the buffer
                    fixed (float* brshptrpin = brush)
                    {
                        float* brshptr = brshptrpin;

                        ColorBgra* ptr0 = (ColorBgra*)Surface.Scan0.VoidStar;
                        for (int y = 0; y < brushheight; ++y)
                        {
                            int srcy = Clamp(srcpt.Y + y, 0, srfcHeight - 1);
                            ColorBgra* ptrrow = ptr0 + srfcWidth * srcy;
                            for (int x = 0; x < brushwidth; ++x)
                            {
                                int srcx = Clamp(srcpt.X + x, 0, srfcWidth - 1);
                                ColorBgra* ptr = ptrrow + srcx;

                                brshptr[B] = ptr->B;
                                brshptr[G] = ptr->G;
                                brshptr[R] = ptr->R;
                                brshptr[A] = ptr->A;

                                brshptr += 4;
                            }
                        }
                    }
                }
            }
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }
    }
}