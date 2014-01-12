namespace RunInTray
{
	partial class LogForm
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
					components = null;
				}
				if (disposable != null)
				{
					disposable.Dispose();
					disposable = null;
				}
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
			this.logTextBox = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// logTextBox
			// 
			this.logTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.logTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.logTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.logTextBox.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.logTextBox.Location = new System.Drawing.Point(0, 0);
			this.logTextBox.Name = "logTextBox";
			this.logTextBox.ReadOnly = true;
			this.logTextBox.Size = new System.Drawing.Size(284, 262);
			this.logTextBox.TabIndex = 0;
			this.logTextBox.Text = "";
			this.logTextBox.WordWrap = false;
			// 
			// LogForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Controls.Add(this.logTextBox);
			this.Name = "LogForm";
			this.Text = "LogForm";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox logTextBox;

	}
}