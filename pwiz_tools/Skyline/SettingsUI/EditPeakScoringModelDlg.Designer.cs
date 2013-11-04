﻿namespace pwiz.Skyline.SettingsUI
{
    partial class EditPeakScoringModelDlg
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditPeakScoringModelDlg));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.helpTip = new System.Windows.Forms.ToolTip(this.components);
            this.bindingPeakCalculators = new System.Windows.Forms.BindingSource(this.components);
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.zedGraphMProphet = new ZedGraph.ZedGraphControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.zedGraphSelectedCalculator = new ZedGraph.ZedGraphControl();
            this.gridPeakCalculators = new pwiz.Skyline.Controls.DataGridViewEx();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.decoyCheckBox = new System.Windows.Forms.CheckBox();
            this.secondBestCheckBox = new System.Windows.Forms.CheckBox();
            this.btnTrainModel = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.lblColinearWarning = new System.Windows.Forms.Label();
            this.comboModel = new System.Windows.Forms.ComboBox();
            this.textName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.IsEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.PeakCalculatorName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PeakCalculatorWeight = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PeakCalculatorPercentContribution = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.bindingPeakCalculators)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridPeakCalculators)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // helpTip
            // 
            this.helpTip.AutoPopDelay = 15000;
            this.helpTip.InitialDelay = 500;
            this.helpTip.ReshowDelay = 100;
            // 
            // btnOk
            // 
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.Name = "btnOk";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.zedGraphMProphet);
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // zedGraphMProphet
            // 
            resources.ApplyResources(this.zedGraphMProphet, "zedGraphMProphet");
            this.zedGraphMProphet.IsEnableHPan = false;
            this.zedGraphMProphet.IsEnableHZoom = false;
            this.zedGraphMProphet.IsEnableVPan = false;
            this.zedGraphMProphet.IsEnableVZoom = false;
            this.zedGraphMProphet.IsEnableWheelZoom = false;
            this.zedGraphMProphet.IsShowCopyMessage = false;
            this.zedGraphMProphet.Name = "zedGraphMProphet";
            this.zedGraphMProphet.ScrollGrace = 0D;
            this.zedGraphMProphet.ScrollMaxX = 0D;
            this.zedGraphMProphet.ScrollMaxY = 0D;
            this.zedGraphMProphet.ScrollMaxY2 = 0D;
            this.zedGraphMProphet.ScrollMinX = 0D;
            this.zedGraphMProphet.ScrollMinY = 0D;
            this.zedGraphMProphet.ScrollMinY2 = 0D;
            this.zedGraphMProphet.ContextMenuBuilder += new ZedGraph.ZedGraphControl.ContextMenuBuilderEventHandler(this.zedGraph_ContextMenuBuilder);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.zedGraphSelectedCalculator);
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // zedGraphSelectedCalculator
            // 
            resources.ApplyResources(this.zedGraphSelectedCalculator, "zedGraphSelectedCalculator");
            this.zedGraphSelectedCalculator.IsEnableHPan = false;
            this.zedGraphSelectedCalculator.IsEnableHZoom = false;
            this.zedGraphSelectedCalculator.IsEnableVPan = false;
            this.zedGraphSelectedCalculator.IsEnableVZoom = false;
            this.zedGraphSelectedCalculator.IsEnableWheelZoom = false;
            this.zedGraphSelectedCalculator.IsShowCopyMessage = false;
            this.zedGraphSelectedCalculator.Name = "zedGraphSelectedCalculator";
            this.zedGraphSelectedCalculator.ScrollGrace = 0D;
            this.zedGraphSelectedCalculator.ScrollMaxX = 0D;
            this.zedGraphSelectedCalculator.ScrollMaxY = 0D;
            this.zedGraphSelectedCalculator.ScrollMaxY2 = 0D;
            this.zedGraphSelectedCalculator.ScrollMinX = 0D;
            this.zedGraphSelectedCalculator.ScrollMinY = 0D;
            this.zedGraphSelectedCalculator.ScrollMinY2 = 0D;
            this.zedGraphSelectedCalculator.ContextMenuBuilder += new ZedGraph.ZedGraphControl.ContextMenuBuilderEventHandler(this.zedGraph_ContextMenuBuilder);
            // 
            // gridPeakCalculators
            // 
            this.gridPeakCalculators.AllowUserToAddRows = false;
            this.gridPeakCalculators.AllowUserToDeleteRows = false;
            resources.ApplyResources(this.gridPeakCalculators, "gridPeakCalculators");
            this.gridPeakCalculators.AutoGenerateColumns = false;
            this.gridPeakCalculators.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridPeakCalculators.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.gridPeakCalculators.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridPeakCalculators.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.IsEnabled,
            this.PeakCalculatorName,
            this.PeakCalculatorWeight,
            this.PeakCalculatorPercentContribution});
            this.gridPeakCalculators.DataSource = this.bindingPeakCalculators;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridPeakCalculators.DefaultCellStyle = dataGridViewCellStyle4;
            this.gridPeakCalculators.Name = "gridPeakCalculators";
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridPeakCalculators.RowHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.gridPeakCalculators.RowHeadersVisible = false;
            this.gridPeakCalculators.ShowEditingIcon = false;
            this.helpTip.SetToolTip(this.gridPeakCalculators, resources.GetString("gridPeakCalculators.ToolTip"));
            this.gridPeakCalculators.SelectionChanged += new System.EventHandler(this.gridPeakCalculators_SelectionChanged);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.decoyCheckBox);
            this.groupBox2.Controls.Add(this.secondBestCheckBox);
            this.groupBox2.Controls.Add(this.btnTrainModel);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // decoyCheckBox
            // 
            resources.ApplyResources(this.decoyCheckBox, "decoyCheckBox");
            this.decoyCheckBox.Name = "decoyCheckBox";
            this.decoyCheckBox.UseVisualStyleBackColor = true;
            this.decoyCheckBox.CheckedChanged += new System.EventHandler(this.decoyCheckBox_CheckedChanged);
            // 
            // secondBestCheckBox
            // 
            resources.ApplyResources(this.secondBestCheckBox, "secondBestCheckBox");
            this.secondBestCheckBox.Name = "secondBestCheckBox";
            this.secondBestCheckBox.UseVisualStyleBackColor = true;
            this.secondBestCheckBox.CheckedChanged += new System.EventHandler(this.falseTargetCheckBox_CheckedChanged);
            // 
            // btnTrainModel
            // 
            resources.ApplyResources(this.btnTrainModel, "btnTrainModel");
            this.btnTrainModel.Name = "btnTrainModel";
            this.helpTip.SetToolTip(this.btnTrainModel, resources.GetString("btnTrainModel.ToolTip"));
            this.btnTrainModel.UseVisualStyleBackColor = true;
            this.btnTrainModel.Click += new System.EventHandler(this.btnTrainModel_Click);
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // lblColinearWarning
            // 
            resources.ApplyResources(this.lblColinearWarning, "lblColinearWarning");
            this.lblColinearWarning.ForeColor = System.Drawing.Color.Red;
            this.lblColinearWarning.Name = "lblColinearWarning";
            // 
            // comboModel
            // 
            this.comboModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboModel.FormattingEnabled = true;
            this.comboModel.Items.AddRange(new object[] {
            resources.GetString("comboModel.Items"),
            resources.GetString("comboModel.Items1")});
            resources.ApplyResources(this.comboModel, "comboModel");
            this.comboModel.Name = "comboModel";
            // 
            // textName
            // 
            resources.ApplyResources(this.textName, "textName");
            this.textName.Name = "textName";
            this.helpTip.SetToolTip(this.textName, resources.GetString("textName.ToolTip"));
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // IsEnabled
            // 
            this.IsEnabled.DataPropertyName = "IsEnabled";
            this.IsEnabled.FalseValue = "False";
            resources.ApplyResources(this.IsEnabled, "IsEnabled");
            this.IsEnabled.Name = "IsEnabled";
            this.IsEnabled.TrueValue = "True";
            // 
            // PeakCalculatorName
            // 
            this.PeakCalculatorName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.PeakCalculatorName.DataPropertyName = "Name";
            this.PeakCalculatorName.FillWeight = 500F;
            resources.ApplyResources(this.PeakCalculatorName, "PeakCalculatorName");
            this.PeakCalculatorName.Name = "PeakCalculatorName";
            this.PeakCalculatorName.ReadOnly = true;
            // 
            // PeakCalculatorWeight
            // 
            this.PeakCalculatorWeight.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.PeakCalculatorWeight.DataPropertyName = "Weight";
            dataGridViewCellStyle2.Format = "N4";
            this.PeakCalculatorWeight.DefaultCellStyle = dataGridViewCellStyle2;
            this.PeakCalculatorWeight.FillWeight = 80F;
            resources.ApplyResources(this.PeakCalculatorWeight, "PeakCalculatorWeight");
            this.PeakCalculatorWeight.Name = "PeakCalculatorWeight";
            this.PeakCalculatorWeight.ReadOnly = true;
            // 
            // PeakCalculatorPercentContribution
            // 
            this.PeakCalculatorPercentContribution.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.PeakCalculatorPercentContribution.DataPropertyName = "PercentContribution";
            dataGridViewCellStyle3.Format = "0.0%";
            dataGridViewCellStyle3.NullValue = null;
            this.PeakCalculatorPercentContribution.DefaultCellStyle = dataGridViewCellStyle3;
            resources.ApplyResources(this.PeakCalculatorPercentContribution, "PeakCalculatorPercentContribution");
            this.PeakCalculatorPercentContribution.Name = "PeakCalculatorPercentContribution";
            this.PeakCalculatorPercentContribution.ReadOnly = true;
            // 
            // EditPeakScoringModelDlg
            // 
            this.AcceptButton = this.btnOk;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.gridPeakCalculators);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lblColinearWarning);
            this.Controls.Add(this.comboModel);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.textName);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditPeakScoringModelDlg";
            this.ShowInTaskbar = false;
            ((System.ComponentModel.ISupportInitialize)(this.bindingPeakCalculators)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridPeakCalculators)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private pwiz.Skyline.Controls.DataGridViewEx gridPeakCalculators;
        private System.Windows.Forms.Button btnTrainModel;
        private System.Windows.Forms.ToolTip helpTip;
        private System.Windows.Forms.BindingSource bindingPeakCalculators;
        private System.Windows.Forms.Label label3;
        private ZedGraph.ZedGraphControl zedGraphMProphet;
        private ZedGraph.ZedGraphControl zedGraphSelectedCalculator;
        private System.Windows.Forms.Label lblColinearWarning;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboModel;
        private System.Windows.Forms.CheckBox secondBestCheckBox;
        private System.Windows.Forms.CheckBox decoyCheckBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridViewCheckBoxColumn IsEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn PeakCalculatorName;
        private System.Windows.Forms.DataGridViewTextBoxColumn PeakCalculatorWeight;
        private System.Windows.Forms.DataGridViewTextBoxColumn PeakCalculatorPercentContribution;
    }
}