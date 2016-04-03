using PaintDotNet;
using System.Drawing;

namespace pyrochild.effects.common
{
    public struct HistoryItem
    {
        public Rectangle DeltaRect;
        public DiskBackedSurface DeltaSurface;

        public HistoryItem(Surface surface, Rectangle bounds)
        {
            DeltaRect = Rectangle.Intersect(surface.Bounds, bounds);
            Surface temp;
            if (DeltaRect.Width * DeltaRect.Height > 0)
            {
                temp = new Surface(DeltaRect.Size);
                temp.CopySurface(surface, temp.Bounds.Location, DeltaRect);
            }
            else
            {
                temp = new Surface(1, 1);
                DeltaRect.Width = 1;
                DeltaRect.Height = 1;
            }
            DeltaSurface = new DiskBackedSurface(temp, true);
            DeltaSurface.ToDisk();
            temp.Dispose();
        }

        public HistoryItem(Surface surface)
        {
            DeltaRect = surface.Bounds;
            DeltaSurface = new DiskBackedSurface(surface, false);
            DeltaSurface.ToDisk();
        }
    }
}
