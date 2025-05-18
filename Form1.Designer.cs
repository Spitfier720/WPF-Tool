namespace Windows_Forms_Project
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button buttonOpenForm2;
        private System.Windows.Forms.Label labelDisplayInput;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonOpenForm2 = new System.Windows.Forms.Button();
            this.labelDisplayInput = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonOpenForm2
            // 
            this.buttonOpenForm2.Location = new System.Drawing.Point(50, 50);
            this.buttonOpenForm2.Name = "buttonOpenForm2";
            this.buttonOpenForm2.Size = new System.Drawing.Size(150, 30);
            this.buttonOpenForm2.TabIndex = 0;
            this.buttonOpenForm2.Text = "Open Form2";
            this.buttonOpenForm2.UseVisualStyleBackColor = true;
            this.buttonOpenForm2.Click += new System.EventHandler(this.buttonOpenForm2_Click);
            // 
            // labelDisplayInput
            // 
            this.labelDisplayInput.AutoSize = true;
            this.labelDisplayInput.Location = new System.Drawing.Point(50, 100);
            this.labelDisplayInput.Name = "labelDisplayInput";
            this.labelDisplayInput.Size = new System.Drawing.Size(100, 20);
            this.labelDisplayInput.TabIndex = 1;
            this.labelDisplayInput.Text = "User Input:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 200);
            this.Controls.Add(this.labelDisplayInput);
            this.Controls.Add(this.buttonOpenForm2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
