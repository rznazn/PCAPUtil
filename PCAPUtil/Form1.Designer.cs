namespace PCAPUtil
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.dgvCaptures = new System.Windows.Forms.DataGridView();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ledBlinky = new EARS.LED();
            this.ledRunning = new EARS.LED();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCaptures)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvCaptures
            // 
            this.dgvCaptures.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvCaptures.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCaptures.Location = new System.Drawing.Point(12, 51);
            this.dgvCaptures.Name = "dgvCaptures";
            this.dgvCaptures.Size = new System.Drawing.Size(859, 172);
            this.dgvCaptures.TabIndex = 0;
            this.dgvCaptures.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCaptures_CellClick);
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(174, 13);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 1;
            this.btnRun.Text = "run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(12, 12);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(75, 23);
            this.btnLoad.TabIndex = 2;
            this.btnLoad.Text = "load config";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(93, 13);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "save config";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(689, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Semi-superfluous Blinky Light";
            // 
            // ledBlinky
            // 
            this.ledBlinky.BackColor = System.Drawing.Color.Transparent;
            this.ledBlinky.Location = new System.Drawing.Point(839, 13);
            this.ledBlinky.Name = "ledBlinky";
            this.ledBlinky.Size = new System.Drawing.Size(22, 22);
            this.ledBlinky.TabIndex = 5;
            // 
            // ledRunning
            // 
            this.ledRunning.BackColor = System.Drawing.Color.Transparent;
            this.ledRunning.Location = new System.Drawing.Point(255, 13);
            this.ledRunning.Name = "ledRunning";
            this.ledRunning.Size = new System.Drawing.Size(22, 22);
            this.ledRunning.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(887, 243);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ledBlinky);
            this.Controls.Add(this.ledRunning);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.dgvCaptures);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "PCAP Util";
            ((System.ComponentModel.ISupportInitialize)(this.dgvCaptures)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvCaptures;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnSave;
        private EARS.LED ledRunning;
        private EARS.LED ledBlinky;
        private System.Windows.Forms.Label label1;
    }
}

