namespace OP2StreamingTools
{
    partial class fMain
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
            this.components = new System.ComponentModel.Container();
            this.lblMorale = new System.Windows.Forms.Label();
            this.lblGameMark = new System.Windows.Forms.Label();
            this.lblTotalColonists = new System.Windows.Forms.Label();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblTimer = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.chkMorale = new System.Windows.Forms.CheckBox();
            this.chkTotalColonists = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkGameMark = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lblMorale
            // 
            this.lblMorale.AutoSize = true;
            this.lblMorale.Location = new System.Drawing.Point(8, 63);
            this.lblMorale.Name = "lblMorale";
            this.lblMorale.Size = new System.Drawing.Size(51, 13);
            this.lblMorale.TabIndex = 19;
            this.lblMorale.Text = "Morale: 0";
            // 
            // lblGameMark
            // 
            this.lblGameMark.AutoSize = true;
            this.lblGameMark.Location = new System.Drawing.Point(8, 43);
            this.lblGameMark.Name = "lblGameMark";
            this.lblGameMark.Size = new System.Drawing.Size(80, 13);
            this.lblGameMark.TabIndex = 20;
            this.lblGameMark.Text = "Current Mark: 0";
            // 
            // lblTotalColonists
            // 
            this.lblTotalColonists.AutoSize = true;
            this.lblTotalColonists.Location = new System.Drawing.Point(8, 83);
            this.lblTotalColonists.Name = "lblTotalColonists";
            this.lblTotalColonists.Size = new System.Drawing.Size(88, 13);
            this.lblTotalColonists.TabIndex = 21;
            this.lblTotalColonists.Text = "Total Colonists: 0";
            // 
            // updateTimer
            // 
            this.updateTimer.Interval = 1000;
            this.updateTimer.Tick += new System.EventHandler(this.updateTimer_Tick);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(8, 134);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(47, 13);
            this.lblStatus.TabIndex = 22;
            this.lblStatus.Text = "lblStatus";
            // 
            // lblTimer
            // 
            this.lblTimer.AutoSize = true;
            this.lblTimer.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTimer.Location = new System.Drawing.Point(8, 8);
            this.lblTimer.Name = "lblTimer";
            this.lblTimer.Size = new System.Drawing.Size(114, 26);
            this.lblTimer.TabIndex = 23;
            this.lblTimer.Text = "00:00:00.0";
            this.lblTimer.Click += new System.EventHandler(this.lblTimer_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(162, 230);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(74, 23);
            this.btnStart.TabIndex = 24;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // chkMorale
            // 
            this.chkMorale.AutoSize = true;
            this.chkMorale.Checked = true;
            this.chkMorale.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMorale.Location = new System.Drawing.Point(11, 213);
            this.chkMorale.Name = "chkMorale";
            this.chkMorale.Size = new System.Drawing.Size(76, 17);
            this.chkMorale.TabIndex = 25;
            this.chkMorale.Text = "chkMorale";
            this.chkMorale.UseVisualStyleBackColor = true;
            // 
            // chkTotalColonists
            // 
            this.chkTotalColonists.AutoSize = true;
            this.chkTotalColonists.Checked = true;
            this.chkTotalColonists.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTotalColonists.Location = new System.Drawing.Point(11, 236);
            this.chkTotalColonists.Name = "chkTotalColonists";
            this.chkTotalColonists.Size = new System.Drawing.Size(110, 17);
            this.chkTotalColonists.TabIndex = 26;
            this.chkTotalColonists.Text = "chkTotalColonists";
            this.chkTotalColonists.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 164);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 27;
            this.label1.Text = "Display:";
            // 
            // chkGameMark
            // 
            this.chkGameMark.AutoSize = true;
            this.chkGameMark.Checked = true;
            this.chkGameMark.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGameMark.Location = new System.Drawing.Point(11, 190);
            this.chkGameMark.Name = "chkGameMark";
            this.chkGameMark.Size = new System.Drawing.Size(96, 17);
            this.chkGameMark.TabIndex = 28;
            this.chkGameMark.Text = "chkGameMark";
            this.chkGameMark.UseVisualStyleBackColor = true;
            // 
            // fMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(397, 305);
            this.Controls.Add(this.chkGameMark);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkTotalColonists);
            this.Controls.Add(this.chkMorale);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.lblTimer);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblTotalColonists);
            this.Controls.Add(this.lblGameMark);
            this.Controls.Add(this.lblMorale);
            this.MaximizeBox = false;
            this.Name = "fMain";
            this.Text = "Outpost 2";
            this.Load += new System.EventHandler(this.fMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMorale;
        private System.Windows.Forms.Label lblGameMark;
        private System.Windows.Forms.Label lblTotalColonists;
        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblTimer;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.CheckBox chkMorale;
        private System.Windows.Forms.CheckBox chkTotalColonists;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkGameMark;
    }
}

