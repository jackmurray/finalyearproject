namespace SpeakerController
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
            this.txtFriendlyName = new System.Windows.Forms.TextBox();
            this.btnSaveFriendlyName = new System.Windows.Forms.Button();
            this.btnGetCert = new System.Windows.Forms.Button();
            this.lstDevices = new System.Windows.Forms.ListView();
            this.dGridTrace = new System.Windows.Forms.DataGridView();
            this.Severity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Source = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Message = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitter1 = new System.Windows.Forms.Splitter();
            ((System.ComponentModel.ISupportInitialize)(this.dGridTrace)).BeginInit();
            this.SuspendLayout();
            // 
            // btnDiscover
            // 
            this.btnDiscover.Location = new System.Drawing.Point(12, 196);
            this.btnDiscover.Name = "btnDiscover";
            this.btnDiscover.Size = new System.Drawing.Size(75, 23);
            this.btnDiscover.TabIndex = 0;
            this.btnDiscover.Text = "Discover";
            this.btnDiscover.UseVisualStyleBackColor = true;
            this.btnDiscover.Click += new System.EventHandler(this.btnDiscover_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 225);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Call";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtFriendlyName
            // 
            this.txtFriendlyName.Location = new System.Drawing.Point(720, 12);
            this.txtFriendlyName.Name = "txtFriendlyName";
            this.txtFriendlyName.Size = new System.Drawing.Size(100, 20);
            this.txtFriendlyName.TabIndex = 3;
            // 
            // btnSaveFriendlyName
            // 
            this.btnSaveFriendlyName.Location = new System.Drawing.Point(730, 39);
            this.btnSaveFriendlyName.Name = "btnSaveFriendlyName";
            this.btnSaveFriendlyName.Size = new System.Drawing.Size(75, 23);
            this.btnSaveFriendlyName.TabIndex = 4;
            this.btnSaveFriendlyName.Text = "Save";
            this.btnSaveFriendlyName.UseVisualStyleBackColor = true;
            this.btnSaveFriendlyName.Click += new System.EventHandler(this.btnSaveFriendlyName_Click);
            // 
            // btnGetCert
            // 
            this.btnGetCert.Location = new System.Drawing.Point(109, 225);
            this.btnGetCert.Name = "btnGetCert";
            this.btnGetCert.Size = new System.Drawing.Size(75, 23);
            this.btnGetCert.TabIndex = 6;
            this.btnGetCert.Text = "Get Cert";
            this.btnGetCert.UseVisualStyleBackColor = true;
            this.btnGetCert.Click += new System.EventHandler(this.btnGetCert_Click);
            // 
            // lstDevices
            // 
            this.lstDevices.Location = new System.Drawing.Point(12, 12);
            this.lstDevices.MultiSelect = false;
            this.lstDevices.Name = "lstDevices";
            this.lstDevices.Size = new System.Drawing.Size(205, 173);
            this.lstDevices.TabIndex = 7;
            this.lstDevices.UseCompatibleStateImageBehavior = false;
            this.lstDevices.View = System.Windows.Forms.View.List;
            // 
            // dGridTrace
            // 
            this.dGridTrace.AllowUserToAddRows = false;
            this.dGridTrace.AllowUserToDeleteRows = false;
            this.dGridTrace.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dGridTrace.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dGridTrace.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dGridTrace.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Severity,
            this.Source,
            this.ID,
            this.Message});
            this.dGridTrace.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dGridTrace.Location = new System.Drawing.Point(0, 342);
            this.dGridTrace.Name = "dGridTrace";
            this.dGridTrace.ReadOnly = true;
            this.dGridTrace.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
            this.dGridTrace.Size = new System.Drawing.Size(832, 122);
            this.dGridTrace.TabIndex = 8;
            // 
            // Severity
            // 
            this.Severity.HeaderText = "Severity";
            this.Severity.Name = "Severity";
            this.Severity.ReadOnly = true;
            this.Severity.Width = 70;
            // 
            // Source
            // 
            this.Source.HeaderText = "Source";
            this.Source.Name = "Source";
            this.Source.ReadOnly = true;
            this.Source.Width = 66;
            // 
            // ID
            // 
            this.ID.HeaderText = "ID";
            this.ID.Name = "ID";
            this.ID.ReadOnly = true;
            this.ID.Width = 43;
            // 
            // Message
            // 
            this.Message.HeaderText = "Message";
            this.Message.Name = "Message";
            this.Message.ReadOnly = true;
            this.Message.Width = 75;
            // 
            // splitter1
            // 
            this.splitter1.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Location = new System.Drawing.Point(0, 339);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(832, 3);
            this.splitter1.TabIndex = 9;
            this.splitter1.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(832, 464);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.dGridTrace);
            this.Controls.Add(this.lstDevices);
            this.Controls.Add(this.btnGetCert);
            this.Controls.Add(this.btnSaveFriendlyName);
            this.Controls.Add(this.txtFriendlyName);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnDiscover);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dGridTrace)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnDiscover;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtFriendlyName;
        private System.Windows.Forms.Button btnSaveFriendlyName;
        private System.Windows.Forms.Button btnGetCert;
        private System.Windows.Forms.ListView lstDevices;
        private System.Windows.Forms.DataGridView dGridTrace;
        private System.Windows.Forms.DataGridViewTextBoxColumn Severity;
        private System.Windows.Forms.DataGridViewTextBoxColumn Source;
        private System.Windows.Forms.DataGridViewTextBoxColumn ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Message;
        private System.Windows.Forms.Splitter splitter1;
    }
}

