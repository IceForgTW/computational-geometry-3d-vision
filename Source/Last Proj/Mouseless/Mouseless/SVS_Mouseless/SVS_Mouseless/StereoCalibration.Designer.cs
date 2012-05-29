namespace SVS_Mouseless
{
    partial class StereoCalibration
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
            this.btnStart = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tbNoS = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbCellSize = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbCBCol = new System.Windows.Forms.TextBox();
            this.tbCBRow = new System.Windows.Forms.TextBox();
            this.tbFrameHeight = new System.Windows.Forms.TextBox();
            this.tbFrameWidth = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbConfigFile = new System.Windows.Forms.TextBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(268, 136);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tbNoS);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.tbCellSize);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.tbCBCol);
            this.groupBox1.Controls.Add(this.tbCBRow);
            this.groupBox1.Controls.Add(this.tbFrameHeight);
            this.groupBox1.Controls.Add(this.tbFrameWidth);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 9);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(330, 94);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Calibration Info";
            // 
            // tbNoS
            // 
            this.tbNoS.Location = new System.Drawing.Point(268, 66);
            this.tbNoS.Name = "tbNoS";
            this.tbNoS.Size = new System.Drawing.Size(56, 20);
            this.tbNoS.TabIndex = 11;
            this.tbNoS.Text = "30";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(162, 69);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(97, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Number of samples";
            // 
            // tbCellSize
            // 
            this.tbCellSize.Location = new System.Drawing.Point(92, 66);
            this.tbCellSize.Name = "tbCellSize";
            this.tbCellSize.Size = new System.Drawing.Size(55, 20);
            this.tbCellSize.TabIndex = 9;
            this.tbCellSize.Text = "24.5";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 69);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Cell size (mm)";
            // 
            // tbCBCol
            // 
            this.tbCBCol.Location = new System.Drawing.Point(268, 42);
            this.tbCBCol.Name = "tbCBCol";
            this.tbCBCol.Size = new System.Drawing.Size(56, 20);
            this.tbCBCol.TabIndex = 7;
            this.tbCBCol.Text = "9";
            // 
            // tbCBRow
            // 
            this.tbCBRow.Location = new System.Drawing.Point(268, 16);
            this.tbCBRow.Name = "tbCBRow";
            this.tbCBRow.Size = new System.Drawing.Size(56, 20);
            this.tbCBRow.TabIndex = 6;
            this.tbCBRow.Text = "6";
            // 
            // tbFrameHeight
            // 
            this.tbFrameHeight.Location = new System.Drawing.Point(92, 42);
            this.tbFrameHeight.Name = "tbFrameHeight";
            this.tbFrameHeight.Size = new System.Drawing.Size(55, 20);
            this.tbFrameHeight.TabIndex = 5;
            this.tbFrameHeight.Text = "240";
            // 
            // tbFrameWidth
            // 
            this.tbFrameWidth.Location = new System.Drawing.Point(92, 16);
            this.tbFrameWidth.Name = "tbFrameWidth";
            this.tbFrameWidth.Size = new System.Drawing.Size(55, 20);
            this.tbFrameWidth.TabIndex = 4;
            this.tbFrameWidth.Text = "320";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(162, 45);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(81, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Chessboard Col";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(162, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Chessboard Row";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Frame Height";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Frame Width";
            // 
            // tbConfigFile
            // 
            this.tbConfigFile.Location = new System.Drawing.Point(71, 109);
            this.tbConfigFile.Name = "tbConfigFile";
            this.tbConfigFile.Size = new System.Drawing.Size(191, 20);
            this.tbConfigFile.TabIndex = 2;
            this.tbConfigFile.Text = "Cameras.xml";
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(268, 107);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(75, 23);
            this.btnLoad.TabIndex = 3;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 112);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Config file";
            // 
            // StereoCalibration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(354, 164);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.tbConfigFile);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnStart);
            this.Name = "StereoCalibration";
            this.Text = "StereoCalibration";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbFrameHeight;
        private System.Windows.Forms.TextBox tbFrameWidth;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbCBCol;
        private System.Windows.Forms.TextBox tbCBRow;
        private System.Windows.Forms.TextBox tbCellSize;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbNoS;
        private System.Windows.Forms.TextBox tbConfigFile;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Label label7;
    }
}