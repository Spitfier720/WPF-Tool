namespace Windows_Forms_Project
{
    partial class Form2
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox textBoxInput1;
        private System.Windows.Forms.TextBox textBoxInput2;
        private System.Windows.Forms.TextBox textBoxInput3;
        private System.Windows.Forms.Button buttonSubmit;
        private System.Windows.Forms.Label labelInput1;
        private System.Windows.Forms.Label labelInput2;
        private System.Windows.Forms.Label labelInput3;

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

        private void InitializeComponent()
        {
            this.textBoxInput1 = new System.Windows.Forms.TextBox();
            this.textBoxInput2 = new System.Windows.Forms.TextBox();
            this.textBoxInput3 = new System.Windows.Forms.TextBox();
            this.buttonSubmit = new System.Windows.Forms.Button();
            this.labelInput1 = new System.Windows.Forms.Label();
            this.labelInput2 = new System.Windows.Forms.Label();
            this.labelInput3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelInput1
            // 
            this.labelInput1.AutoSize = true;
            this.labelInput1.Location = new System.Drawing.Point(30, 30);
            this.labelInput1.Name = "labelInput1";
            this.labelInput1.Size = new System.Drawing.Size(60, 20);
            this.labelInput1.TabIndex = 0;
            this.labelInput1.Text = "Input 1:";
            // 
            // textBoxInput1
            // 
            this.textBoxInput1.Location = new System.Drawing.Point(100, 30);
            this.textBoxInput1.Name = "textBoxInput1";
            this.textBoxInput1.Size = new System.Drawing.Size(200, 27);
            this.textBoxInput1.TabIndex = 1;
            // 
            // labelInput2
            // 
            this.labelInput2.AutoSize = true;
            this.labelInput2.Location = new System.Drawing.Point(30, 70);
            this.labelInput2.Name = "labelInput2";
            this.labelInput2.Size = new System.Drawing.Size(60, 20);
            this.labelInput2.TabIndex = 2;
            this.labelInput2.Text = "Input 2:";
            // 
            // textBoxInput2
            // 
            this.textBoxInput2.Location = new System.Drawing.Point(100, 70);
            this.textBoxInput2.Name = "textBoxInput2";
            this.textBoxInput2.Size = new System.Drawing.Size(200, 27);
            this.textBoxInput2.TabIndex = 3;
            // 
            // labelInput3
            // 
            this.labelInput3.AutoSize = true;
            this.labelInput3.Location = new System.Drawing.Point(30, 110);
            this.labelInput3.Name = "labelInput3";
            this.labelInput3.Size = new System.Drawing.Size(60, 20);
            this.labelInput3.TabIndex = 4;
            this.labelInput3.Text = "Input 3:";
            // 
            // textBoxInput3
            // 
            this.textBoxInput3.Location = new System.Drawing.Point(100, 110);
            this.textBoxInput3.Name = "textBoxInput3";
            this.textBoxInput3.Size = new System.Drawing.Size(200, 27);
            this.textBoxInput3.TabIndex = 5;
            // 
            // buttonSubmit
            // 
            this.buttonSubmit.Location = new System.Drawing.Point(100, 160);
            this.buttonSubmit.Name = "buttonSubmit";
            this.buttonSubmit.Size = new System.Drawing.Size(100, 30);
            this.buttonSubmit.TabIndex = 6;
            this.buttonSubmit.Text = "Submit";
            this.buttonSubmit.UseVisualStyleBackColor = true;
            this.buttonSubmit.Click += new System.EventHandler(this.buttonSubmit_Click);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 220);
            this.Controls.Add(this.buttonSubmit);
            this.Controls.Add(this.textBoxInput3);
            this.Controls.Add(this.labelInput3);
            this.Controls.Add(this.textBoxInput2);
            this.Controls.Add(this.labelInput2);
            this.Controls.Add(this.textBoxInput1);
            this.Controls.Add(this.labelInput1);
            this.Name = "Form2";
            this.Text = "Form2";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
