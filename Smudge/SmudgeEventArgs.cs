using pyrochild.effects.common;
using System.Drawing;
using System.Windows.Forms;

namespace pyrochild.effects.smudge
{
    public class SmudgeEventArgs : QueuedToolEventArgs
    {
        public SmudgeEventArgs(QueuedToolEventType eventtype, CanvasMouseEventArgs eventargs, PngBrush brush, int brushwidth, float strength, float jitter, float quality)
            : base(eventtype)
        {
            Button = eventargs.Button;
            X = (int)eventargs.X;
            Y = (int)eventargs.Y;
            Brush = brush;
            BrushWidth = brushwidth;
            Strength = strength;
            Jitter = jitter;
            Quality = quality;
        }
        public MouseButtons Button { get; set; }
        public Point Location { get { return new Point(X, Y); } }
        public int X { get; set; }
        public int Y { get; set; }
        public PngBrush Brush { get; set; }
        public int BrushWidth { get; set; }
        public float Strength { get; set; }
        public float Jitter { get; set; }
        public float Quality { get; set; }
    }
}