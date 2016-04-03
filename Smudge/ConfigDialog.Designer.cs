using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using pyrochild.effects.common;

namespace pyrochild.effects.smudge
{
    partial class ConfigDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
                if (historystack != null)
                    historystack.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.settingStrip = new System.Windows.Forms.ToolStrip();
            this.brushSizeSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.brushcombobox = new System.Windows.Forms.ToolStripComboBox();
            this.brushSizeLabel = new System.Windows.Forms.ToolStripLabel();
            this.brushSizeDecrement = new System.Windows.Forms.ToolStripButton();
            this.brushSize = new System.Windows.Forms.ToolStripComboBox();
            this.brushSizeIncrement = new System.Windows.Forms.ToolStripButton();
            this.brushSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.pressureLabel = new System.Windows.Forms.ToolStripLabel();
            this.densityLabel = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.undo = new System.Windows.Forms.ToolStripButton();
            this.redo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.zoomOut = new System.Windows.Forms.ToolStripButton();
            this.zoom = new System.Windows.Forms.ToolStripComboBox();
            this.zoomIn = new System.Windows.Forms.ToolStripButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.quality = new pyrochild.effects.common.SliderControl();
            this.abort = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ok = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.donate = new System.Windows.Forms.LinkLabel();
            this.tooltip = new System.Windows.Forms.ToolTip(this.components);
            this.canvas = new pyrochild.effects.common.CanvasPanel();
            this.settingStrip.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // settingStrip
            // 
            this.settingStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.settingStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.brushSizeSeparator,
            this.brushcombobox,
            this.brushSizeLabel,
            this.brushSizeDecrement,
            this.brushSize,
            this.brushSizeIncrement,
            this.brushSeparator,
            this.pressureLabel,
            this.densityLabel,
            this.toolStripSeparator1,
            this.undo,
            this.redo,
            this.toolStripSeparator2,
            this.zoomOut,
            this.zoom,
            this.zoomIn});
            this.settingStrip.Location = new System.Drawing.Point(0, 0);
            this.settingStrip.Name = "settingStrip";
            this.settingStrip.Size = new System.Drawing.Size(684, 25);
            this.settingStrip.TabIndex = 1;
            this.settingStrip.Text = "toolStrip1";
            // 
            // brushSizeSeparator
            // 
            this.brushSizeSeparator.Name = "brushSizeSeparator";
            this.brushSizeSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // brushcombobox
            // 
            this.brushcombobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.brushcombobox.Name = "brushcombobox";
            this.brushcombobox.Size = new System.Drawing.Size(110, 25);
            this.brushcombobox.DropDown += new System.EventHandler(this.brushcombobox_DropDown);
            this.brushcombobox.DropDownClosed += new System.EventHandler(this.brushcombobox_DropDownClosed);
            this.brushcombobox.SelectedIndexChanged += new System.EventHandler(this.brushcombobox_SelectedIndexChanged);
            // 
            // brushSizeLabel
            // 
            this.brushSizeLabel.Name = "brushSizeLabel";
            this.brushSizeLabel.Size = new System.Drawing.Size(30, 22);
            this.brushSizeLabel.Text = "Size:";
            // 
            // brushSizeDecrement
            // 
            this.brushSizeDecrement.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.brushSizeDecrement.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.brushSizeDecrement.Name = "brushSizeDecrement";
            this.brushSizeDecrement.Size = new System.Drawing.Size(23, 22);
            this.brushSizeDecrement.Click += new System.EventHandler(this.brushSizeDecrement_Click);
            // 
            // brushSize
            // 
            this.brushSize.AutoSize = false;
            this.brushSize.Name = "brushSize";
            this.brushSize.Size = new System.Drawing.Size(44, 23);
            this.brushSize.Validating += new System.ComponentModel.CancelEventHandler(this.brushSize_Validating);
            this.brushSize.TextChanged += new System.EventHandler(this.brushSize_Validating);
            // 
            // brushSizeIncrement
            // 
            this.brushSizeIncrement.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.brushSizeIncrement.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.brushSizeIncrement.Name = "brushSizeIncrement";
            this.brushSizeIncrement.Size = new System.Drawing.Size(23, 22);
            this.brushSizeIncrement.Click += new System.EventHandler(this.brushSizeIncrement_Click);
            // 
            // brushSeparator
            // 
            this.brushSeparator.Name = "brushSeparator";
            this.brushSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // pressureLabel
            // 
            this.pressureLabel.Name = "pressureLabel";
            this.pressureLabel.Size = new System.Drawing.Size(54, 22);
            this.pressureLabel.Text = "Pressure:";
            // 
            // densityLabel
            // 
            this.densityLabel.Name = "densityLabel";
            this.densityLabel.Size = new System.Drawing.Size(35, 22);
            this.densityLabel.Text = "Jitter:";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // undo
            // 
            this.undo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.undo.Enabled = false;
            this.undo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.undo.Name = "undo";
            this.undo.Size = new System.Drawing.Size(23, 22);
            this.undo.Click += new System.EventHandler(this.undo_Click);
            // 
            // redo
            // 
            this.redo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.redo.Enabled = false;
            this.redo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.redo.Name = "redo";
            this.redo.Size = new System.Drawing.Size(23, 22);
            this.redo.Click += new System.EventHandler(this.redo_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // zoomOut
            // 
            this.zoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.zoomOut.Name = "zoomOut";
            this.zoomOut.Size = new System.Drawing.Size(23, 22);
            this.zoomOut.Click += new System.EventHandler(this.zoomOut_Click);
            // 
            // zoom
            // 
            this.zoom.AutoSize = false;
            this.zoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.zoom.Name = "zoom";
            this.zoom.Size = new System.Drawing.Size(58, 23);
            this.zoom.SelectedIndexChanged += new System.EventHandler(this.zoom_SelectedIndexChanged);
            // 
            // zoomIn
            // 
            this.zoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.zoomIn.Name = "zoomIn";
            this.zoomIn.Size = new System.Drawing.Size(23, 22);
            this.zoomIn.Click += new System.EventHandler(this.zoomIn_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.quality);
            this.panel1.Controls.Add(this.abort);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.ok);
            this.panel1.Controls.Add(this.cancel);
            this.panel1.Controls.Add(this.donate);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 412);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(684, 30);
            this.panel1.TabIndex = 1;
            // 
            // quality
            // 
            this.quality.BackColor = System.Drawing.Color.White;
            this.quality.ForeColor = System.Drawing.Color.DarkGreen;
            this.quality.Location = new System.Drawing.Point(174, 7);
            this.quality.Maximum = 1F;
            this.quality.Minimum = 0F;
            this.quality.Name = "quality";
            this.quality.Size = new System.Drawing.Size(100, 16);
            this.quality.TabIndex = 6;
            this.quality.Text = "sliderControl1";
            this.quality.Value = 0.5F;
            // 
            // abort
            // 
            this.abort.Location = new System.Drawing.Point(280, 4);
            this.abort.Name = "abort";
            this.abort.Size = new System.Drawing.Size(75, 23);
            this.abort.TabIndex = 3;
            this.abort.Text = "Abort";
            this.abort.UseVisualStyleBackColor = true;
            this.abort.Click += new System.EventHandler(this.abort_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(104, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Smoothness:";
            // 
            // ok
            // 
            this.ok.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ok.Location = new System.Drawing.Point(525, 4);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 23);
            this.ok.TabIndex = 3;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // cancel
            // 
            this.cancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(606, 4);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 4;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // donate
            // 
            this.donate.AutoSize = true;
            this.donate.Location = new System.Drawing.Point(3, 9);
            this.donate.Name = "donate";
            this.donate.Size = new System.Drawing.Size(45, 13);
            this.donate.TabIndex = 5;
            this.donate.TabStop = true;
            this.donate.Text = "Donate!";
            this.donate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.donate_LinkClicked);
            // 
            // canvas
            // 
            this.canvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.canvas.AutoScroll = true;
            this.canvas.AutoScrollMargin = new System.Drawing.Size(10, 10);
            this.canvas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.canvas.BrushSize = 0;
            this.canvas.CanvasBackColor = System.Drawing.Color.Transparent;
            this.canvas.Location = new System.Drawing.Point(0, 25);
            this.canvas.MouseHoldInterval = 50;
            this.canvas.Name = "canvas";
            this.canvas.Selection = null;
            this.canvas.Size = new System.Drawing.Size(684, 387);
            this.canvas.Surface = null;
            this.canvas.TabIndex = 2;
            this.canvas.ZoomFactor = 1F;
            this.canvas.CanvasMouseDown += new System.EventHandler<pyrochild.effects.common.CanvasMouseEventArgs>(this.canvas_CanvasMouseDown);
            this.canvas.CanvasMouseMove += new System.EventHandler<pyrochild.effects.common.CanvasMouseEventArgs>(this.canvas_CanvasMouseMove);
            this.canvas.CanvasMouseUp += new System.EventHandler<pyrochild.effects.common.CanvasMouseEventArgs>(this.canvas_CanvasMouseUp);
            this.canvas.ZoomFactorChanged += new System.EventHandler(this.canvas_ZoomFactorChanged);
            // 
            // ConfigDialog
            // 
            this.AcceptButton = this.ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.cancel;
            this.ClientSize = new System.Drawing.Size(684, 442);
            this.Controls.Add(this.canvas);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.settingStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.KeyPreview = true;
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = true;
            this.MinimumSize = new System.Drawing.Size(700, 480);
            this.Name = "ConfigDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Auto;
            this.Load += new System.EventHandler(this.ConfigDialog_Load);
            this.Controls.SetChildIndex(this.settingStrip, 0);
            this.Controls.SetChildIndex(this.panel1, 0);
            this.Controls.SetChildIndex(this.canvas, 0);
            this.settingStrip.ResumeLayout(false);
            this.settingStrip.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        
        #endregion

        private ToolStrip settingStrip;
        private Panel panel1;
        private Button ok;
        private Button cancel;
        private LinkLabel donate;
        private ToolStripSeparator brushSizeSeparator;
        private ToolStripLabel brushSizeLabel;
        private ToolStripButton brushSizeDecrement;
        private ToolStripComboBox brushSize;
        private ToolStripButton brushSizeIncrement;
        private ToolStripSeparator brushSeparator;
        private ToolStripLabel pressureLabel;
        private ToolStripLabel densityLabel;
        private CanvasPanel canvas;
        private ToolTip tooltip;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton undo;
        private ToolStripButton redo;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton zoomOut;
        private ToolStripComboBox zoom;
        private ToolStripButton zoomIn;
        private Label label1;
        private Button abort;
        private ToolStripComboBox brushcombobox;
        private SliderControl quality;
    }
}