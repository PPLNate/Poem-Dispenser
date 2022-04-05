
namespace Poem_Dispenser
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
            this.RefreshPoems = new System.ComponentModel.BackgroundWorker();
            this.cb_Printers = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // RefreshPoems
            // 
            this.RefreshPoems.DoWork += new System.ComponentModel.DoWorkEventHandler(this.RefreshPoems_DoWork);
            // 
            // cb_Printers
            // 
            this.cb_Printers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_Printers.FormattingEnabled = true;
            this.cb_Printers.Location = new System.Drawing.Point(13, 38);
            this.cb_Printers.Name = "cb_Printers";
            this.cb_Printers.Size = new System.Drawing.Size(249, 21);
            this.cb_Printers.TabIndex = 0;
            this.cb_Printers.SelectionChangeCommitted += new System.EventHandler(this.cb_Printers_SelectionChangeCommitted);
            this.cb_Printers.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form1_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Print to:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(362, 174);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cb_Printers);
            this.Name = "Form1";
            this.Text = "Poemz";
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form1_KeyPress);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.ComponentModel.BackgroundWorker RefreshPoems;
        private System.Windows.Forms.ComboBox cb_Printers;
        private System.Windows.Forms.Label label1;
    }
}

