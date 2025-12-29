namespace MCModInstaller.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.headerLabel = new System.Windows.Forms.Label();
            this.instructionsGroupBox = new System.Windows.Forms.GroupBox();
            this.instructionsLabel = new System.Windows.Forms.Label();
            this.pathLabel = new System.Windows.Forms.Label();
            this.pathTextBox = new System.Windows.Forms.TextBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.statusLabel = new System.Windows.Forms.Label();
            this.installButton = new System.Windows.Forms.Button();
            this.instructionsGroupBox.SuspendLayout();
            this.SuspendLayout();
            //
            // headerLabel
            //
            this.headerLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.headerLabel.Location = new System.Drawing.Point(12, 15);
            this.headerLabel.Name = "headerLabel";
            this.headerLabel.Size = new System.Drawing.Size(476, 25);
            this.headerLabel.TabIndex = 0;
            this.headerLabel.Text = "Installation de MCPlantator Mod";
            this.headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // instructionsGroupBox
            //
            this.instructionsGroupBox.Controls.Add(this.instructionsLabel);
            this.instructionsGroupBox.Location = new System.Drawing.Point(12, 50);
            this.instructionsGroupBox.Name = "instructionsGroupBox";
            this.instructionsGroupBox.Size = new System.Drawing.Size(476, 110);
            this.instructionsGroupBox.TabIndex = 1;
            this.instructionsGroupBox.TabStop = false;
            this.instructionsGroupBox.Text = "Comment trouver le chemin d\'accès";
            //
            // instructionsLabel
            //
            this.instructionsLabel.Location = new System.Drawing.Point(10, 20);
            this.instructionsLabel.Name = "instructionsLabel";
            this.instructionsLabel.Size = new System.Drawing.Size(456, 80);
            this.instructionsLabel.TabIndex = 0;
            this.instructionsLabel.Text = "1. Ouvrir CurseForge\r\n2. Clic droit sur le modpack\r\n3. Sélectionner \"Open folder\"" +
                "\r\n4. Copier le chemin d\'accès depuis l\'explorateur Windows\r\n5. Coller le chemin" +
                " ci-dessous";
            //
            // pathLabel
            //
            this.pathLabel.AutoSize = true;
            this.pathLabel.Location = new System.Drawing.Point(12, 173);
            this.pathLabel.Name = "pathLabel";
            this.pathLabel.Size = new System.Drawing.Size(195, 15);
            this.pathLabel.TabIndex = 2;
            this.pathLabel.Text = "Chemin de l\'instance Minecraft :";
            //
            // pathTextBox
            //
            this.pathTextBox.Location = new System.Drawing.Point(12, 191);
            this.pathTextBox.Name = "pathTextBox";
            this.pathTextBox.Size = new System.Drawing.Size(385, 23);
            this.pathTextBox.TabIndex = 3;
            //
            // browseButton
            //
            this.browseButton.Location = new System.Drawing.Point(403, 190);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(85, 25);
            this.browseButton.TabIndex = 4;
            this.browseButton.Text = "Parcourir...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            //
            // progressBar
            //
            this.progressBar.Location = new System.Drawing.Point(12, 227);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(476, 23);
            this.progressBar.TabIndex = 5;
            this.progressBar.Visible = false;
            //
            // statusLabel
            //
            this.statusLabel.Location = new System.Drawing.Point(12, 253);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(476, 15);
            this.statusLabel.TabIndex = 6;
            this.statusLabel.Text = "Prêt à installer";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.statusLabel.Visible = false;
            //
            // installButton
            //
            this.installButton.Location = new System.Drawing.Point(200, 275);
            this.installButton.Name = "installButton";
            this.installButton.Size = new System.Drawing.Size(100, 30);
            this.installButton.TabIndex = 7;
            this.installButton.Text = "Installer";
            this.installButton.UseVisualStyleBackColor = true;
            this.installButton.Click += new System.EventHandler(this.InstallButton_Click);
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 320);
            this.Controls.Add(this.installButton);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.pathTextBox);
            this.Controls.Add(this.pathLabel);
            this.Controls.Add(this.instructionsGroupBox);
            this.Controls.Add(this.headerLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MCModInstaller - Installation de Mod";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.instructionsGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label headerLabel;
        private System.Windows.Forms.GroupBox instructionsGroupBox;
        private System.Windows.Forms.Label instructionsLabel;
        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.TextBox pathTextBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Button installButton;
    }
}
