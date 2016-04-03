using PaintDotNet;
using System;

namespace pyrochild.effects.common
{
    public static class RoundBrush
    {
        public unsafe static Surface CreateSurface(int diameter, float centerstrength, float edgestrength, float holeratio)
        {
            float[] brush = CreateBrush(diameter, centerstrength, edgestrength, holeratio);
            Surface retval = new Surface(diameter, diameter);

            fixed (float* brushptrpin = brush)
            {
                float* brushptr = brushptrpin;
                ColorBgra* surfaceptr = (ColorBgra*)retval.Scan0.VoidStar;
                for (int i = 0; i < brush.Length; ++i)
                {
                    surfaceptr->A = (byte)(255.0f * brushptr[0] + 0.5f);
                    ++brushptr;
                    ++surfaceptr;
                }
            }
            return retval;
        }
        public unsafe static float[] CreateBrush(int diameter, float centerstrength, float edgestrength, float holeratio)
        {
            float[] retval = new float[diameter * diameter];

            float radius = diameter / 2f - 1;
            float holeradius = radius * holeratio - 1;
            float recipraddiff = 1 / (radius - holeradius);

            fixed (float* _ptr = retval)
            {
                float* ptr = _ptr;
                for (int y = 0; y < diameter; ++y)
                {
                    float dy = y - radius;
                    float dy2 = dy * dy;
                    for (int x = 0; x < diameter; ++x)
                    {
                        float dx = x - radius;
                        float dist = (float)Math.Sqrt(dx * dx + dy2);
                        float factor = (dist - holeradius) * recipraddiff;


                        if (dist < radius && dist >= holeradius)
                        {
                            *ptr = centerstrength + (edgestrength - centerstrength) * factor;
                        }
                        else if (dist <= radius + 1 && dist >= radius)
                        {
                            float aafactor = radius + 1 - dist;
                            *ptr = edgestrength * aafactor;
                        }
                        else if (dist < holeradius && dist >= holeradius - 1)
                        {
                            float aafactor = 1 - holeradius + dist;
                            *ptr = centerstrength * aafactor;
                        }
                        ++ptr;
                    }
                }
            }
            return retval;
        }
    }
}
