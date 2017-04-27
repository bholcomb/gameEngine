namespace ParticleEditor
{
   partial class Form1
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
         if (disposing && (components != null))
         {
            components.Dispose();
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
         this.menuStrip1 = new System.Windows.Forms.MenuStrip();
         this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
         this.splitContainer1 = new System.Windows.Forms.SplitContainer();
         this.featureFlowLayout = new System.Windows.Forms.FlowLayoutPanel();
         this.featureCollection = new System.Windows.Forms.GroupBox();
         this.button1 = new System.Windows.Forms.Button();
         this.particleSystemPropGrid = new System.Windows.Forms.PropertyGrid();
         this.glControl1 = new OpenTK.GLControl();
         this.menuStrip1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
         this.splitContainer1.Panel1.SuspendLayout();
         this.splitContainer1.Panel2.SuspendLayout();
         this.splitContainer1.SuspendLayout();
         this.SuspendLayout();
         // 
         // menuStrip1
         // 
         this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.helpToolStripMenuItem});
         this.menuStrip1.Location = new System.Drawing.Point(0, 0);
         this.menuStrip1.Name = "menuStrip1";
         this.menuStrip1.Size = new System.Drawing.Size(1264, 24);
         this.menuStrip1.TabIndex = 0;
         this.menuStrip1.Text = "menuStrip1";
         // 
         // fileToolStripMenuItem
         // 
         this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.exitToolStripMenuItem});
         this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
         this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
         this.fileToolStripMenuItem.Text = "File";
         // 
         // loadToolStripMenuItem
         // 
         this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
         this.loadToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
         this.loadToolStripMenuItem.Text = "Load";
         this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
         // 
         // saveToolStripMenuItem
         // 
         this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
         this.saveToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
         this.saveToolStripMenuItem.Text = "Save";
         this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
         // 
         // exportToolStripMenuItem
         // 
         this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
         this.exportToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
         this.exportToolStripMenuItem.Text = "Export";
         // 
         // exitToolStripMenuItem
         // 
         this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
         this.exitToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
         this.exitToolStripMenuItem.Text = "Exit";
         // 
         // editToolStripMenuItem
         // 
         this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem});
         this.editToolStripMenuItem.Name = "editToolStripMenuItem";
         this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
         this.editToolStripMenuItem.Text = "Edit";
         // 
         // copyToolStripMenuItem
         // 
         this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
         this.copyToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
         this.copyToolStripMenuItem.Text = "Copy";
         // 
         // pasteToolStripMenuItem
         // 
         this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
         this.pasteToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
         this.pasteToolStripMenuItem.Text = "Paste";
         // 
         // helpToolStripMenuItem
         // 
         this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem,
            this.aboutToolStripMenuItem1});
         this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
         this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
         this.helpToolStripMenuItem.Text = "Help";
         // 
         // aboutToolStripMenuItem
         // 
         this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
         this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
         this.aboutToolStripMenuItem.Text = "Help";
         // 
         // aboutToolStripMenuItem1
         // 
         this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
         this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(107, 22);
         this.aboutToolStripMenuItem1.Text = "About";
         // 
         // splitContainer1
         // 
         this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.splitContainer1.Location = new System.Drawing.Point(0, 24);
         this.splitContainer1.Name = "splitContainer1";
         // 
         // splitContainer1.Panel1
         // 
         this.splitContainer1.Panel1.Controls.Add(this.glControl1);
         // 
         // splitContainer1.Panel2
         // 
         this.splitContainer1.Panel2.Controls.Add(this.featureFlowLayout);
         this.splitContainer1.Panel2.Controls.Add(this.featureCollection);
         this.splitContainer1.Panel2.Controls.Add(this.button1);
         this.splitContainer1.Panel2.Controls.Add(this.particleSystemPropGrid);
         this.splitContainer1.Size = new System.Drawing.Size(1264, 706);
         this.splitContainer1.SplitterDistance = 1026;
         this.splitContainer1.TabIndex = 1;
         // 
         // featureFlowLayout
         // 
         this.featureFlowLayout.AutoScroll = true;
         this.featureFlowLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
         this.featureFlowLayout.Location = new System.Drawing.Point(9, 303);
         this.featureFlowLayout.Name = "featureFlowLayout";
         this.featureFlowLayout.Size = new System.Drawing.Size(216, 400);
         this.featureFlowLayout.TabIndex = 0;
         this.featureFlowLayout.WrapContents = false;
         // 
         // featureCollection
         // 
         this.featureCollection.Location = new System.Drawing.Point(3, 284);
         this.featureCollection.Name = "featureCollection";
         this.featureCollection.Size = new System.Drawing.Size(228, 419);
         this.featureCollection.TabIndex = 2;
         this.featureCollection.TabStop = false;
         this.featureCollection.Text = "Features";
         // 
         // button1
         // 
         this.button1.Location = new System.Drawing.Point(78, 3);
         this.button1.Name = "button1";
         this.button1.Size = new System.Drawing.Size(25, 23);
         this.button1.TabIndex = 1;
         this.button1.Text = "+";
         this.button1.UseVisualStyleBackColor = true;
         // 
         // particleSystemPropGrid
         // 
         this.particleSystemPropGrid.Location = new System.Drawing.Point(2, 3);
         this.particleSystemPropGrid.Name = "particleSystemPropGrid";
         this.particleSystemPropGrid.Size = new System.Drawing.Size(232, 275);
         this.particleSystemPropGrid.TabIndex = 0;
         // 
         // glControl1
         // 
         this.glControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
         this.glControl1.BackColor = System.Drawing.Color.Black;
         this.glControl1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.glControl1.Location = new System.Drawing.Point(0, 0);
         this.glControl1.Name = "glControl1";
         this.glControl1.Size = new System.Drawing.Size(1026, 706);
         this.glControl1.TabIndex = 0;
         this.glControl1.VSync = false;
         this.glControl1.Load += new System.EventHandler(this.glControl1_Load);
         this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
         this.glControl1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.glControl1_KeyDown);
         this.glControl1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.glControl1_KeyUp);
         this.glControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseDown);
         this.glControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseMove);
         this.glControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseUp);
         this.glControl1.Resize += new System.EventHandler(this.glControl1_Resize);
         // 
         // Form1
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(1264, 730);
         this.Controls.Add(this.splitContainer1);
         this.Controls.Add(this.menuStrip1);
         this.MainMenuStrip = this.menuStrip1;
         this.Name = "Form1";
         this.Text = "Form1";
         this.menuStrip1.ResumeLayout(false);
         this.menuStrip1.PerformLayout();
         this.splitContainer1.Panel1.ResumeLayout(false);
         this.splitContainer1.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
         this.splitContainer1.ResumeLayout(false);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.MenuStrip menuStrip1;
      private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
      private System.Windows.Forms.SplitContainer splitContainer1;
      private System.Windows.Forms.PropertyGrid particleSystemPropGrid;
      private System.Windows.Forms.Button button1;
      private System.Windows.Forms.GroupBox featureCollection;
      private System.Windows.Forms.FlowLayoutPanel featureFlowLayout;
      private OpenTK.GLControl glControl1;
   }
}

