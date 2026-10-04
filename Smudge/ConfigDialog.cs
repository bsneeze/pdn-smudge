using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.Imaging;
using PaintDotNet.Rendering;
using pyrochild.effects.common;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace pyrochild.effects.smudge
{
    public partial class ConfigDialog : EffectConfigForm<Smudge, ConfigToken>
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

        private float DpiScale
        {
            get { return this.DeviceDpi / 96f; }
        }

        public ConfigDialog()
        {
            InitializeComponent();
            this.Load += (themeSender, themeArgs) =>
            {
                ThemeHelper.Apply(this);
                ThemeHelper.SetEnabled(abort, strokeInProgress);
            };
            this.Shown += (themeSender, themeArgs) => ThemeHelper.Apply(this);

            this.brushcombobox.ComboBox.DrawMode = DrawMode.OwnerDrawVariable;
            this.brushcombobox.ComboBox.MeasureItem += new MeasureItemEventHandler(brushcombobox_MeasureItem);
            this.brushcombobox.ComboBox.ItemHeight = (int)Math.Round(16 * DpiScale);
            this.brushcombobox.ComboBox.DrawItem += new DrawItemEventHandler(brushcombobox_DrawItem);
            this.brushcombobox.DropDownHeight = this.Height - 100;

            pressure = new SliderControl();
            jitter = new SliderControl();

            float dpiScale = DpiScale;
            if (dpiScale > 1.01f)
            {
                // settingStrip's own button icons aren't scaled here: ToolStrip does that itself.
                pressure.Size = new Size((int)Math.Round(pressure.Width * dpiScale), (int)Math.Round(pressure.Height * dpiScale));
                jitter.Size = new Size((int)Math.Round(jitter.Width * dpiScale), (int)Math.Round(jitter.Height * dpiScale));

                brushcombobox.Size = new Size((int)Math.Round(brushcombobox.Width * dpiScale), brushcombobox.Height);
                brushSize.Size = new Size((int)Math.Round(brushSize.Width * dpiScale), brushSize.Height);
                zoom.Size = new Size((int)Math.Round(zoom.Width * dpiScale), zoom.Height);
            }

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
            renderer.Aborted += new EventHandler(renderer_Aborted);
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
                    EndStroke();
                }
            }
            catch (ObjectDisposedException) { }
        }

        // True from the start of a stroke until it has finished rendering or been aborted.
        private bool strokeInProgress;

        // Abort drops the queued end-of-stroke event, so an aborted stroke is finished off here too:
        // what was rendered becomes an undo step and the buttons come back.
        private void EndStroke()
        {
            if (!strokeInProgress)
            {
                return;
            }
            strokeInProgress = false;

            historystack.AddHistoryItem(surface, renderer.PopTotalInvalidRect());
            UpdateHistoryButtons(false);
            ok.Enabled = true;
            ThemeHelper.SetEnabled(abort, false);
        }

        // Raised on the render thread. Posted rather than invoked so the render thread never waits
        // on the UI thread, which may itself be waiting for the renderer while the dialog closes.
        void renderer_Aborted(object sender, EventArgs e)
        {
            try
            {
                if (IsHandleCreated && !IsDisposed)
                {
                    BeginInvoke(new Action(EndStroke));
                }
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
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
                    strokeInProgress = true;
                    UpdateHistoryButtons(true);
                    ok.Enabled = false;
                    ThemeHelper.SetEnabled(abort, true);
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
                    this.brushSize.BackColor = ThemeHelper.FieldBackColor;
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
            ((PaintDotNet.AppModel.IShellService)Services.GetService(typeof(PaintDotNet.AppModel.IShellService))).LaunchUrl(this, "http://forums.getpaint.net/index.php?showtopic=7291");
        }

        private unsafe Surface GetSourceAsClassicSurface()
        {
            SizeInt32 docSize = Environment.Document.Size;
            Surface result = new Surface(docSize.Width, docSize.Height);

            using (IEffectInputBitmap<ColorBgra32> srcBitmap = Environment.GetSourceBitmapBgra32())
            using (IBitmapLock<ColorBgra32> srcLock = srcBitmap.Lock(new RectInt32(0, 0, docSize.Width, docSize.Height)))
            {
                RegionPtr<ColorBgra32> srcRegion32 = new RegionPtr<ColorBgra32>(srcLock.Buffer, srcLock.Size, srcLock.BufferStride);
                RegionPtr<ColorBgra> dstRegion = new RegionPtr<ColorBgra>(result.GetPointPointer(0, 0), result.Width, result.Height, result.Stride);
                srcRegion32.Cast<ColorBgra>().CopyTo(dstRegion);
            }

            return result;
        }

        private void ConfigDialog_Load(object sender, EventArgs e)
        {
            int topMargin = (int)Math.Round(6 * DpiScale);
            settingStrip.Location = new Point(settingStrip.Location.X, settingStrip.Location.Y + topMargin);

            int newCanvasTop = settingStrip.Bottom;
            int topDelta = newCanvasTop - canvas.Top;
            canvas.Bounds = new Rectangle(canvas.Left, newCanvasTop, canvas.Width, canvas.Height - topDelta);

            brushcollection = new PngBrushCollection(Services, Smudge.RawName);
            CreateDefaultBrushes();
            OnBrushesChanged();

            this.Text = Smudge.StaticDialogName;
            surface = GetSourceAsClassicSurface();
            canvas.Surface = surface;

            canvas.Selection = CreateSelectionRegion();

            historystack = new HistoryStack(surface, false);

            InitializeRenderer();

            this.DesktopLocation = Owner.PointToScreen(new Point(0, 30));
            this.Size = new Size(Owner.ClientSize.Width, Owner.ClientSize.Height - 30);
            this.WindowState = Owner.WindowState;
        }

        private PdnRegion CreateSelectionRegion()
        {
            Rectangle[] scans = Environment.Selection.RenderScans
                .Select(r => new Rectangle(r.X, r.Y, r.Width, r.Height))
                .ToArray();

            using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath(System.Drawing.Drawing2D.FillMode.Winding))
            {
                if (scans.Length > 0)
                {
                    path.AddRectangles(scans);
                }
                return new PdnRegion(path);
            }
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

                brushDirSignature = GetBrushDirSignature();
                UpdateBrushDropDownWidth();
            }
        }

        private string brushDirSignature;

        private static string GetBrushDirSignature()
        {
            try
            {
                string dir = PngBrushCollection.BrushesPath;
                if (dir == null || !Directory.Exists(dir))
                {
                    return string.Empty;
                }

                StringBuilder sb = new StringBuilder();
                foreach (string file in Directory.GetFiles(dir, "*.png", SearchOption.TopDirectoryOnly))
                {
                    FileInfo fi = new FileInfo(file);
                    sb.Append(fi.Name).Append('|').Append(fi.Length).Append('|').Append(fi.LastWriteTimeUtc.Ticks).Append(';');
                }
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
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
            int newWidth = Math.Clamp(BrushSize + delta, minPenSize, maxPenSize);
            BrushSize = newWidth;
        }

        protected override EffectConfigToken OnCreateInitialToken()
        {
            return new ConfigToken();
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


        protected override void OnUpdateDialogFromToken(ConfigToken token)
        {
            brushcombobox.SelectedItem = token.brush;
            this.Pressure = token.strength;
            this.Jitter = token.jitter;
            this.BrushSize = token.width;
            this.Quality = token.quality;
        }

        // The token references `surface` and outlives this dialog ("Repeat <effect>"), so once
        // shared it must not be disposed here.
        private bool surfaceSharedWithToken;

        protected override void OnUpdateTokenFromDialog(ConfigToken token)
        {
            if (!this.IsDisposed)
            {
                token.width = this.BrushSize;
                token.strength = this.Pressure;
                token.jitter = this.Jitter;
                token.quality = this.Quality;
                token.surface = surface;
                surfaceSharedWithToken = true;

                if (this.brushcombobox.SelectedItem != null)
                    token.brush = (PngBrush)this.brushcombobox.SelectedItem;
            }
        }

        private void ok_Click(object sender, EventArgs e)
        {
            UpdateTokenFromDialog();
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
        }

        void brushcombobox_DropDown(object sender, EventArgs e)
        {
            // Rebuilding re-decodes every brush PNG, so only do it when the brushes folder changed.
            if (GetBrushDirSignature() != brushDirSignature)
            {
                OnBrushesChanged();
            }

            // The width computed at load may have been clamped to the pre-resize form width.
            UpdateBrushDropDownWidth();
        }

        // Always the thumbnail height; the collapsed field uses ComboBox.ItemHeight instead.
        // Re-measuring on open/close caused flicker.
        void brushcombobox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = (int)Math.Round(32 * DpiScale);
        }

        private void UpdateBrushDropDownWidth()
        {
            float dpiScale = DpiScale;
            int expandedItemHeight = (int)Math.Round(32 * dpiScale);
            int maxWidth = 0;

            using (Graphics g = brushcombobox.ComboBox.CreateGraphics())
            {
                for (int i = 0; i < brushcollection.Count; i++)
                {
                    int width = (int)g.MeasureString(brushcollection[i].Name, brushcombobox.Font).Width + expandedItemHeight;
                    width = Math.Max(width, (int)g.MeasureString(brushcollection[i].NativeSizePrettyString, brushcombobox.Font).Width + (int)Math.Round(72 * dpiScale));
                    maxWidth = Math.Max(maxWidth, width);
                }
            }

            maxWidth = Math.Clamp(maxWidth, 0, this.Width - 100);
            if (maxWidth > brushcombobox.ComboBox.DropDownWidth)
            {
                brushcombobox.DropDownWidth = maxWidth;
            }
        }

        void brushcombobox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.State == DrawItemState.Selected)
            {
                e.DrawFocusRectangle();
            }

            float dpiScale = DpiScale;
            int boxSize = (int)Math.Round(32 * dpiScale);
            int collapsedItemHeight = (int)Math.Round(16 * dpiScale);
            float thumbScale = boxSize / 32f;

            using (SolidBrush foreBrush = new SolidBrush(e.ForeColor))
            using (SolidBrush dimBrush = new SolidBrush(ColorBgra.Blend(new ColorBgra[] { ColorBgra.FromColor(e.ForeColor), ColorBgra.FromColor(e.BackColor) }).ToColor()))
            {
                if (e.Index == brushcombobox.Items.Count - 1)
                {
                    e.Graphics.DrawString(brushcombobox.Items[e.Index].ToString(), e.Font, dimBrush, e.Bounds.X, e.Bounds.Y + boxSize / 4);
                }
                else if (e.Index >= 0)
                {
                    PngBrush pngBrush = brushcollection[e.Index];
                    Bitmap image = pngBrush.ThumbnailAlphaOnlyBitmap;

                    if ((e.State & DrawItemState.ComboBoxEdit) == 0)
                    {
                        int drawWidth = (int)Math.Round(image.Width * thumbScale);
                        int drawHeight = (int)Math.Round(image.Height * thumbScale);
                        e.Graphics.DrawImage(image, e.Bounds.X + (boxSize - drawWidth) / 2, e.Bounds.Y + (boxSize - drawHeight) / 2, drawWidth, drawHeight);
                        e.Graphics.DrawString(pngBrush.Name, e.Font, foreBrush, e.Bounds.X + boxSize, e.Bounds.Y);
                        e.Graphics.DrawString(pngBrush.NativeSizePrettyString, e.Font, dimBrush, e.Bounds.X + (int)Math.Round(64 * dpiScale), e.Bounds.Y + (int)Math.Round(16 * dpiScale));
                    }
                    else
                    {
                        int drawWidth = (int)Math.Round(image.Width / 2f * thumbScale);
                        int drawHeight = (int)Math.Round(image.Height / 2f * thumbScale);
                        e.Graphics.DrawImage(image, e.Bounds.X + (collapsedItemHeight - drawWidth) / 2, e.Bounds.Y + (collapsedItemHeight - drawHeight) / 2, drawWidth, drawHeight);
                        e.Graphics.DrawString(pngBrush.Name, e.Font, foreBrush, e.Bounds.X + collapsedItemHeight, e.Bounds.Y);
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
                    ((PaintDotNet.AppModel.IShellService)Services.GetService(typeof(PaintDotNet.AppModel.IShellService))).LaunchFolder(this, PngBrushCollection.BrushesPath);
                }
            }
            catch
            { }
        }

        private void abort_Click(object sender, EventArgs e)
        {
            // In dark mode the button only looks disabled, so it can still be clicked.
            if (strokeInProgress)
            {
                renderer.Abort();
            }
        }
    }
}
