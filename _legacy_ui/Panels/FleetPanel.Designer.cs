namespace NMSE.UI.Panels;

partial class FleetPanel
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

    #region Component Designer generated code

    private void InitializeComponent()
    {
        this._tabControl = new DoubleBufferedTabControl();
        this._freighterTab = new System.Windows.Forms.TabPage();
        this._frigateTab = new System.Windows.Forms.TabPage();
        this._squadronTab = new System.Windows.Forms.TabPage();
        this._tabControl.SuspendLayout();
        this.SuspendLayout();
        //
        // _tabControl
        //
        this._tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tabControl.TabPages.Add(this._freighterTab);
        this._tabControl.TabPages.Add(this._frigateTab);
        this._tabControl.TabPages.Add(this._squadronTab);
        //
        // _freighterTab
        //
        this._freighterTab.Text = "Freighter";
        //
        // _frigateTab
        //
        this._frigateTab.Text = "Frigates";
        //
        // _squadronTab
        //
        this._squadronTab.Text = "Squadron";
        //
        // FleetPanel
        //
        this.Controls.Add(this._tabControl);
        this._tabControl.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    #endregion

    private DoubleBufferedTabControl _tabControl;
    private System.Windows.Forms.TabPage _freighterTab;
    private System.Windows.Forms.TabPage _frigateTab;
    private System.Windows.Forms.TabPage _squadronTab;
}
