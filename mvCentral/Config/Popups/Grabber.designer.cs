namespace mvCentral.ConfigScreen.Popups
{
    partial class GrabberPopup
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }

            // Clean-up DirectShow interfaces
            CloseInterfaces();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.panVideoWin = new System.Windows.Forms.Panel();
            this.seekbar = new System.Windows.Forms.TrackBar();
            this.btnFrameStep = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnGrabFrame = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.seekbar)).BeginInit();
            this.SuspendLayout();
            // 
            // panVideoWin
            // 
            this.panVideoWin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panVideoWin.Dock = System.Windows.Forms.DockStyle.Top;
            this.panVideoWin.Location = new System.Drawing.Point(0, 0);
            this.panVideoWin.Name = "panVideoWin";
            this.panVideoWin.Size = new System.Drawing.Size(575, 284);
            this.panVideoWin.TabIndex = 98;
            // 
            // seekbar
            // 
            this.seekbar.Enabled = false;
            this.seekbar.Location = new System.Drawing.Point(14, 319);
            this.seekbar.Name = "seekbar";
            this.seekbar.Size = new System.Drawing.Size(350, 45);
            this.seekbar.TabIndex = 103;
            this.seekbar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.seekbar.Scroll += new System.EventHandler(this.seekbar_Scroll);
            this.seekbar.MouseCaptureChanged += new System.EventHandler(this.seekbar_MouseCaptureChanged);
            this.seekbar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.seekbar_MouseDown);
            // 
            // btnFrameStep
            // 
            this.btnFrameStep.Location = new System.Drawing.Point(287, 290);
            this.btnFrameStep.Name = "btnFrameStep";
            this.btnFrameStep.Size = new System.Drawing.Size(75, 23);
            this.btnFrameStep.TabIndex = 102;
            this.btnFrameStep.Text = "Step";
            this.btnFrameStep.UseVisualStyleBackColor = true;
            this.btnFrameStep.Click += new System.EventHandler(this.btnFrameStep_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(196, 290);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 101;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(105, 290);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(75, 23);
            this.btnPause.TabIndex = 100;
            this.btnPause.Text = "Pause";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.Location = new System.Drawing.Point(14, 290);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(75, 23);
            this.btnPlay.TabIndex = 99;
            this.btnPlay.Text = "Play";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(488, 290);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 104;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnGrabFrame
            // 
            this.btnGrabFrame.Location = new System.Drawing.Point(381, 290);
            this.btnGrabFrame.Name = "btnGrabFrame";
            this.btnGrabFrame.Size = new System.Drawing.Size(75, 23);
            this.btnGrabFrame.TabIndex = 105;
            this.btnGrabFrame.Text = "Grab Frame";
            this.btnGrabFrame.UseVisualStyleBackColor = true;
            this.btnGrabFrame.Click += new System.EventHandler(this.btnGrabFrame_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 200;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // GrabberPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(575, 356);
            this.Controls.Add(this.btnGrabFrame);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.seekbar);
            this.Controls.Add(this.btnFrameStep);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.panVideoWin);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "GrabberPopup";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Art";
            ((System.ComponentModel.ISupportInitialize)(this.seekbar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panVideoWin;
        private System.Windows.Forms.TrackBar seekbar;
        private System.Windows.Forms.Button btnFrameStep;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnGrabFrame;
        private System.Windows.Forms.Timer timer1;

    }
}