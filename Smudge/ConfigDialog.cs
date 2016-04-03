using PaintDotNet;
using PaintDotNet.Effects;
using pyrochild.effects.common;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace pyrochild.effects.smudge
{
    public partial class ConfigDialog : EffectConfigDialog
    {
        PngBrushCollection brushcollection;
        private HistoryStack historystack;
        private SmudgeRenderer renderer;
        private const char decPenSizeShortcut = '[';
        private const char decPenSizeBy5Shortcut = (char)27; // Ctrl [ but must also test that Ctrl is down
        private const char incPenSizeShortcut = ']';
        private const char incPenSizeBy5Shortcut = (char)29; // Ctrl ] but must also test that Ctrl is down
        private const char undoShortcut = (char)26;
        private const char redoShortcut = (char)25;
        private Surface surface;
        private SliderControl pressure, jitter;
        private const int minPenSize = 2;
        private const int maxPenSize = 1500;
        private int[] brushSizes =
            { 
                2, 3, 4, 5, 6, 7, 8, 9, 10, 
                11, 12, 13, 14, 15, 20, 25, 30, 
                35, 40, 45, 50, 60, 70, 80, 90, 100, 125, 150, 200, 300
            };

        public ConfigDialog()
        {
            InitializeComponent();

            this.brushcombobox.ComboBox.DrawMode = DrawMode.OwnerDrawVariable;
            this.brushcombobox.ComboBox.MeasureItem += new MeasureItemEventHandler(brushcombobox_MeasureItem);
            this.brushcombobox.ComboBox.ItemHeight = 16;
            this.brushcombobox.ComboBox.DrawItem += new DrawItemEventHandler(brushcombobox_DrawItem);
            this.brushcombobox.DropDownHeight = this.Height - 100;

            pressure = new SliderControl();
            jitter = new SliderControl();

            this.brushSize.ComboBox.SuspendLayout();

            for (int i = 0; i < this.brushSizes.Length; ++i)
            {
                this.brushSize.Items.Add(this.brushSizes[i].ToString());
            }

            this.brushSize.ComboBox.ResumeLayout(false);

            settingStrip.Items.Insert(
                settingStrip.Items.IndexOf(pressureLabel) + 1,
                new ToolStripControlHost(pressure) { AutoSize = false });

            settingStrip.Items.Insert(
                settingStrip.Items.IndexOf(densityLabel) + 1,
                new ToolStripControlHost(jitter) { AutoSize = false });

            this.zoom.ComboBox.SuspendLayout();

            string percent100 = null;
            for (int i = 0; i < CanvasPanel.ZoomFactors.Length; i++)
            {
                string zoomValueString = (CanvasPanel.ZoomFactors[i] * 100).ToString();
                string zoomItemString = string.Format("{0}%", zoomValueString);

                if (CanvasPanel.ZoomFactors[i] == 1.0)
                {
                    percent100 = zoomItemString;
                }

                this.zoom.Items.Add(zoomItemString);
            }
            this.zoom.ComboBox.ResumeLayout(false);
            this.zoom.Text = percent100;

            InitializeTooltips();
            InitializeUIImages();
        }

        private void InitializeUIImages()
        {
            undo.Image = new Bitmap(typeof(Smudge), "images.undo.png");
            redo.Image = new Bitmap(typeof(Smudge), "images.redo.png");
            zoomIn.Image = new Bitmap(typeof(Smudge), "images.zoomin.png");
            zoomOut.Image = new Bitmap(typeof(Smudge), "images.zoomout.png");
            brushSizeIncrement.Image = new Bitmap(typeof(Smudge), "images.plus.png");
            brushSizeDecrement.Image = new Bitmap(typeof(Smudge), "images.minus.png");
        }

        private void InitializeTooltips()
        {
            undo.ToolTipText= "Undo";
            redo.ToolTipText= "Redo";
            brushSizeIncrement.ToolTipText = "Increase brush size";
            brushSizeDecrement.ToolTipText = "Decrease brush size";
            zoomIn.ToolTipText = "Zoom in";
            zoomOut.ToolTipText = "Zoom out";
        }

        private void InitializeRenderer()
        {
            renderer = new SmudgeRenderer(surface);

            renderer.Invalidated += new InvalidateEventHandler(renderer_Invalidated);
            renderer.MouseDown += new QueuedToolEventHandler(renderer_MouseDown);
            renderer.MouseUp += new QueuedToolEventHandler(renderer_MouseUp);       
        }

        private void canvas_ZoomFactorChanged(object sender, EventArgs e)
        {
            zoom.SelectedItem = string.Format("{0}%", canvas.ZoomFactor * 100);
        }

        void renderer_MouseUp(object sender, QueuedToolEventArgs e)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    Action<object, QueuedToolEventArgs> action = renderer_MouseUp;
                    this.Invoke(action, new object[] { sender, e });
                }
                else
                {
                    historystack.AddHistoryItem(surface, renderer.PopTotalInvalidRect());
                    UpdateHistoryButtons(false);
                    ok.Enabled = true;
                }
            }
            catch (ObjectDisposedException) { }
        }

        void renderer_MouseDown(object sender, QueuedToolEventArgs e)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    Action<object, QueuedToolEventArgs> action = renderer_MouseDown;
                    this.Invoke(action, new object[] { sender, e });
                }
                else
                {
                    UpdateHistoryButtons(true);
                    ok.Enabled = false;
                }
            }
            catch (ObjectDisposedException) { }
        }

        private void renderer_Invalidated(object sender, InvalidateEventArgs e)
        {
            canvas.InvalidateCanvas(e.InvalidRect);
        }

        private void brushSize_Validating(object sender, EventArgs e)
        {
            float penSize;
            bool valid = float.TryParse(this.brushSize.Text, out penSize);

            if (!valid)
            {
                this.brushSize.BackColor = Color.Red;
            }
            else
            {
                if (penSize < minPenSize)
                {
                    this.brushSize.BackColor = Color.Red;
                }
                else if (penSize > maxPenSize)
                {
                    this.brushSize.BackColor = Color.Red;
                }
                else
                {
                    this.brushSize.BackColor = SystemColors.Window;
                    this.brushSize.ToolTipText = string.Empty;
                    OnPenChanged();
                }
            }
        }

        private void OnPenChanged()
        {
            canvas.BrushSize = BrushSize;
        }

        private void donate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Services.GetService<PaintDotNet.AppModel.IShellService>().LaunchUrl(this, "http://forums.getpaint.net/index.php?showtopic=7291");
        }

        private void ConfigDialog_Load(object sender, EventArgs e)
        {
            brushcollection = new PngBrushCollection(Services, Smudge.RawName);
            CreateDefaultBrushes();
            OnBrushesChanged();

            for (int i = 0; i < brushcollection.Count; i++)
            {
                brushcombobox.Items.Add(brushcollection[i]);
            }
            brushcombobox.Items.Add("Add/Remove Brushes...");

            this.BackColor = SystemColors.Control;
            this.Text = Smudge.StaticDialogName;
            surface = EffectSourceSurface.Clone();
            canvas.Surface = surface;
            canvas.Selection = Selection;
            historystack = new HistoryStack(surface, false);

            InitializeRenderer();

            this.DesktopLocation = Owner.PointToScreen(new Point(0, 30));
            this.Size = new Size(Owner.ClientSize.Width, Owner.ClientSize.Height - 30);
            this.WindowState = Owner.WindowState;
        }

        private void CreateDefaultBrushes()
        {
            string softbrushpath = Path.Combine(PngBrushCollection.BrushesPath, "Soft Brush.png");
            string paintbrushpath = Path.Combine(PngBrushCollection.BrushesPath, "Paintbrush.png");
            string hardbrushpath = Path.Combine(PngBrushCollection.BrushesPath, "Hard Brush.png");

            if (!File.Exists(softbrushpath))
            {
                try
                {
                    using (FileStream fs = new FileStream(softbrushpath, FileMode.CreateNew))
                    using (Surface brush = RoundBrush.CreateSurface(500, 1, 0, 0))
                    {
                        brush.CreateAliasedBitmap().Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
                catch { }
            }
            if (!File.Exists(hardbrushpath))
            {
                try
                {
                    using (FileStream fs = new FileStream(hardbrushpath, FileMode.CreateNew))
                    using (Surface brush = RoundBrush.CreateSurface(250, 1, 1, 0))
                    {
                        brush.CreateAliasedBitmap().Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
                catch { }
            }
            if (!File.Exists(paintbrushpath))
            {
                try
                {
                    using (FileStream fs = new FileStream(paintbrushpath, FileMode.CreateNew))
                    using (Surface brush = Surface.CopyFromBitmap(new Bitmap(typeof(Smudge), "images.Paintbrush.png")))
                    {
                        brush.CreateAliasedBitmap().Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
                catch { }
            }
        }

        private void OnBrushesChanged()
        {
            if (this.InvokeRequired)
            {
                Action obcd = OnBrushesChanged;
                this.Invoke(obcd);
            }
            else
            {
                brushcollection.Dispose();
                brushcollection = new PngBrushCollection(Services, Smudge.RawName);

                if (brushcollection.Count == 0)
                    CreateDefaultBrushes();

                object lastsel = brushcombobox.SelectedItem;

                brushcombobox.Items.Clear();
                for (int i = 0; i < brushcollection.Count; i++)
                {
                    brushcombobox.Items.Add(brushcollection[i]);
                }
                brushcombobox.Items.Add("Add/Remove Brushes...");

                if (lastsel != null && brushcombobox.Items.Contains(lastsel))
                {
                    brushcombobox.SelectedItem = lastsel;
                }
                else
                {
                    brushcombobox.SelectedItem = new PngBrush("Soft Brush");
                }
            }
        }

        private void brushSizeDecrement_Click(object sender, EventArgs e)
        {
            int amount = -1;

            if ((Control.ModifierKeys & Keys.Control) != 0)
            {
                amount *= 5;
            }

            AddToPenSize(amount);
        }

        private void brushSizeIncrement_Click(object sender, EventArgs e)
        {
            int amount = 1;

            if ((Control.ModifierKeys & Keys.Control) != 0)
            {
                amount *= 5;
            }

            AddToPenSize(amount);
        }

        public void AddToPenSize(int delta)
        {
            int newWidth = Int32Util.Clamp(BrushSize + delta, minPenSize, maxPenSize);
            BrushSize = newWidth;
        }

        protected override void InitialInitToken()
        {
            theEffectToken = new ConfigToken();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            canvas.PerformMouseWheel(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (!e.Handled) //hasn't been handled? our turn, then.
            {
                if (e.KeyChar == decPenSizeShortcut)
                {
                    AddToPenSize(-1);
                    e.Handled = true;
                }
                else if (e.KeyChar == decPenSizeBy5Shortcut && (ModifierKeys & Keys.Control) != 0)
                {
                    AddToPenSize(-5);
                    e.Handled = true;
                }
                else if (e.KeyChar == incPenSizeShortcut)
                {
                    AddToPenSize(+1);
                    e.Handled = true;
                }
                else if (e.KeyChar == incPenSizeBy5Shortcut && (ModifierKeys & Keys.Control) != 0)
                {
                    AddToPenSize(+5);
                    e.Handled = true;
                }
                else if (e.KeyChar == undoShortcut && (ModifierKeys & Keys.Control) != 0)
                {
                    DoUndo();
                }
                else if (e.KeyChar == redoShortcut && (ModifierKeys & Keys.Control) != 0)
                {
                    DoRedo();
                }
            }
            base.OnKeyPress(e);
        }

        private void DoUndo()
        {
            if (historystack.CanStepBack)
            {
                historystack.StepBack(surface);
                UpdateHistoryButtons(false);
                canvas.Invalidate();
            }
        }

        private void DoRedo()
        {
            if (historystack.CanStepForward)
            {
                historystack.StepForward(surface);
                UpdateHistoryButtons(false);
                canvas.Invalidate();
            }
        }

        private void UpdateHistoryButtons(bool disable)
        {
            if (!this.IsDisposed)
            {
                if (this.InvokeRequired)
                {
                    Action<bool> uhbd = UpdateHistoryButtons;
                    try
                    {
                        this.Invoke(uhbd, new object[] { disable });
                    }
                    catch { }
                }
                else
                {
                    redo.Enabled = historystack.CanStepForward && !disable;
                    undo.Enabled = historystack.CanStepBack && !disable;
                }
            }
        }

        public int BrushSize
        {
            get
            {
                int width;

                try
                {
                    width = (int)float.Parse(this.brushSize.Text);
                }

                catch (FormatException)
                {
                    width = 30;
                }

                return width;
            }
            set
            {
                this.brushSize.Text = value.ToString();
                OnPenChanged();
            }
        }

        public float Pressure
        {
            get
            {
                return this.pressure.Value;
            }
            set
            {
                this.pressure.Value = value;
            }
        }

        public float Jitter
        {
            get
            {
                return this.jitter.Value;
            }
            set
            {
                this.jitter.Value = value;
            }
        }

        public float Quality
        {
            get
            {
                return this.quality.Value;
            }
            set
            {
                this.quality.Value = value;
            }
        }

        private void canvas_CanvasMouseDown(object sender, CanvasMouseEventArgs e)
        {
            renderer.AddEvent(
                new SmudgeEventArgs(
                    QueuedToolEventType.MouseDown,
                    e,
                    brushcombobox.SelectedItem as PngBrush,
                    BrushSize,
                    Pressure,
                    Jitter,
                    Quality
                    ));
        }

        private void canvas_CanvasMouseMove(object sender, CanvasMouseEventArgs e)
        {
            renderer.AddEvent(
                new SmudgeEventArgs(
                    QueuedToolEventType.MouseMove,
                    e,
                    brushcombobox.SelectedItem as PngBrush,
                    BrushSize,
                    Pressure,
                    Jitter,
                    Quality
                    ));
        }

        private void canvas_CanvasMouseUp(object sender, CanvasMouseEventArgs e)
        {
            renderer.AddEvent(
                new SmudgeEventArgs(
                    QueuedToolEventType.MouseUp,
                    e,
                    brushcombobox.SelectedItem as PngBrush,
                    BrushSize,
                    Pressure,
                    Jitter,
                    Quality
                    ));
        }


        private void zoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (zoom.SelectedIndex >= 0)
                canvas.ZoomFactor = CanvasPanel.ZoomFactors[zoom.SelectedIndex];
        }


        protected override void InitDialogFromToken(EffectConfigToken effectTokenCopy)
        {
            ConfigToken token = effectTokenCopy as ConfigToken;

            brushcombobox.SelectedItem = token.brush;
            this.Pressure = token.strength;
            this.Jitter = token.jitter;
            this.BrushSize = token.width;
            this.Quality = token.quality;

            base.InitDialogFromToken(effectTokenCopy);
        }

        protected override void InitTokenFromDialog()
        {
            if (!this.IsDisposed)
            {
                ConfigToken token = EffectToken as ConfigToken;

                token.width = this.BrushSize;
                token.strength = this.Pressure;
                token.jitter = this.Jitter;
                token.quality = this.Quality;
                token.surface = surface;

                if (this.brushcombobox.SelectedItem != null)
                    token.brush = (PngBrush)this.brushcombobox.SelectedItem;

                base.InitTokenFromDialog();
            }
        }

        private void ok_Click(object sender, EventArgs e)
        {
            FinishTokenUpdate();
        }

        private void undo_Click(object sender, EventArgs e)
        {
            DoUndo();
        }

        private void redo_Click(object sender, EventArgs e)
        {
            DoRedo();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            renderer.Abort();
        }

        private void zoomOut_Click(object sender, EventArgs e)
        {
            canvas.ZoomOut();
        }

        private void zoomIn_Click(object sender, EventArgs e)
        {
            canvas.ZoomIn();
        }

        int lastselected = 0;
        void brushcombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (brushcombobox.SelectedIndex == brushcombobox.Items.Count - 1)
            {
                DoAddBrushes();
                brushcombobox.SelectedIndex = lastselected;
            }
            else
            {
                lastselected = brushcombobox.SelectedIndex;
            }
        }

        void brushcombobox_DropDownClosed(object sender, EventArgs e)
        {
            brushcombobox.BeginUpdate();
            droppingdown = false;
            brushcombobox.ComboBox.ItemHeight--;
            brushcombobox.EndUpdate();
        }

        bool droppingdown = false;
        void brushcombobox_DropDown(object sender, EventArgs e)
        {
            brushcombobox.BeginUpdate();
            OnBrushesChanged();
            droppingdown = true;
            brushcombobox.ComboBox.ItemHeight++;
            brushcombobox.EndUpdate();
        }

        void brushcombobox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index == brushcombobox.Items.Count - 1)
            {
                e.ItemHeight = 32;
            }
            else
            {
                if (droppingdown)
                {
                    e.ItemHeight = 32;

                    int width = (int)e.Graphics.MeasureString(brushcollection[e.Index].Name, brushcombobox.Font).Width + 32;
                    width = Math.Max(width, (int)e.Graphics.MeasureString(brushcollection[e.Index].NativeSizePrettyString, brushcombobox.Font).Width + 72);
                    width = width.Clamp(0, this.Width - 100);
                    if (width > brushcombobox.ComboBox.DropDownWidth)
                    {
                        brushcombobox.DropDownWidth = width;
                    }
                }
                else
                {
                    e.ItemHeight = 16;
                }
            }
        }

        void brushcombobox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.State == DrawItemState.Selected)
            {
                e.DrawFocusRectangle();
            }

            if (e.Index == brushcombobox.Items.Count - 1)
            {
                e.Graphics.DrawString(brushcombobox.Items[e.Index].ToString(), e.Font, new SolidBrush(ColorBgra.Blend(new ColorBgra[] { ColorBgra.FromColor(e.ForeColor), ColorBgra.FromColor(e.BackColor) }).ToColor()), e.Bounds.X, e.Bounds.Y + 8);
            }
            else
            {
                if (e.Index >= 0)
                {
                    if (droppingdown)
                    {
                        var image = brushcollection[e.Index].ThumbnailAlphaOnly.CreateAliasedBitmap();
                        e.Graphics.DrawImage(image, e.Bounds.X + (32 - image.Width) / 2, e.Bounds.Y + (32 - image.Height) / 2);
                        e.Graphics.DrawString(brushcollection[e.Index].Name, e.Font, new SolidBrush(e.ForeColor), e.Bounds.X + 32, e.Bounds.Y);

                        e.Graphics.DrawString(brushcollection[e.Index].NativeSizePrettyString, e.Font, new SolidBrush(ColorBgra.Blend(new ColorBgra[] { ColorBgra.FromColor(e.ForeColor), ColorBgra.FromColor(e.BackColor) }).ToColor()), e.Bounds.X + 64, e.Bounds.Y + 16);
                    }
                    else
                    {
                        var image = brushcollection[e.Index].ThumbnailAlphaOnly.CreateAliasedBitmap();
                        e.Graphics.DrawImage(image, e.Bounds.X + (32 - image.Width) / 4, e.Bounds.Y + (32 - image.Height) / 4, image.Width / 2, image.Height / 2);
                        e.Graphics.DrawString(brushcollection[e.Index].Name, e.Font, new SolidBrush(e.ForeColor), e.Bounds.X + 16, e.Bounds.Y);
                    }
                }
            }
        }

        private void DoAddBrushes()
        {
            try
            {
                using (new WaitCursorChanger(this))
                {
                    Services.GetService<PaintDotNet.AppModel.IShellService>().LaunchFolder(this, PngBrushCollection.BrushesPath);
                }
            }
            catch
            { }
        }

        private void abort_Click(object sender, EventArgs e)
        {
            renderer.Abort();
        }
    }
}
