namespace Carebed.UI
{
    partial class SingleAlertPopup: Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public SingleAlertPopup(string details, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            InitializeComponent();

            // Set up icon
            PictureBox iconBox = new PictureBox
            {
                Size = new Size(48, 48),
                Location = new Point(16, 16),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            switch (icon)
            {
                case MessageBoxIcon.Information:
                    iconBox.Image = SystemIcons.Information.ToBitmap();
                    break;
                case MessageBoxIcon.Warning:
                    iconBox.Image = SystemIcons.Warning.ToBitmap();
                    break;
                case MessageBoxIcon.Error:
                    iconBox.Image = SystemIcons.Error.ToBitmap();
                    break;
                case MessageBoxIcon.Question:
                    iconBox.Image = SystemIcons.Question.ToBitmap();
                    break;
                default:
                    iconBox.Image = null;
                    break;
            }
            Controls.Add(iconBox);

            // Set up label
            Label detailsLabel = new Label
            {
                Text = details,
                Location = new Point(80, 16),
                AutoSize = true,
                MaximumSize = new Size(400, 0)
            };
            Controls.Add(detailsLabel);

            // Force layout so detailsLabel.Bottom is correct
            detailsLabel.PerformLayout();
            this.PerformLayout();

            // Set up buttons
            int buttonY = detailsLabel.Bottom + 40;
            int buttonX = 80;
            int buttonHeight = 0;
            if (buttons == MessageBoxButtons.OKCancel)
            {
                Button okButton = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(buttonX, buttonY), Size = new Size(90, 32) };
                Button cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(buttonX + 100, buttonY), Size = new Size(90, 32) };
                Controls.Add(okButton);
                Controls.Add(cancelButton);
                AcceptButton = okButton;
                CancelButton = cancelButton;
                buttonHeight = okButton.Height;
            }
            else if (buttons == MessageBoxButtons.OK)
            {
                Button okButton = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(buttonX, buttonY) };
                Controls.Add(okButton);
                AcceptButton = okButton;
                buttonHeight = okButton.Height;
            }

            // Set form properties
            
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            int bottomPadding = 32;
            this.ClientSize = new Size(
                Math.Max(320, detailsLabel.Right + 24),
                buttonY + buttonHeight + bottomPadding
            );
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.ShowInTaskbar = false;
        }

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
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "SingleAlertPopup";
        }

        #endregion
    }
}