namespace PD2_script_data
{
    partial class MainForm
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
            this.decodeButton = new System.Windows.Forms.Button();
            this.encodeButton = new System.Windows.Forms.Button();
            this.jsonEncodeTypeCombobox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.generateXMLFile = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // decodeButton
            // 
            this.decodeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.decodeButton.Location = new System.Drawing.Point(12, 12);
            this.decodeButton.Name = "decodeButton";
            this.decodeButton.Size = new System.Drawing.Size(107, 23);
            this.decodeButton.TabIndex = 0;
            this.decodeButton.Text = "Load Script File";
            this.decodeButton.UseVisualStyleBackColor = true;
            this.decodeButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // encodeButton
            // 
            this.encodeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.encodeButton.Location = new System.Drawing.Point(132, 12);
            this.encodeButton.Name = "encodeButton";
            this.encodeButton.Size = new System.Drawing.Size(106, 23);
            this.encodeButton.TabIndex = 1;
            this.encodeButton.Text = "Encode Json File";
            this.encodeButton.UseVisualStyleBackColor = true;
            this.encodeButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // jsonEncodeTypeCombobox
            // 
            this.jsonEncodeTypeCombobox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.jsonEncodeTypeCombobox.FormattingEnabled = true;
            this.jsonEncodeTypeCombobox.Items.AddRange(new object[] {
            "sequence_manager",
            "environment",
            "menu",
            "continent",
            "continents",
            "mission",
            "nav_data",
            "cover_data",
            "world",
            "world_cameras",
            "world_sounds",
            "prefhud"});
            this.jsonEncodeTypeCombobox.Location = new System.Drawing.Point(119, 41);
            this.jsonEncodeTypeCombobox.Name = "jsonEncodeTypeCombobox";
            this.jsonEncodeTypeCombobox.Size = new System.Drawing.Size(119, 21);
            this.jsonEncodeTypeCombobox.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Encode Script Type:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(108, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(133, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Written by I am not a spy...";
            // 
            // generateXMLFile
            // 
            this.generateXMLFile.AutoSize = true;
            this.generateXMLFile.Checked = true;
            this.generateXMLFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.generateXMLFile.Location = new System.Drawing.Point(12, 68);
            this.generateXMLFile.Name = "generateXMLFile";
            this.generateXMLFile.Size = new System.Drawing.Size(177, 17);
            this.generateXMLFile.TabIndex = 5;
            this.generateXMLFile.Text = "Generate XML file on script load";
            this.generateXMLFile.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(260, 115);
            this.Controls.Add(this.generateXMLFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.jsonEncodeTypeCombobox);
            this.Controls.Add(this.encodeButton);
            this.Controls.Add(this.decodeButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximumSize = new System.Drawing.Size(276, 153);
            this.MinimumSize = new System.Drawing.Size(276, 153);
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "Diesel Script Tool";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button decodeButton;
        private System.Windows.Forms.Button encodeButton;
        private System.Windows.Forms.ComboBox jsonEncodeTypeCombobox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox generateXMLFile;
    }
}

