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
            this.btnTrustDevice = new System.Windows.Forms.Button();
            this.lstDevicesAvail = new System.Windows.Forms.ListView();
            this.dGridTrace = new System.Windows.Forms.DataGridView();
            this.Severity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Source = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Message = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.btnListPairedDevices = new System.Windows.Forms.Button();
            this.btnOpenFile = new System.Windows.Forms.Button();
            this.txtGroupAddr = new System.Windows.Forms.TextBox();
            this.btnJoinGroup = new System.Windows.Forms.Button();
            this.btnStream = new System.Windows.Forms.Button();
            this.chkLogPause = new System.Windows.Forms.CheckBox();
            this.btnStreamTestSound = new System.Windows.Forms.Button();
            this.cmbLogLevel = new System.Windows.Forms.ComboBox();
            this.chkEnableEncrypt = new System.Windows.Forms.CheckBox();
            this.chkEnableAuth = new System.Windows.Forms.CheckBox();
            this.btnRotateKey = new System.Windows.Forms.Button();
            this.lstDevicesActive = new System.Windows.Forms.ListView();
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
            this.txtFriendlyName.Location = new System.Drawing.Point(956, 15);
            this.txtFriendlyName.Name = "txtFriendlyName";
            this.txtFriendlyName.Size = new System.Drawing.Size(100, 20);
            this.txtFriendlyName.TabIndex = 3;
            // 
            // btnSaveFriendlyName
            // 
            this.btnSaveFriendlyName.Location = new System.Drawing.Point(966, 42);
            this.btnSaveFriendlyName.Name = "btnSaveFriendlyName";
            this.btnSaveFriendlyName.Size = new System.Drawing.Size(75, 23);
            this.btnSaveFriendlyName.TabIndex = 4;
            this.btnSaveFriendlyName.Text = "Save";
            this.btnSaveFriendlyName.UseVisualStyleBackColor = true;
            this.btnSaveFriendlyName.Click += new System.EventHandler(this.btnSaveFriendlyName_Click);
            // 
            // btnTrustDevice
            // 
            this.btnTrustDevice.Location = new System.Drawing.Point(93, 196);
            this.btnTrustDevice.Name = "btnTrustDevice";
            this.btnTrustDevice.Size = new System.Drawing.Size(75, 23);
            this.btnTrustDevice.TabIndex = 6;
            this.btnTrustDevice.Text = "Pair";
            this.btnTrustDevice.UseVisualStyleBackColor = true;
            this.btnTrustDevice.Click += new System.EventHandler(this.btnGetCert_Click);
            // 
            // lstDevicesAvail
            // 
            this.lstDevicesAvail.Location = new System.Drawing.Point(12, 12);
            this.lstDevicesAvail.MultiSelect = false;
            this.lstDevicesAvail.Name = "lstDevicesAvail";
            this.lstDevicesAvail.Size = new System.Drawing.Size(205, 173);
            this.lstDevicesAvail.TabIndex = 7;
            this.lstDevicesAvail.UseCompatibleStateImageBehavior = false;
            this.lstDevicesAvail.View = System.Windows.Forms.View.List;
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
            this.dGridTrace.Location = new System.Drawing.Point(0, 338);
            this.dGridTrace.Name = "dGridTrace";
            this.dGridTrace.ReadOnly = true;
            this.dGridTrace.RowHeadersVisible = false;
            this.dGridTrace.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
            this.dGridTrace.ShowEditingIcon = false;
            this.dGridTrace.Size = new System.Drawing.Size(1068, 190);
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
            this.splitter1.Location = new System.Drawing.Point(0, 335);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(1068, 3);
            this.splitter1.TabIndex = 9;
            this.splitter1.TabStop = false;
            // 
            // btnListPairedDevices
            // 
            this.btnListPairedDevices.Location = new System.Drawing.Point(93, 225);
            this.btnListPairedDevices.Name = "btnListPairedDevices";
            this.btnListPairedDevices.Size = new System.Drawing.Size(109, 23);
            this.btnListPairedDevices.TabIndex = 10;
            this.btnListPairedDevices.Text = "List Paired Devices";
            this.btnListPairedDevices.UseVisualStyleBackColor = true;
            this.btnListPairedDevices.Click += new System.EventHandler(this.btnListPairedDevices_Click);
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.Location = new System.Drawing.Point(573, 54);
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(75, 23);
            this.btnOpenFile.TabIndex = 11;
            this.btnOpenFile.Text = "Open File";
            this.btnOpenFile.UseVisualStyleBackColor = true;
            this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
            // 
            // txtGroupAddr
            // 
            this.txtGroupAddr.Location = new System.Drawing.Point(941, 121);
            this.txtGroupAddr.Name = "txtGroupAddr";
            this.txtGroupAddr.Size = new System.Drawing.Size(100, 20);
            this.txtGroupAddr.TabIndex = 12;
            this.txtGroupAddr.Text = "224.1.1.1";
            // 
            // btnJoinGroup
            // 
            this.btnJoinGroup.Location = new System.Drawing.Point(941, 147);
            this.btnJoinGroup.Name = "btnJoinGroup";
            this.btnJoinGroup.Size = new System.Drawing.Size(75, 23);
            this.btnJoinGroup.TabIndex = 13;
            this.btnJoinGroup.Text = "Join Group";
            this.btnJoinGroup.UseVisualStyleBackColor = true;
            this.btnJoinGroup.Click += new System.EventHandler(this.btnJoinGroup_Click);
            // 
            // btnStream
            // 
            this.btnStream.Location = new System.Drawing.Point(573, 99);
            this.btnStream.Name = "btnStream";
            this.btnStream.Size = new System.Drawing.Size(75, 23);
            this.btnStream.TabIndex = 14;
            this.btnStream.Text = "Stream";
            this.btnStream.UseVisualStyleBackColor = true;
            this.btnStream.Click += new System.EventHandler(this.btnStream_Click);
            // 
            // chkLogPause
            // 
            this.chkLogPause.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkLogPause.AutoSize = true;
            this.chkLogPause.Location = new System.Drawing.Point(1033, 306);
            this.chkLogPause.Name = "chkLogPause";
            this.chkLogPause.Size = new System.Drawing.Size(23, 23);
            this.chkLogPause.TabIndex = 16;
            this.chkLogPause.Text = "II";
            this.chkLogPause.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkLogPause.UseVisualStyleBackColor = true;
            // 
            // btnStreamTestSound
            // 
            this.btnStreamTestSound.Location = new System.Drawing.Point(557, 128);
            this.btnStreamTestSound.Name = "btnStreamTestSound";
            this.btnStreamTestSound.Size = new System.Drawing.Size(109, 23);
            this.btnStreamTestSound.TabIndex = 17;
            this.btnStreamTestSound.Text = "Send Test Sound";
            this.btnStreamTestSound.UseVisualStyleBackColor = true;
            this.btnStreamTestSound.Click += new System.EventHandler(this.btnStreamTestSound_Click);
            // 
            // cmbLogLevel
            // 
            this.cmbLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLogLevel.FormattingEnabled = true;
            this.cmbLogLevel.Items.AddRange(new object[] {
            "Critical",
            "Error",
            "Warning",
            "Information",
            "Verbose"});
            this.cmbLogLevel.Location = new System.Drawing.Point(812, 15);
            this.cmbLogLevel.Name = "cmbLogLevel";
            this.cmbLogLevel.Size = new System.Drawing.Size(121, 21);
            this.cmbLogLevel.TabIndex = 18;
            this.cmbLogLevel.SelectedIndexChanged += new System.EventHandler(this.cmbLogLevel_SelectedIndexChanged);
            // 
            // chkEnableEncrypt
            // 
            this.chkEnableEncrypt.AutoSize = true;
            this.chkEnableEncrypt.Location = new System.Drawing.Point(812, 43);
            this.chkEnableEncrypt.Name = "chkEnableEncrypt";
            this.chkEnableEncrypt.Size = new System.Drawing.Size(112, 17);
            this.chkEnableEncrypt.TabIndex = 19;
            this.chkEnableEncrypt.Text = "Enable Encryption";
            this.chkEnableEncrypt.UseVisualStyleBackColor = true;
            this.chkEnableEncrypt.CheckedChanged += new System.EventHandler(this.chkEnableEncrypt_CheckedChanged);
            // 
            // chkEnableAuth
            // 
            this.chkEnableAuth.AutoSize = true;
            this.chkEnableAuth.Location = new System.Drawing.Point(812, 67);
            this.chkEnableAuth.Name = "chkEnableAuth";
            this.chkEnableAuth.Size = new System.Drawing.Size(130, 17);
            this.chkEnableAuth.TabIndex = 20;
            this.chkEnableAuth.Text = "Enable Authentication";
            this.chkEnableAuth.UseVisualStyleBackColor = true;
            this.chkEnableAuth.CheckedChanged += new System.EventHandler(this.chkEnableAuth_CheckedChanged);
            // 
            // btnRotateKey
            // 
            this.btnRotateKey.Location = new System.Drawing.Point(792, 306);
            this.btnRotateKey.Name = "btnRotateKey";
            this.btnRotateKey.Size = new System.Drawing.Size(75, 23);
            this.btnRotateKey.TabIndex = 21;
            this.btnRotateKey.Text = "Rotate Key";
            this.btnRotateKey.UseVisualStyleBackColor = true;
            this.btnRotateKey.Click += new System.EventHandler(this.btnRotateKey_Click);
            // 
            // lstDevicesActive
            // 
            this.lstDevicesActive.Location = new System.Drawing.Point(223, 12);
            this.lstDevicesActive.MultiSelect = false;
            this.lstDevicesActive.Name = "lstDevicesActive";
            this.lstDevicesActive.Size = new System.Drawing.Size(205, 173);
            this.lstDevicesActive.TabIndex = 22;
            this.lstDevicesActive.UseCompatibleStateImageBehavior = false;
            this.lstDevicesActive.View = System.Windows.Forms.View.List;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1068, 528);
            this.Controls.Add(this.lstDevicesActive);
            this.Controls.Add(this.btnRotateKey);
            this.Controls.Add(this.chkEnableAuth);
            this.Controls.Add(this.chkEnableEncrypt);
            this.Controls.Add(this.cmbLogLevel);
            this.Controls.Add(this.btnStreamTestSound);
            this.Controls.Add(this.chkLogPause);
            this.Controls.Add(this.btnStream);
            this.Controls.Add(this.btnJoinGroup);
            this.Controls.Add(this.txtGroupAddr);
            this.Controls.Add(this.btnOpenFile);
            this.Controls.Add(this.btnListPairedDevices);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.dGridTrace);
            this.Controls.Add(this.lstDevicesAvail);
            this.Controls.Add(this.btnTrustDevice);
            this.Controls.Add(this.btnSaveFriendlyName);
            this.Controls.Add(this.txtFriendlyName);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnDiscover);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
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
        private System.Windows.Forms.Button btnTrustDevice;
        private System.Windows.Forms.ListView lstDevicesAvail;
        private System.Windows.Forms.DataGridView dGridTrace;
        private System.Windows.Forms.DataGridViewTextBoxColumn Severity;
        private System.Windows.Forms.DataGridViewTextBoxColumn Source;
        private System.Windows.Forms.DataGridViewTextBoxColumn ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Message;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Button btnListPairedDevices;
        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.TextBox txtGroupAddr;
        private System.Windows.Forms.Button btnJoinGroup;
        private System.Windows.Forms.Button btnStream;
        private System.Windows.Forms.CheckBox chkLogPause;
        private System.Windows.Forms.Button btnStreamTestSound;
        private System.Windows.Forms.ComboBox cmbLogLevel;
        private System.Windows.Forms.CheckBox chkEnableEncrypt;
        private System.Windows.Forms.CheckBox chkEnableAuth;
        private System.Windows.Forms.Button btnRotateKey;
        private System.Windows.Forms.ListView lstDevicesActive;
    }
}

