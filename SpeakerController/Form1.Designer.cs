﻿namespace SpeakerController
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
            this.btnDiscover = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.lstDevices = new System.Windows.Forms.ListBox();
            this.txtFriendlyName = new System.Windows.Forms.TextBox();
            this.btnSaveFriendlyName = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnDiscover
            // 
            this.btnDiscover.Location = new System.Drawing.Point(93, 198);
            this.btnDiscover.Name = "btnDiscover";
            this.btnDiscover.Size = new System.Drawing.Size(75, 23);
            this.btnDiscover.TabIndex = 0;
            this.btnDiscover.Text = "Discover";
            this.btnDiscover.UseVisualStyleBackColor = true;
            this.btnDiscover.Click += new System.EventHandler(this.btnDiscover_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(93, 227);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lstDevices
            // 
            this.lstDevices.FormattingEnabled = true;
            this.lstDevices.Location = new System.Drawing.Point(12, 12);
            this.lstDevices.Name = "lstDevices";
            this.lstDevices.Size = new System.Drawing.Size(120, 95);
            this.lstDevices.TabIndex = 2;
            // 
            // txtFriendlyName
            // 
            this.txtFriendlyName.Location = new System.Drawing.Point(161, 13);
            this.txtFriendlyName.Name = "txtFriendlyName";
            this.txtFriendlyName.Size = new System.Drawing.Size(100, 20);
            this.txtFriendlyName.TabIndex = 3;
            // 
            // btnSaveFriendlyName
            // 
            this.btnSaveFriendlyName.Location = new System.Drawing.Point(171, 40);
            this.btnSaveFriendlyName.Name = "btnSaveFriendlyName";
            this.btnSaveFriendlyName.Size = new System.Drawing.Size(75, 23);
            this.btnSaveFriendlyName.TabIndex = 4;
            this.btnSaveFriendlyName.Text = "Save";
            this.btnSaveFriendlyName.UseVisualStyleBackColor = true;
            this.btnSaveFriendlyName.Click += new System.EventHandler(this.btnSaveFriendlyName_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.btnSaveFriendlyName);
            this.Controls.Add(this.txtFriendlyName);
            this.Controls.Add(this.lstDevices);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnDiscover);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnDiscover;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListBox lstDevices;
        private System.Windows.Forms.TextBox txtFriendlyName;
        private System.Windows.Forms.Button btnSaveFriendlyName;
    }
}

