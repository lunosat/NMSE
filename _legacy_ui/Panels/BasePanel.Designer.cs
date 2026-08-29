#nullable enable
namespace NMSE.UI.Panels;

partial class BasePanel
{
    private System.ComponentModel.IContainer? components = null;

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
        this._basesSubPanel = new NMSE.UI.Panels.BasesSubPanel();
        this._chestsSubPanel = new NMSE.UI.Panels.ChestsSubPanel();
        this._storageSubPanel = new NMSE.UI.Panels.StorageSubPanel();
        this._innerTabs = new NMSE.UI.Panels.DoubleBufferedTabControl();
        this._basesPage = new System.Windows.Forms.TabPage();
        this._chestsPage = new System.Windows.Forms.TabPage();
        this._storagePage = new System.Windows.Forms.TabPage();
        this.SuspendLayout();
        //
        // _basesSubPanel
        //
        this._basesSubPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        //
        // _chestsSubPanel
        //
        this._chestsSubPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        //
        // _storageSubPanel
        //
        this._storageSubPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        //
        // basesPage
        //
        this._basesPage.Text = "Bases";
        this._basesPage.Controls.Add(this._basesSubPanel);
        //
        // chestsPage
        //
        this._chestsPage.Text = "Chests";
        this._chestsPage.Controls.Add(this._chestsSubPanel);
        //
        // storagePage
        //
        this._storagePage.Text = "Storage";
        this._storagePage.Controls.Add(this._storageSubPanel);
        //
        // _innerTabs
        //
        this._innerTabs.Dock = System.Windows.Forms.DockStyle.Fill;
        this._innerTabs.TabPages.Add(this._basesPage);
        this._innerTabs.TabPages.Add(this._chestsPage);
        this._innerTabs.TabPages.Add(this._storagePage);
        //
        // BasePanel
        //
        this.DoubleBuffered = true;
        this.Controls.Add(this._innerTabs);
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private NMSE.UI.Panels.DoubleBufferedTabControl _innerTabs = null!;
    private NMSE.UI.Panels.BasesSubPanel _basesSubPanel = null!;
    private NMSE.UI.Panels.ChestsSubPanel _chestsSubPanel = null!;
    private NMSE.UI.Panels.StorageSubPanel _storageSubPanel = null!;
    private System.Windows.Forms.TabPage _basesPage = null!;
    private System.Windows.Forms.TabPage _chestsPage = null!;
    private System.Windows.Forms.TabPage _storagePage = null!;
}
