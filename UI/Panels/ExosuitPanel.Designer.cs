using NMSE.Core;
using NMSE.UI.Util;

namespace NMSE.UI.Panels;

partial class ExosuitPanel
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
        this._layout = new System.Windows.Forms.TableLayoutPanel();
        this._headerStrip = new System.Windows.Forms.TableLayoutPanel();
        this._gotoButtonPanel = new System.Windows.Forms.FlowLayoutPanel();
        this._titleLabel = new System.Windows.Forms.Label();
        this._gotoJsonBtn = new System.Windows.Forms.Button();
        this._generalGrid = new NMSE.UI.Panels.InventoryGridPanel();
        this._techGrid = new NMSE.UI.Panels.InventoryGridPanel();
        this._invTabs = new NMSE.UI.Panels.DoubleBufferedTabControl();
        this._generalPage = new System.Windows.Forms.TabPage();
        this._techPage = new System.Windows.Forms.TabPage();
        this._layout.SuspendLayout();
        this._headerStrip.SuspendLayout();
        this._gotoButtonPanel.SuspendLayout();
        this._invTabs.SuspendLayout();
        this._generalPage.SuspendLayout();
        this._techPage.SuspendLayout();
        this.SuspendLayout();
        //
        // _layout
        //
        this._layout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._layout.ColumnCount = 1;
        this._layout.RowCount = 2;
        this._layout.Padding = new System.Windows.Forms.Padding(10);
        this._layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this._layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._layout.Controls.Add(this._headerStrip, 0, 0);
        this._layout.Controls.Add(this._invTabs, 0, 1);
        //
        // _headerStrip
        //
        this._headerStrip.Dock = System.Windows.Forms.DockStyle.Top;
        this._headerStrip.AutoSize = true;
        this._headerStrip.ColumnCount = 3;
        this._headerStrip.RowCount = 1;
        this._headerStrip.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this._headerStrip.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._headerStrip.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this._headerStrip.Controls.Add(this._titleLabel, 0, 0);
        this._headerStrip.Controls.Add(this._gotoButtonPanel, 2, 0);
        //
        // _titleLabel
        //
        this._titleLabel.Text = "Exosuit Inventory";
        FontManager.ApplyHeadingFont(_titleLabel, 14);
        this._titleLabel.AutoSize = true;
        this._titleLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
        this._titleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        this._titleLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
        //
        // _gotoButtonPanel
        //
        this._gotoButtonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gotoButtonPanel.AutoSize = true;
		this._gotoButtonPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
		this._gotoButtonPanel.WrapContents = false;
        //
        // _gotoJsonBtn
        //
		this._gotoJsonBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this._gotoJsonBtn.FlatAppearance.BorderSize = 1;
		this._gotoJsonBtn.FlatAppearance.BorderColor = ThemeManager.Effective == AppTheme.Dark ? Color.FromArgb(100, 100, 100) : SystemColors.ControlDark;
        this._gotoJsonBtn.Font = new System.Drawing.Font("Segoe UI Emoji", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._gotoJsonBtn.Size = new System.Drawing.Size(28, 24);
        this._gotoJsonBtn.Text = "\U0001F4D1";
        this._gotoJsonBtn.Margin = new System.Windows.Forms.Padding(1, 3, 1, 1);
        this._gotoJsonBtn.Cursor = System.Windows.Forms.Cursors.Hand;
        this._gotoJsonBtn.Click += OnGoToJsonClicked;
        this._gotoButtonPanel.Controls.Add(this._gotoJsonBtn);
        //
        // _generalGrid
        //
        this._generalGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        //
        // _techGrid
        //
        this._techGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        //
        // _invTabs
        //
        this._invTabs.Dock = System.Windows.Forms.DockStyle.Fill;
        this._invTabs.TabPages.Add(this._generalPage);
        this._invTabs.TabPages.Add(this._techPage);
        //
        // _generalPage
        //
        this._generalPage.Text = "Cargo";
        this._generalPage.Controls.Add(this._generalGrid);
        //
        // _techPage
        //
        this._techPage.Text = "Technology";
        this._techPage.Controls.Add(this._techGrid);
        //
        // ExosuitPanel
        //
        this.DoubleBuffered = true;
        this.Controls.Add(this._layout);
        this._layout.ResumeLayout(false);
        this._layout.PerformLayout();
        this._headerStrip.ResumeLayout(false);
        this._headerStrip.PerformLayout();
        this._gotoButtonPanel.ResumeLayout(false);
        this._gotoButtonPanel.PerformLayout();
        this._invTabs.ResumeLayout(false);
        this._generalPage.ResumeLayout(false);
        this._techPage.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private void SetupLayout()
    {
        _techGrid.SetIsTechInventory(true);
        _generalGrid.SetIsCargoInventory(true);
        _generalGrid.SetSortingEnabled(true);
        _techGrid.SetSortingEnabled(false);
        _techGrid.SetInventoryOwnerType("Suit");
        _generalGrid.SetInventoryOwnerType("Suit");
        _generalGrid.SetInventoryGroup("PersonalCargo");
        _generalGrid.SetPinSlotFeatureEnabled(true);
        _techGrid.SetInventoryGroup("Personal");
        _generalGrid.DataModified += (s, e) => DataModified?.Invoke(this, e);
        _techGrid.DataModified += (s, e) => DataModified?.Invoke(this, e);
        _generalGrid.PinnedSlotsChanged += OnPinnedSlotsChanged;
        _generalGrid.AutoStackToStorageRequested += OnAutoStackToStorageRequested;
        _generalGrid.AutoStackToStarshipRequested += OnAutoStackToStarshipRequested;
        _generalGrid.AutoStackToFreighterRequested += OnAutoStackToFreighterRequested;
        _generalGrid.AutoStackSelectedSlotToStorageRequested += OnAutoStackSelectedSlotToStorageRequested;
        _generalGrid.AutoStackSelectedSlotToStarshipRequested += OnAutoStackSelectedSlotToStarshipRequested;
        _generalGrid.AutoStackSelectedSlotToFreighterRequested += OnAutoStackSelectedSlotToFreighterRequested;
        _generalGrid.RefreshToolbarActions();
        _techGrid.RefreshToolbarActions();
        var cfg = ExportConfig.Instance;
        _generalGrid.SetExportFileName($"exosuit_cargo_inv{cfg.ExosuitExt}");
        _techGrid.SetExportFileName($"exosuit_tech_inv{cfg.ExosuitExt}");
        string cargoExportFilter = ExportConfig.BuildDialogFilter(cfg.ExosuitExt, "Exosuit cargo");
        string cargoImportFilter = ExportConfig.BuildImportFilter(cfg.ExosuitExt, "Exosuit cargo");
        _generalGrid.SetExportFileFilter(cargoExportFilter, cargoImportFilter, cfg.ExosuitExt.TrimStart('.'));
        string techExportFilter = ExportConfig.BuildDialogFilter(cfg.ExosuitExt, "Exosuit tech");
        string techImportFilter = ExportConfig.BuildImportFilter(cfg.ExosuitExt, "Exosuit tech");
        _techGrid.SetExportFileFilter(techExportFilter, techImportFilter, cfg.ExosuitExt.TrimStart('.'));
        _generalGrid.SetMaxSupportedLabel(ExosuitLogic.CargoMaxLabel);
        _techGrid.SetMaxSupportedLabel(ExosuitLogic.TechMaxLabel);
        _generalGrid.SetSuperchargeDisabled(true);
    }

    private System.Windows.Forms.TableLayoutPanel _layout;
    private System.Windows.Forms.TableLayoutPanel _headerStrip;
    private System.Windows.Forms.FlowLayoutPanel _gotoButtonPanel;
    private System.Windows.Forms.Label _titleLabel;
    private System.Windows.Forms.Button _gotoJsonBtn;
    private NMSE.UI.Panels.DoubleBufferedTabControl _invTabs;
    private System.Windows.Forms.TabPage _generalPage;
    private System.Windows.Forms.TabPage _techPage;
    private InventoryGridPanel _generalGrid;
    private InventoryGridPanel _techGrid;
}
