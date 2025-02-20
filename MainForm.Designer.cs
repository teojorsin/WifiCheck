// This file under the MIT license.
// See LICENSE for details.

namespace WifiCheck
{
    partial class MainForm
    {
        /// <summary>
        /// Designer variable used to keep track of non-visual components.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Disposes resources used by the form.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// This method is required for Windows Forms designer support.
        /// Do not change the method contents inside the source code editor. The Forms designer might
        /// not be able to load this method if it was changed manually.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.uiListWifi = new System.Windows.Forms.ListView();
            this.columnHeaderProfile = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderSSID = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderChannel = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderQuality = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderSecurity = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderBSSIDs = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderVendor = new System.Windows.Forms.ColumnHeader();
            this.timerSignalQuality = new System.Windows.Forms.Timer(this.components);
            this.uiChannelGraph = new WifiCheck.APChannelGraph();
            this.uiButtonUpdate = new System.Windows.Forms.Button();
            this.uiButtonScan = new System.Windows.Forms.Button();
            this.animationTimer = new System.Windows.Forms.Timer(this.components);
            this.uiRadioBand24 = new System.Windows.Forms.RadioButton();
            this.uiRadioBand5k = new System.Windows.Forms.RadioButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiListWifi
            // 
            this.uiListWifi.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.uiListWifi.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.uiListWifi.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                    this.columnHeaderSSID,
                                    this.columnHeaderChannel,
                                    this.columnHeaderQuality,
                                    this.columnHeaderSecurity,
                                    this.columnHeaderBSSIDs,
                                    this.columnHeaderVendor,
                                    this.columnHeaderProfile});
            this.uiListWifi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiListWifi.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uiListWifi.ForeColor = System.Drawing.Color.Gainsboro;
            this.uiListWifi.FullRowSelect = true;
            this.uiListWifi.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.uiListWifi.Location = new System.Drawing.Point(0, 0);
            this.uiListWifi.Name = "uiListWifi";
            this.uiListWifi.ShowGroups = false;
            this.uiListWifi.Size = new System.Drawing.Size(684, 293);
            this.uiListWifi.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.uiListWifi.TabIndex = 0;
            this.uiListWifi.UseCompatibleStateImageBehavior = false;
            this.uiListWifi.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderProfile
            // 
            this.columnHeaderProfile.Text = "P";
            this.columnHeaderProfile.Width = 30;
            // 
            // columnHeaderSSID
            // 
            this.columnHeaderSSID.Text = "SSID";
            this.columnHeaderSSID.Width = 216;
            // 
            // columnHeaderChannel
            // 
            this.columnHeaderChannel.Text = "Channel";
            // 
            // columnHeaderQuality
            // 
            this.columnHeaderQuality.Text = "Quality";
            // 
            // columnHeaderSecurity
            // 
            this.columnHeaderSecurity.Text = "Security";
            this.columnHeaderSecurity.Width = 100;
            // 
            // columnHeaderBSSIDs
            // 
            this.columnHeaderBSSIDs.Text = "BSSID (Mac)";
            this.columnHeaderBSSIDs.Width = 120;
            // 
            // columnHeaderVendor
            // 
            this.columnHeaderVendor.Text = "Vendor";
            this.columnHeaderVendor.Width = 120;
            // 
            // timerSignalQuality
            // 
            this.timerSignalQuality.Enabled = true;
            this.timerSignalQuality.Interval = 2000;
            this.timerSignalQuality.Tick += new System.EventHandler(this.TimerSignalQualityTick);
            // 
            // uiChannelGraph
            // 
            this.uiChannelGraph.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(24)))));
            this.uiChannelGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiChannelGraph.Location = new System.Drawing.Point(0, 0);
            this.uiChannelGraph.Name = "uiChannelGraph";
            this.uiChannelGraph.Size = new System.Drawing.Size(684, 253);
            this.uiChannelGraph.TabIndex = 1;
            // 
            // uiButtonUpdate
            // 
            this.uiButtonUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.uiButtonUpdate.Location = new System.Drawing.Point(587, 558);
            this.uiButtonUpdate.Name = "uiButtonUpdate";
            this.uiButtonUpdate.Size = new System.Drawing.Size(85, 31);
            this.uiButtonUpdate.TabIndex = 2;
            this.uiButtonUpdate.Text = "Update";
            this.uiButtonUpdate.UseVisualStyleBackColor = true;
            this.uiButtonUpdate.Visible = false;
            this.uiButtonUpdate.Click += new System.EventHandler(this.UiButtonUpdateClick);
            // 
            // uiButtonScan
            // 
            this.uiButtonScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.uiButtonScan.Location = new System.Drawing.Point(500, 558);
            this.uiButtonScan.Name = "uiButtonScan";
            this.uiButtonScan.Size = new System.Drawing.Size(81, 31);
            this.uiButtonScan.TabIndex = 3;
            this.uiButtonScan.Text = "Scan";
            this.uiButtonScan.UseVisualStyleBackColor = true;
            this.uiButtonScan.Visible = false;
            this.uiButtonScan.Click += new System.EventHandler(this.UiButtonScanClick);
            // 
            // animationTimer
            // 
            this.animationTimer.Interval = 25;
            this.animationTimer.Tick += new System.EventHandler(this.AnimationTimerTick);
            // 
            // uiRadioBand24
            // 
            this.uiRadioBand24.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.uiRadioBand24.Appearance = System.Windows.Forms.Appearance.Button;
            this.uiRadioBand24.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.uiRadioBand24.Checked = true;
            this.uiRadioBand24.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.uiRadioBand24.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.uiRadioBand24.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.uiRadioBand24.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uiRadioBand24.Location = new System.Drawing.Point(248, 558);
            this.uiRadioBand24.Name = "uiRadioBand24";
            this.uiRadioBand24.Size = new System.Drawing.Size(81, 36);
            this.uiRadioBand24.TabIndex = 4;
            this.uiRadioBand24.TabStop = true;
            this.uiRadioBand24.Text = "2.4 GHz";
            this.uiRadioBand24.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.uiRadioBand24.UseVisualStyleBackColor = false;
            this.uiRadioBand24.CheckedChanged += new System.EventHandler(this.UiRadioBand24CheckedChanged);
            // 
            // uiRadioBand5k
            // 
            this.uiRadioBand5k.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.uiRadioBand5k.Appearance = System.Windows.Forms.Appearance.Button;
            this.uiRadioBand5k.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.uiRadioBand5k.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.uiRadioBand5k.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.uiRadioBand5k.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.uiRadioBand5k.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uiRadioBand5k.Location = new System.Drawing.Point(328, 558);
            this.uiRadioBand5k.Name = "uiRadioBand5k";
            this.uiRadioBand5k.Size = new System.Drawing.Size(81, 36);
            this.uiRadioBand5k.TabIndex = 5;
            this.uiRadioBand5k.Text = "5 GHz";
            this.uiRadioBand5k.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.uiRadioBand5k.UseVisualStyleBackColor = false;
            this.uiRadioBand5k.CheckedChanged += new System.EventHandler(this.UiRadioBand5kCheckedChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                    | System.Windows.Forms.AnchorStyles.Left)
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(1, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.uiListWifi);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.uiChannelGraph);
            this.splitContainer1.Size = new System.Drawing.Size(684, 552);
            this.splitContainer1.SplitterDistance = 293;
            this.splitContainer1.SplitterWidth = 6;
            this.splitContainer1.TabIndex = 6;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(685, 601);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.uiRadioBand5k);
            this.Controls.Add(this.uiRadioBand24);
            this.Controls.Add(this.uiButtonScan);
            this.Controls.Add(this.uiButtonUpdate);
            this.Name = "MainForm";
            this.Text = "WifiCheck";
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.ColumnHeader columnHeaderProfile;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.RadioButton uiRadioBand5k;
        private System.Windows.Forms.RadioButton uiRadioBand24;
        private System.Windows.Forms.Timer animationTimer;
        private System.Windows.Forms.ColumnHeader columnHeaderVendor;
        private System.Windows.Forms.Button uiButtonScan;
        private System.Windows.Forms.Button uiButtonUpdate;
        private WifiCheck.APChannelGraph uiChannelGraph;
        private System.Windows.Forms.Timer timerSignalQuality;
        private System.Windows.Forms.ColumnHeader columnHeaderBSSIDs;
        private System.Windows.Forms.ColumnHeader columnHeaderQuality;
        private System.Windows.Forms.ColumnHeader columnHeaderSecurity;
        private System.Windows.Forms.ColumnHeader columnHeaderChannel;
        private System.Windows.Forms.ColumnHeader columnHeaderSSID;
        private System.Windows.Forms.ListView uiListWifi;
    }
}