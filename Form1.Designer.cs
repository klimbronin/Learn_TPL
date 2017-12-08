namespace WindowsFormsApplication2
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
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.btnClear = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.btnThread = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.btnThread1 = new System.Windows.Forms.Button();
			this.btnThread2 = new System.Windows.Forms.Button();
			this.btnThread3 = new System.Windows.Forms.Button();
			this.txtBgw = new System.Windows.Forms.TextBox();
			this.bgwStop = new System.Windows.Forms.Button();
			this.bgwRun = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.btmTask_TCS = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.btnTCS = new System.Windows.Forms.Button();
			this.btnChkTCS = new System.Windows.Forms.Button();
			this.label6 = new System.Windows.Forms.Label();
			this.btnSyncCont = new System.Windows.Forms.Button();
			this.label7 = new System.Windows.Forms.Label();
			this.btnInvStart = new System.Windows.Forms.Button();
			this.btnInvChk = new System.Windows.Forms.Button();
			this.btnInvStop = new System.Windows.Forms.Button();
			this.cmbLocks = new System.Windows.Forms.ComboBox();
			this.btnLocksRun = new System.Windows.Forms.Button();
			this.label9 = new System.Windows.Forms.Label();
			this.btnMutex = new System.Windows.Forms.Button();
			this.btnAsyncStart1 = new System.Windows.Forms.Button();
			this.label8 = new System.Windows.Forms.Label();
			this.btnAsyncLock = new System.Windows.Forms.Button();
			this.btnAsyncStart2 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox1.Location = new System.Drawing.Point(180, 0);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBox1.Size = new System.Drawing.Size(757, 778);
			this.textBox1.TabIndex = 0;
			this.textBox1.WordWrap = false;
			// 
			// btnClear
			// 
			this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClear.Location = new System.Drawing.Point(866, 4);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(50, 23);
			this.btnClear.TabIndex = 6;
			this.btnClear.Text = "Clear";
			this.btnClear.UseVisualStyleBackColor = true;
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(23, 424);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(129, 13);
			this.label1.TabIndex = 8;
			this.label1.Text = "Внутренние блокировки";
			// 
			// btnThread
			// 
			this.btnThread.Location = new System.Drawing.Point(26, 16);
			this.btnThread.Name = "btnThread";
			this.btnThread.Size = new System.Drawing.Size(116, 23);
			this.btnThread.TabIndex = 9;
			this.btnThread.Text = "thread and vars";
			this.btnThread.UseVisualStyleBackColor = true;
			this.btnThread.Click += new System.EventHandler(this.btnThread_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(22, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(120, 13);
			this.label2.TabIndex = 10;
			this.label2.Text = "Потоки и переменные";
			// 
			// btnThread1
			// 
			this.btnThread1.Location = new System.Drawing.Point(1, 40);
			this.btnThread1.Name = "btnThread1";
			this.btnThread1.Size = new System.Drawing.Size(53, 23);
			this.btnThread1.TabIndex = 11;
			this.btnThread1.Text = "вар-т 1";
			this.btnThread1.UseVisualStyleBackColor = true;
			this.btnThread1.Click += new System.EventHandler(this.btnThread_Click);
			// 
			// btnThread2
			// 
			this.btnThread2.Location = new System.Drawing.Point(60, 40);
			this.btnThread2.Name = "btnThread2";
			this.btnThread2.Size = new System.Drawing.Size(58, 23);
			this.btnThread2.TabIndex = 12;
			this.btnThread2.Text = "вар-т 2";
			this.btnThread2.UseVisualStyleBackColor = true;
			this.btnThread2.Click += new System.EventHandler(this.btnThread_Click);
			// 
			// btnThread3
			// 
			this.btnThread3.Location = new System.Drawing.Point(124, 40);
			this.btnThread3.Name = "btnThread3";
			this.btnThread3.Size = new System.Drawing.Size(51, 23);
			this.btnThread3.TabIndex = 13;
			this.btnThread3.Text = "вар-т 3";
			this.btnThread3.UseVisualStyleBackColor = true;
			this.btnThread3.Click += new System.EventHandler(this.btnThread_Click);
			// 
			// txtBgw
			// 
			this.txtBgw.Location = new System.Drawing.Point(140, 72);
			this.txtBgw.Name = "txtBgw";
			this.txtBgw.Size = new System.Drawing.Size(35, 20);
			this.txtBgw.TabIndex = 18;
			this.txtBgw.Text = "5";
			// 
			// bgwStop
			// 
			this.bgwStop.Location = new System.Drawing.Point(1, 120);
			this.bgwStop.Name = "bgwStop";
			this.bgwStop.Size = new System.Drawing.Size(174, 23);
			this.bgwStop.TabIndex = 17;
			this.bgwStop.Text = "Остановить";
			this.bgwStop.UseVisualStyleBackColor = true;
			this.bgwStop.Click += new System.EventHandler(this.bgwStop_Click);
			// 
			// bgwRun
			// 
			this.bgwRun.Location = new System.Drawing.Point(1, 95);
			this.bgwRun.Name = "bgwRun";
			this.bgwRun.Size = new System.Drawing.Size(174, 23);
			this.bgwRun.TabIndex = 16;
			this.bgwRun.Text = "Поехали";
			this.bgwRun.UseVisualStyleBackColor = true;
			this.bgwRun.Click += new System.EventHandler(this.bgwRun_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(-2, 75);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(145, 13);
			this.label3.TabIndex = 15;
			this.label3.Text = "BackGroundWorker. Кол-во:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(-2, 151);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(184, 13);
			this.label4.TabIndex = 19;
			this.label4.Text = "Связка Task-TaskCompletionSource";
			// 
			// btmTask_TCS
			// 
			this.btmTask_TCS.Location = new System.Drawing.Point(1, 167);
			this.btmTask_TCS.Name = "btmTask_TCS";
			this.btmTask_TCS.Size = new System.Drawing.Size(174, 23);
			this.btmTask_TCS.TabIndex = 20;
			this.btmTask_TCS.Text = "Поехали";
			this.btmTask_TCS.UseVisualStyleBackColor = true;
			this.btmTask_TCS.Click += new System.EventHandler(this.btnTask_TCS_Click);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(34, 199);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(117, 13);
			this.label5.TabIndex = 21;
			this.label5.Text = "TaskCompletionSource";
			// 
			// btnTCS
			// 
			this.btnTCS.Location = new System.Drawing.Point(1, 215);
			this.btnTCS.Name = "btnTCS";
			this.btnTCS.Size = new System.Drawing.Size(173, 23);
			this.btnTCS.TabIndex = 22;
			this.btnTCS.Text = "Поехали";
			this.btnTCS.UseVisualStyleBackColor = true;
			this.btnTCS.Click += new System.EventHandler(this.btnTCS_Click);
			// 
			// btnChkTCS
			// 
			this.btnChkTCS.Location = new System.Drawing.Point(1, 240);
			this.btnChkTCS.Name = "btnChkTCS";
			this.btnChkTCS.Size = new System.Drawing.Size(174, 23);
			this.btnChkTCS.TabIndex = 23;
			this.btnChkTCS.Text = "Проверить что делается";
			this.btnChkTCS.UseVisualStyleBackColor = true;
			this.btnChkTCS.Click += new System.EventHandler(this.btnChkTCS_Click);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(30, 273);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(112, 13);
			this.label6.TabIndex = 24;
			this.label6.Text = "SyncronizationContext";
			// 
			// btnSyncCont
			// 
			this.btnSyncCont.Location = new System.Drawing.Point(1, 289);
			this.btnSyncCont.Name = "btnSyncCont";
			this.btnSyncCont.Size = new System.Drawing.Size(174, 23);
			this.btnSyncCont.TabIndex = 25;
			this.btnSyncCont.Text = "Поехали";
			this.btnSyncCont.UseVisualStyleBackColor = true;
			this.btnSyncCont.Click += new System.EventHandler(this.btnSyncCont_Click);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(23, 324);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(128, 13);
			this.label7.TabIndex = 26;
			this.label7.Text = "BeginInvoke - EndInvoke";
			// 
			// btnInvStart
			// 
			this.btnInvStart.Location = new System.Drawing.Point(1, 340);
			this.btnInvStart.Name = "btnInvStart";
			this.btnInvStart.Size = new System.Drawing.Size(173, 23);
			this.btnInvStart.TabIndex = 27;
			this.btnInvStart.Text = "Поехали";
			this.btnInvStart.UseVisualStyleBackColor = true;
			this.btnInvStart.Click += new System.EventHandler(this.btnInvStart_Click);
			// 
			// btnInvChk
			// 
			this.btnInvChk.Location = new System.Drawing.Point(1, 365);
			this.btnInvChk.Name = "btnInvChk";
			this.btnInvChk.Size = new System.Drawing.Size(173, 23);
			this.btnInvChk.TabIndex = 28;
			this.btnInvChk.Text = "Проверить что делается";
			this.btnInvChk.UseVisualStyleBackColor = true;
			this.btnInvChk.Click += new System.EventHandler(this.btnInvChk_Click);
			// 
			// btnInvStop
			// 
			this.btnInvStop.Location = new System.Drawing.Point(1, 389);
			this.btnInvStop.Name = "btnInvStop";
			this.btnInvStop.Size = new System.Drawing.Size(173, 23);
			this.btnInvStop.TabIndex = 29;
			this.btnInvStop.Text = "Остановить";
			this.btnInvStop.UseVisualStyleBackColor = true;
			this.btnInvStop.Click += new System.EventHandler(this.btnInvStop_Click);
			// 
			// cmbLocks
			// 
			this.cmbLocks.FormattingEnabled = true;
			this.cmbLocks.Items.AddRange(new object[] {
            "зачем нужны блокировки",
            "lock(this) --",
            "lock(typeOf(...)) --",
            "lock(\"строка\") --",
            "lock +",
            "Monitor Enter/Exit",
            "deadlock example",
            "ManualResetEvent 1",
            "ManualResetEvent 2",
            "Monitor Wait/Pulse",
            "SplinLock"});
			this.cmbLocks.Location = new System.Drawing.Point(2, 440);
			this.cmbLocks.MaxDropDownItems = 12;
			this.cmbLocks.Name = "cmbLocks";
			this.cmbLocks.Size = new System.Drawing.Size(130, 21);
			this.cmbLocks.TabIndex = 33;
			// 
			// btnLocksRun
			// 
			this.btnLocksRun.Location = new System.Drawing.Point(138, 438);
			this.btnLocksRun.Name = "btnLocksRun";
			this.btnLocksRun.Size = new System.Drawing.Size(37, 23);
			this.btnLocksRun.TabIndex = 34;
			this.btnLocksRun.Text = "Run";
			this.btnLocksRun.UseVisualStyleBackColor = true;
			this.btnLocksRun.Click += new System.EventHandler(this.btnLocksRun_Click);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(3, 474);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(169, 13);
			this.label9.TabIndex = 35;
			this.label9.Text = "Блокировки между процессами";
			// 
			// btnMutex
			// 
			this.btnMutex.Location = new System.Drawing.Point(6, 490);
			this.btnMutex.Name = "btnMutex";
			this.btnMutex.Size = new System.Drawing.Size(166, 23);
			this.btnMutex.TabIndex = 36;
			this.btnMutex.Text = "Mutex";
			this.btnMutex.UseVisualStyleBackColor = true;
			this.btnMutex.Click += new System.EventHandler(this.btnMutex_Click);
			// 
			// btnAsyncStart1
			// 
			this.btnAsyncStart1.Location = new System.Drawing.Point(4, 544);
			this.btnAsyncStart1.Name = "btnAsyncStart1";
			this.btnAsyncStart1.Size = new System.Drawing.Size(86, 23);
			this.btnAsyncStart1.TabIndex = 38;
			this.btnAsyncStart1.Text = "Поехали";
			this.btnAsyncStart1.UseVisualStyleBackColor = true;
			this.btnAsyncStart1.Click += new System.EventHandler(this.btnAsyncStart1_Click);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(49, 528);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(69, 13);
			this.label8.TabIndex = 37;
			this.label8.Text = "async - await";
			// 
			// btnAsyncLock
			// 
			this.btnAsyncLock.Location = new System.Drawing.Point(4, 573);
			this.btnAsyncLock.Name = "btnAsyncLock";
			this.btnAsyncLock.Size = new System.Drawing.Size(173, 23);
			this.btnAsyncLock.TabIndex = 39;
			this.btnAsyncLock.Text = "пример блокировки";
			this.btnAsyncLock.UseVisualStyleBackColor = true;
			this.btnAsyncLock.Click += new System.EventHandler(this.btnAsyncLock_Click);
			// 
			// btnAsyncStart2
			// 
			this.btnAsyncStart2.Location = new System.Drawing.Point(96, 544);
			this.btnAsyncStart2.Name = "btnAsyncStart2";
			this.btnAsyncStart2.Size = new System.Drawing.Size(81, 23);
			this.btnAsyncStart2.TabIndex = 40;
			this.btnAsyncStart2.Text = "Поехали";
			this.btnAsyncStart2.UseVisualStyleBackColor = true;
			this.btnAsyncStart2.Click += new System.EventHandler(this.btnAsyncStart2_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(937, 778);
			this.Controls.Add(this.btnAsyncStart2);
			this.Controls.Add(this.btnAsyncLock);
			this.Controls.Add(this.btnAsyncStart1);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.btnMutex);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.btnLocksRun);
			this.Controls.Add(this.cmbLocks);
			this.Controls.Add(this.btnInvStop);
			this.Controls.Add(this.btnInvChk);
			this.Controls.Add(this.btnInvStart);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.btnSyncCont);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.btnChkTCS);
			this.Controls.Add(this.btnTCS);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.btmTask_TCS);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.txtBgw);
			this.Controls.Add(this.bgwStop);
			this.Controls.Add(this.bgwRun);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.btnThread3);
			this.Controls.Add(this.btnThread2);
			this.Controls.Add(this.btnThread1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btnThread);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.textBox1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button btnClear;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnThread;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnThread1;
		private System.Windows.Forms.Button btnThread2;
		private System.Windows.Forms.Button btnThread3;
		private System.Windows.Forms.TextBox txtBgw;
		private System.Windows.Forms.Button bgwStop;
		private System.Windows.Forms.Button bgwRun;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button btmTask_TCS;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button btnTCS;
		private System.Windows.Forms.Button btnChkTCS;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button btnSyncCont;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button btnInvStart;
		private System.Windows.Forms.Button btnInvChk;
		private System.Windows.Forms.Button btnInvStop;
		private System.Windows.Forms.ComboBox cmbLocks;
		private System.Windows.Forms.Button btnLocksRun;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Button btnMutex;
		private System.Windows.Forms.Button btnAsyncStart1;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button btnAsyncLock;
		private System.Windows.Forms.Button btnAsyncStart2;
	}
}

