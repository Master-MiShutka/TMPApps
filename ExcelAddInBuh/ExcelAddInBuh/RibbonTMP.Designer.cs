namespace ExcelAddInBuh
{
    partial class RibbonTMP : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public RibbonTMP()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabOther = this.Factory.CreateRibbonTab();
            this.groupDivided = this.Factory.CreateRibbonGroup();
            this.buttonDiv10 = this.Factory.CreateRibbonButton();
            this.buttonDiv100 = this.Factory.CreateRibbonButton();
            this.buttonDiv1000 = this.Factory.CreateRibbonButton();
            this.buttonDiv10000 = this.Factory.CreateRibbonButton();
            this.buttonDiv100t = this.Factory.CreateRibbonButton();
            this.buttonDiv1bil = this.Factory.CreateRibbonButton();
            this.group1 = this.Factory.CreateRibbonGroup();
            this.buttonMult10 = this.Factory.CreateRibbonButton();
            this.buttonMult100 = this.Factory.CreateRibbonButton();
            this.buttonMult1000 = this.Factory.CreateRibbonButton();
            this.buttonMult10t = this.Factory.CreateRibbonButton();
            this.buttonMult100t = this.Factory.CreateRibbonButton();
            this.buttonMult1bil = this.Factory.CreateRibbonButton();
            this.tabOther.SuspendLayout();
            this.groupDivided.SuspendLayout();
            this.group1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabOther
            // 
            this.tabOther.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tabOther.Groups.Add(this.groupDivided);
            this.tabOther.Groups.Add(this.group1);
            this.tabOther.Label = "Прочее";
            this.tabOther.Name = "tabOther";
            // 
            // groupDivided
            // 
            this.groupDivided.Items.Add(this.buttonDiv10);
            this.groupDivided.Items.Add(this.buttonDiv100);
            this.groupDivided.Items.Add(this.buttonDiv1000);
            this.groupDivided.Items.Add(this.buttonDiv10000);
            this.groupDivided.Items.Add(this.buttonDiv100t);
            this.groupDivided.Items.Add(this.buttonDiv1bil);
            this.groupDivided.Label = "Разделить";
            this.groupDivided.Name = "groupDivided";
            // 
            // buttonDiv10
            // 
            this.buttonDiv10.Label = "/ 10";
            this.buttonDiv10.Name = "buttonDiv10";
            this.buttonDiv10.Tag = "10";
            this.buttonDiv10.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonDivide_Click);
            // 
            // buttonDiv100
            // 
            this.buttonDiv100.Label = "/ 100";
            this.buttonDiv100.Name = "buttonDiv100";
            this.buttonDiv100.Tag = "100";
            this.buttonDiv100.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonDivide_Click);
            // 
            // buttonDiv1000
            // 
            this.buttonDiv1000.Label = "/ 1000";
            this.buttonDiv1000.Name = "buttonDiv1000";
            this.buttonDiv1000.Tag = "1000";
            this.buttonDiv1000.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonDivide_Click);
            // 
            // buttonDiv10000
            // 
            this.buttonDiv10000.Label = "/ 10 тыс";
            this.buttonDiv10000.Name = "buttonDiv10000";
            this.buttonDiv10000.Tag = "10000";
            this.buttonDiv10000.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonDivide_Click);
            // 
            // buttonDiv100t
            // 
            this.buttonDiv100t.Label = "/ 100 тыс";
            this.buttonDiv100t.Name = "buttonDiv100t";
            this.buttonDiv100t.Tag = "100000";
            this.buttonDiv100t.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonDivide_Click);
            // 
            // buttonDiv1bil
            // 
            this.buttonDiv1bil.Label = "/ 1 млн";
            this.buttonDiv1bil.Name = "buttonDiv1bil";
            this.buttonDiv1bil.Tag = "1000000";
            this.buttonDiv1bil.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonDivide_Click);
            // 
            // group1
            // 
            this.group1.Items.Add(this.buttonMult10);
            this.group1.Items.Add(this.buttonMult100);
            this.group1.Items.Add(this.buttonMult1000);
            this.group1.Items.Add(this.buttonMult10t);
            this.group1.Items.Add(this.buttonMult100t);
            this.group1.Items.Add(this.buttonMult1bil);
            this.group1.Label = "Умножить";
            this.group1.Name = "group1";
            // 
            // buttonMult10
            // 
            this.buttonMult10.Label = "× 10";
            this.buttonMult10.Name = "buttonMult10";
            this.buttonMult10.Tag = "10";
            this.buttonMult10.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonMultiple_Click);
            // 
            // buttonMult100
            // 
            this.buttonMult100.Label = "× 100";
            this.buttonMult100.Name = "buttonMult100";
            this.buttonMult100.Tag = "100";
            this.buttonMult100.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonMultiple_Click);
            // 
            // buttonMult1000
            // 
            this.buttonMult1000.Label = "× 1000";
            this.buttonMult1000.Name = "buttonMult1000";
            this.buttonMult1000.Tag = "1000";
            this.buttonMult1000.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonMultiple_Click);
            // 
            // buttonMult10t
            // 
            this.buttonMult10t.Label = "× 10 тыс";
            this.buttonMult10t.Name = "buttonMult10t";
            this.buttonMult10t.Tag = "10000";
            this.buttonMult10t.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonMultiple_Click);
            // 
            // buttonMult100t
            // 
            this.buttonMult100t.Label = "× 100 тыс";
            this.buttonMult100t.Name = "buttonMult100t";
            this.buttonMult100t.Tag = "100000";
            this.buttonMult100t.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonMultiple_Click);
            // 
            // buttonMult1bil
            // 
            this.buttonMult1bil.Label = "× 1 млн";
            this.buttonMult1bil.Name = "buttonMult1bil";
            this.buttonMult1bil.Tag = "1000000";
            this.buttonMult1bil.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonMultiple_Click);
            // 
            // RibbonTMP
            // 
            this.Name = "RibbonTMP";
            this.RibbonType = "Microsoft.Excel.Workbook";
            this.Tabs.Add(this.tabOther);
            this.tabOther.ResumeLayout(false);
            this.tabOther.PerformLayout();
            this.groupDivided.ResumeLayout(false);
            this.groupDivided.PerformLayout();
            this.group1.ResumeLayout(false);
            this.group1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tabOther;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupDivided;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonDiv10;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonDiv100;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonDiv1000;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonDiv10000;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonDiv100t;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonDiv1bil;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonMult10;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonMult100;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonMult1000;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonMult10t;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonMult100t;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonMult1bil;
    }

    partial class ThisRibbonCollection
    {
        internal RibbonTMP Ribbon1
        {
            get { return this.GetRibbon<RibbonTMP>(); }
        }
    }
}
