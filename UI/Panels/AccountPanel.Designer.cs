using NMSE.UI.Controls;

namespace NMSE.UI.Panels;

partial class AccountPanel
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
        this._mainLayout = new System.Windows.Forms.TableLayoutPanel();
        this._statusLabel = new System.Windows.Forms.Label();
        this._warningLabel = new ColorEmojiLabel();
        this._consistencyCheckBtn = new System.Windows.Forms.Button();
        this._tabControl = new NMSE.UI.Panels.DoubleBufferedTabControl();
        this._seasonPage = new System.Windows.Forms.TabPage();
        this._seasonTabLayout = new System.Windows.Forms.TableLayoutPanel();
        this._seasonToolbar = new System.Windows.Forms.FlowLayoutPanel();
        this._seasonUnlockAllBtn = new System.Windows.Forms.Button();
        this._seasonLockAllBtn = new System.Windows.Forms.Button();
        this._seasonRedeemAllBtn = new System.Windows.Forms.Button();
        this._seasonRemoveAllBtn = new System.Windows.Forms.Button();
        this._seasonFilterLabel = new System.Windows.Forms.Label();
        this._seasonFilterBox = new System.Windows.Forms.TextBox();
        this._seasonFilterClearBtn = new System.Windows.Forms.Button();
        this._seasonGrid = new System.Windows.Forms.DataGridView();
        this._seasonIconColumn = new System.Windows.Forms.DataGridViewImageColumn();
        this._seasonRewardIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._seasonRewardNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._seasonExpeditionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._seasonUnlockedColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
        this._seasonRedeemedColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
        this._twitchPage = new System.Windows.Forms.TabPage();
        this._twitchTabLayout = new System.Windows.Forms.TableLayoutPanel();
        this._twitchToolbar = new System.Windows.Forms.FlowLayoutPanel();
        this._twitchUnlockAllBtn = new System.Windows.Forms.Button();
        this._twitchLockAllBtn = new System.Windows.Forms.Button();
        this._twitchRedeemAllBtn = new System.Windows.Forms.Button();
        this._twitchRemoveAllBtn = new System.Windows.Forms.Button();
        this._twitchFilterLabel = new System.Windows.Forms.Label();
        this._twitchFilterBox = new System.Windows.Forms.TextBox();
        this._twitchFilterClearBtn = new System.Windows.Forms.Button();
        this._twitchGrid = new System.Windows.Forms.DataGridView();
        this._twitchIconColumn = new System.Windows.Forms.DataGridViewImageColumn();
        this._twitchRewardIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._twitchRewardNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._twitchUnlockedColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
        this._twitchRedeemedColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
        this._platformPage = new System.Windows.Forms.TabPage();
        this._platformTabLayout = new System.Windows.Forms.TableLayoutPanel();
        this._platformMxmlInfoLabel = new System.Windows.Forms.Label();
        this._platformMxmlFilePanel = new System.Windows.Forms.FlowLayoutPanel();
        this._platformMxmlPathBox = new System.Windows.Forms.TextBox();
        this._platformMxmlBrowseBtn = new System.Windows.Forms.Button();
        this._platformMxmlStatusLabel = new System.Windows.Forms.Label();
        this._platformToolbar = new System.Windows.Forms.FlowLayoutPanel();
        this._platformUnlockAllBtn = new System.Windows.Forms.Button();
        this._platformLockAllBtn = new System.Windows.Forms.Button();
        this._platformRedeemAllBtn = new System.Windows.Forms.Button();
        this._platformRemoveAllBtn = new System.Windows.Forms.Button();
        this._platformFilterLabel = new System.Windows.Forms.Label();
        this._platformFilterBox = new System.Windows.Forms.TextBox();
        this._platformFilterClearBtn = new System.Windows.Forms.Button();
        this._platformGrid = new System.Windows.Forms.DataGridView();
        this._platformIconColumn = new System.Windows.Forms.DataGridViewImageColumn();
        this._platformRewardIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._platformRewardNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._platformUnlockedColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
        this._platformRedeemedColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
        this._mainLayout.SuspendLayout();
        this._tabControl.SuspendLayout();
        this._seasonPage.SuspendLayout();
        this._seasonTabLayout.SuspendLayout();
        this._seasonToolbar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._seasonGrid)).BeginInit();
        this._twitchPage.SuspendLayout();
        this._twitchTabLayout.SuspendLayout();
        this._twitchToolbar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._twitchGrid)).BeginInit();
        this._platformPage.SuspendLayout();
        this._platformTabLayout.SuspendLayout();
        this._platformToolbar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._platformGrid)).BeginInit();
        this.SuspendLayout();
        //
        // _mainLayout
        //
        this._mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._mainLayout.ColumnCount = 1;
        this._mainLayout.RowCount = 4;
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._mainLayout.Controls.Add(this._statusLabel, 0, 0);
        this._mainLayout.Controls.Add(this._warningLabel, 0, 1);
        this._mainLayout.Controls.Add(this._consistencyCheckBtn, 0, 2);
        this._mainLayout.Controls.Add(this._tabControl, 0, 3);
        //
        // _statusLabel
        //
        this._statusLabel.Text = "Account data: Not loaded. Select a save directory to auto-detect accountdata.hg.";
        this._statusLabel.AutoSize = true;
        this._statusLabel.Padding = new System.Windows.Forms.Padding(5);
        this._statusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        //
        // _warningLabel
        //
        this._warningLabel.Text = "\u26A0 Twitch drops and platform rewards require you to be offline before you start the game. You can claim them at the Synthesis vendor in the Anomaly.";
        this._warningLabel.AutoSize = true;
        this._warningLabel.Padding = new System.Windows.Forms.Padding(5, 2, 5, 5);
        this._warningLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        this._warningLabel.ForeColor = System.Drawing.Color.DarkOrange;
        this._warningLabel.Font = new System.Drawing.Font("Segoe UI Emoji", 9F, System.Drawing.FontStyle.Bold);
        //
        // _consistencyCheckBtn
        //
        this._consistencyCheckBtn.Text = "Check Consistency";
        this._consistencyCheckBtn.AutoSize = true;
        this._consistencyCheckBtn.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
        //
        // _tabControl
        //
        this._tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tabControl.TabPages.Add(this._seasonPage);
        this._tabControl.TabPages.Add(this._twitchPage);
        this._tabControl.TabPages.Add(this._platformPage);
        //
        // _seasonPage
        //
        this._seasonPage.Text = "Season Rewards";
        this._seasonPage.Controls.Add(this._seasonTabLayout);
        //
        // _seasonTabLayout
        //
        this._seasonTabLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._seasonTabLayout.ColumnCount = 1;
        this._seasonTabLayout.RowCount = 2;
        this._seasonTabLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this._seasonTabLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._seasonTabLayout.Controls.Add(this._seasonToolbar, 0, 0);
        this._seasonTabLayout.Controls.Add(this._seasonGrid, 0, 1);
        //
        // _seasonToolbar
        //
        this._seasonToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
        this._seasonToolbar.AutoSize = true;
        this._seasonToolbar.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._seasonToolbar.Padding = new System.Windows.Forms.Padding(2);
        this._seasonToolbar.Controls.Add(this._seasonUnlockAllBtn);
        this._seasonToolbar.Controls.Add(this._seasonLockAllBtn);
        this._seasonToolbar.Controls.Add(this._seasonRedeemAllBtn);
        this._seasonToolbar.Controls.Add(this._seasonRemoveAllBtn);
        this._seasonToolbar.Controls.Add(this._seasonFilterLabel);
        this._seasonToolbar.Controls.Add(this._seasonFilterBox);
        this._seasonToolbar.Controls.Add(this._seasonFilterClearBtn);
        //
        // _seasonUnlockAllBtn
        //
        this._seasonUnlockAllBtn.Text = "Unlock All";
        this._seasonUnlockAllBtn.AutoSize = true;
        //
        // _seasonLockAllBtn
        //
        this._seasonLockAllBtn.Text = "Lock All";
        this._seasonLockAllBtn.AutoSize = true;
        //
        // _seasonRedeemAllBtn
        //
        this._seasonRedeemAllBtn.Text = "Redeem All";
        this._seasonRedeemAllBtn.AutoSize = true;
        //
        // _seasonRemoveAllBtn
        //
        this._seasonRemoveAllBtn.Text = "Remove All";
        this._seasonRemoveAllBtn.AutoSize = true;
        //
        // _seasonFilterLabel
        //
        this._seasonFilterLabel.Text = "Filter:";
        this._seasonFilterLabel.AutoSize = true;
        this._seasonFilterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this._seasonFilterLabel.Padding = new System.Windows.Forms.Padding(8, 5, 0, 0);
        //
        // _seasonFilterBox
        //
        this._seasonFilterBox.Width = 200;
        //
        // _seasonFilterClearBtn
        //
        this._seasonFilterClearBtn.Text = "X";
        this._seasonFilterClearBtn.AutoSize = true;
        this._seasonFilterClearBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this._seasonFilterClearBtn.MinimumSize = new System.Drawing.Size(26, 0);
        this._seasonFilterClearBtn.Click += (_, _) => { this._seasonFilterBox.Clear(); };
        //
        // _seasonGrid
        //
        this._seasonGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._seasonGrid.AllowUserToAddRows = false;
        this._seasonGrid.AllowUserToDeleteRows = false;
        this._seasonGrid.AllowUserToResizeRows = false;
        this._seasonGrid.RowHeadersVisible = false;
        this._seasonGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._seasonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this._seasonGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this._seasonIconColumn, this._seasonRewardIdColumn, this._seasonRewardNameColumn, this._seasonExpeditionColumn, this._seasonUnlockedColumn, this._seasonRedeemedColumn });
        //
        // _seasonIconColumn
        //
        this._seasonIconColumn.Name = "Icon";
        this._seasonIconColumn.HeaderText = "";
        this._seasonIconColumn.Width = 32;
        this._seasonIconColumn.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
        this._seasonIconColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
        //
        // _seasonRewardIdColumn
        //
        this._seasonRewardIdColumn.Name = "RewardId";
        this._seasonRewardIdColumn.HeaderText = "Reward ID";
        this._seasonRewardIdColumn.ReadOnly = true;
        this._seasonRewardIdColumn.FillWeight = 30F;
        //
        // _seasonRewardNameColumn
        //
        this._seasonRewardNameColumn.Name = "RewardName";
        this._seasonRewardNameColumn.HeaderText = "Name";
        this._seasonRewardNameColumn.ReadOnly = true;
        this._seasonRewardNameColumn.FillWeight = 30F;
        //
        // _seasonExpeditionColumn
        //
        this._seasonExpeditionColumn.Name = "Expedition";
        this._seasonExpeditionColumn.HeaderText = "Expedition";
        this._seasonExpeditionColumn.ReadOnly = true;
        this._seasonExpeditionColumn.FillWeight = 10F;
        //
        // _seasonUnlockedColumn
        //
        this._seasonUnlockedColumn.Name = "Unlocked";
        this._seasonUnlockedColumn.HeaderText = "Unlocked on Account";
        this._seasonUnlockedColumn.FillWeight = 15F;
        //
        // _seasonRedeemedColumn
        //
        this._seasonRedeemedColumn.Name = "RedeemedInSave";
        this._seasonRedeemedColumn.HeaderText = "Redeemed in Save";
        this._seasonRedeemedColumn.FillWeight = 15F;
        //
        // _twitchPage
        //
        this._twitchPage.Text = "Twitch Rewards";
        this._twitchPage.Controls.Add(this._twitchTabLayout);
        //
        // _twitchTabLayout
        //
        this._twitchTabLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._twitchTabLayout.ColumnCount = 1;
        this._twitchTabLayout.RowCount = 2;
        this._twitchTabLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this._twitchTabLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._twitchTabLayout.Controls.Add(this._twitchToolbar, 0, 0);
        this._twitchTabLayout.Controls.Add(this._twitchGrid, 0, 1);
        //
        // _twitchToolbar
        //
        this._twitchToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
        this._twitchToolbar.AutoSize = true;
        this._twitchToolbar.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._twitchToolbar.Padding = new System.Windows.Forms.Padding(2);
        this._twitchToolbar.Controls.Add(this._twitchUnlockAllBtn);
        this._twitchToolbar.Controls.Add(this._twitchLockAllBtn);
        this._twitchToolbar.Controls.Add(this._twitchRedeemAllBtn);
        this._twitchToolbar.Controls.Add(this._twitchRemoveAllBtn);
        this._twitchToolbar.Controls.Add(this._twitchFilterLabel);
        this._twitchToolbar.Controls.Add(this._twitchFilterBox);
        this._twitchToolbar.Controls.Add(this._twitchFilterClearBtn);
        //
        // _twitchUnlockAllBtn
        //
        this._twitchUnlockAllBtn.Text = "Unlock All";
        this._twitchUnlockAllBtn.AutoSize = true;
        //
        // _twitchLockAllBtn
        //
        this._twitchLockAllBtn.Text = "Lock All";
        this._twitchLockAllBtn.AutoSize = true;
        //
        // _twitchRedeemAllBtn
        //
        this._twitchRedeemAllBtn.Text = "Redeem All";
        this._twitchRedeemAllBtn.AutoSize = true;
        //
        // _twitchRemoveAllBtn
        //
        this._twitchRemoveAllBtn.Text = "Remove All";
        this._twitchRemoveAllBtn.AutoSize = true;
        //
        // _twitchFilterLabel
        //
        this._twitchFilterLabel.Text = "Filter:";
        this._twitchFilterLabel.AutoSize = true;
        this._twitchFilterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this._twitchFilterLabel.Padding = new System.Windows.Forms.Padding(8, 5, 0, 0);
        //
        // _twitchFilterBox
        //
        this._twitchFilterBox.Width = 200;
        //
        // _twitchFilterClearBtn
        //
        this._twitchFilterClearBtn.Text = "X";
        this._twitchFilterClearBtn.AutoSize = true;
        this._twitchFilterClearBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this._twitchFilterClearBtn.MinimumSize = new System.Drawing.Size(26, 0);
        this._twitchFilterClearBtn.Click += (_, _) => { this._twitchFilterBox.Clear(); };
        //
        // _twitchGrid
        //
        this._twitchGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._twitchGrid.AllowUserToAddRows = false;
        this._twitchGrid.AllowUserToDeleteRows = false;
        this._twitchGrid.AllowUserToResizeRows = false;
        this._twitchGrid.RowHeadersVisible = false;
        this._twitchGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._twitchGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this._twitchGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this._twitchIconColumn, this._twitchRewardIdColumn, this._twitchRewardNameColumn, this._twitchUnlockedColumn, this._twitchRedeemedColumn });
        //
        // _twitchIconColumn
        //
        this._twitchIconColumn.Name = "Icon";
        this._twitchIconColumn.HeaderText = "";
        this._twitchIconColumn.Width = 32;
        this._twitchIconColumn.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
        this._twitchIconColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
        //
        // _twitchRewardIdColumn
        //
        this._twitchRewardIdColumn.Name = "RewardId";
        this._twitchRewardIdColumn.HeaderText = "Reward ID";
        this._twitchRewardIdColumn.ReadOnly = true;
        this._twitchRewardIdColumn.FillWeight = 35F;
        //
        // _twitchRewardNameColumn
        //
        this._twitchRewardNameColumn.Name = "RewardName";
        this._twitchRewardNameColumn.HeaderText = "Name";
        this._twitchRewardNameColumn.ReadOnly = true;
        this._twitchRewardNameColumn.FillWeight = 35F;
        //
        // _twitchUnlockedColumn
        //
        this._twitchUnlockedColumn.Name = "Unlocked";
        this._twitchUnlockedColumn.HeaderText = "Unlocked on Account";
        this._twitchUnlockedColumn.FillWeight = 15F;
        //
        // _twitchRedeemedColumn
        //
        this._twitchRedeemedColumn.Name = "RedeemedInSave";
        this._twitchRedeemedColumn.HeaderText = "Redeemed in Save";
        this._twitchRedeemedColumn.FillWeight = 15F;
        //
        // _platformPage
        //
        this._platformPage.Text = "Platform Rewards";
        this._platformPage.Controls.Add(this._platformTabLayout);
        //
        // _platformTabLayout
        //
        this._platformTabLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._platformTabLayout.ColumnCount = 1;
        this._platformTabLayout.RowCount = 4;
        this._platformTabLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this._platformTabLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this._platformTabLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this._platformTabLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._platformTabLayout.Controls.Add(this._platformMxmlInfoLabel, 0, 0);
        this._platformTabLayout.Controls.Add(this._platformMxmlFilePanel, 0, 1);
        this._platformTabLayout.Controls.Add(this._platformToolbar, 0, 2);
        this._platformTabLayout.Controls.Add(this._platformGrid, 0, 3);
        //
        // _platformMxmlInfoLabel
        //
        this._platformMxmlInfoLabel.Text = "Platform Rewards require editing the file 'GCUSERSETTINGSDATA.MXML' in the NMS install directory.";
        this._platformMxmlInfoLabel.AutoSize = true;
        this._platformMxmlInfoLabel.Padding = new System.Windows.Forms.Padding(5, 5, 5, 2);
        this._platformMxmlInfoLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        this._platformMxmlInfoLabel.ForeColor = System.Drawing.Color.DarkOrange;
        this._platformMxmlInfoLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        //
        // _platformMxmlFilePanel
        //
        this._platformMxmlFilePanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this._platformMxmlFilePanel.AutoSize = true;
        this._platformMxmlFilePanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._platformMxmlFilePanel.Padding = new System.Windows.Forms.Padding(2, 0, 2, 2);
        this._platformMxmlFilePanel.Controls.Add(this._platformMxmlPathBox);
        this._platformMxmlFilePanel.Controls.Add(this._platformMxmlBrowseBtn);
        this._platformMxmlFilePanel.Controls.Add(this._platformMxmlStatusLabel);
        //
        // _platformMxmlPathBox
        //
        this._platformMxmlPathBox.Width = 500;
        this._platformMxmlPathBox.ReadOnly = true;
        this._platformMxmlPathBox.PlaceholderText = "(No MXML file selected)";
        //
        // _platformMxmlBrowseBtn
        //
        this._platformMxmlBrowseBtn.Text = "Browse...";
        this._platformMxmlBrowseBtn.AutoSize = true;
        //
        // _platformMxmlStatusLabel
        //
        this._platformMxmlStatusLabel.Text = "";
        this._platformMxmlStatusLabel.AutoSize = true;
        this._platformMxmlStatusLabel.Padding = new System.Windows.Forms.Padding(5, 5, 0, 0);
        this._platformMxmlStatusLabel.ForeColor = System.Drawing.Color.Gray;
        //
        // _platformToolbar
        //
        this._platformToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
        this._platformToolbar.AutoSize = true;
        this._platformToolbar.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._platformToolbar.Padding = new System.Windows.Forms.Padding(2);
        this._platformToolbar.Controls.Add(this._platformUnlockAllBtn);
        this._platformToolbar.Controls.Add(this._platformLockAllBtn);
        this._platformToolbar.Controls.Add(this._platformRedeemAllBtn);
        this._platformToolbar.Controls.Add(this._platformRemoveAllBtn);
        this._platformToolbar.Controls.Add(this._platformFilterLabel);
        this._platformToolbar.Controls.Add(this._platformFilterBox);
        this._platformToolbar.Controls.Add(this._platformFilterClearBtn);
        //
        // _platformUnlockAllBtn
        //
        this._platformUnlockAllBtn.Text = "Unlock All";
        this._platformUnlockAllBtn.AutoSize = true;
        //
        // _platformLockAllBtn
        //
        this._platformLockAllBtn.Text = "Lock All";
        this._platformLockAllBtn.AutoSize = true;
        //
        // _platformRedeemAllBtn
        //
        this._platformRedeemAllBtn.Text = "Redeem All";
        this._platformRedeemAllBtn.AutoSize = true;
        //
        // _platformRemoveAllBtn
        //
        this._platformRemoveAllBtn.Text = "Remove All";
        this._platformRemoveAllBtn.AutoSize = true;
        //
        // _platformFilterLabel
        //
        this._platformFilterLabel.Text = "Filter:";
        this._platformFilterLabel.AutoSize = true;
        this._platformFilterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this._platformFilterLabel.Padding = new System.Windows.Forms.Padding(8, 5, 0, 0);
        //
        // _platformFilterBox
        //
        this._platformFilterBox.Width = 200;
        //
        // _platformFilterClearBtn
        //
        this._platformFilterClearBtn.Text = "X";
        this._platformFilterClearBtn.AutoSize = true;
        this._platformFilterClearBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this._platformFilterClearBtn.MinimumSize = new System.Drawing.Size(26, 0);
        this._platformFilterClearBtn.Click += (_, _) => { this._platformFilterBox.Clear(); };
        //
        // _platformGrid
        //
        this._platformGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._platformGrid.AllowUserToAddRows = false;
        this._platformGrid.AllowUserToDeleteRows = false;
        this._platformGrid.AllowUserToResizeRows = false;
        this._platformGrid.RowHeadersVisible = false;
        this._platformGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._platformGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this._platformGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this._platformIconColumn, this._platformRewardIdColumn, this._platformRewardNameColumn, this._platformUnlockedColumn, this._platformRedeemedColumn });
        //
        // _platformIconColumn
        //
        this._platformIconColumn.Name = "Icon";
        this._platformIconColumn.HeaderText = "";
        this._platformIconColumn.Width = 32;
        this._platformIconColumn.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
        this._platformIconColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
        //
        // _platformRewardIdColumn
        //
        this._platformRewardIdColumn.Name = "RewardId";
        this._platformRewardIdColumn.HeaderText = "Reward ID";
        this._platformRewardIdColumn.ReadOnly = true;
        this._platformRewardIdColumn.FillWeight = 35F;
        //
        // _platformRewardNameColumn
        //
        this._platformRewardNameColumn.Name = "RewardName";
        this._platformRewardNameColumn.HeaderText = "Name";
        this._platformRewardNameColumn.ReadOnly = true;
        this._platformRewardNameColumn.FillWeight = 35F;
        //
        // _platformUnlockedColumn
        //
        this._platformUnlockedColumn.Name = "Unlocked";
        this._platformUnlockedColumn.HeaderText = "Unlocked on Account";
        this._platformUnlockedColumn.FillWeight = 15F;
        //
        // _platformRedeemedColumn
        //
        this._platformRedeemedColumn.Name = "RedeemedInSave";
        this._platformRedeemedColumn.HeaderText = "Redeemed in Save";
        this._platformRedeemedColumn.FillWeight = 15F;
        //
        // AccountPanel
        //
        this.DoubleBuffered = true;
        this.Controls.Add(this._mainLayout);
        this._mainLayout.ResumeLayout(false);
        this._tabControl.ResumeLayout(false);
        this._seasonPage.ResumeLayout(false);
        this._seasonTabLayout.ResumeLayout(false);
        this._seasonToolbar.ResumeLayout(false);
        this._seasonToolbar.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this._seasonGrid)).EndInit();
        this._twitchPage.ResumeLayout(false);
        this._twitchTabLayout.ResumeLayout(false);
        this._twitchToolbar.ResumeLayout(false);
        this._twitchToolbar.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this._twitchGrid)).EndInit();
        this._platformPage.ResumeLayout(false);
        this._platformTabLayout.ResumeLayout(false);
        this._platformToolbar.ResumeLayout(false);
        this._platformToolbar.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this._platformGrid)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private void SetupLayout()
    {
        _seasonUnlockAllBtn.Click += (_, _) => SetAllUnlocked(_seasonGrid, true);
        _seasonLockAllBtn.Click += (_, _) => SetAllUnlocked(_seasonGrid, false);
        _seasonRedeemAllBtn.Click += (_, _) => SetAllRedeemed(_seasonGrid, true);
        _seasonRemoveAllBtn.Click += (_, _) => SetAllRedeemed(_seasonGrid, false);
        _seasonFilterBox.TextChanged += (_, _) => ApplyFilter(_seasonGrid, _seasonFilterBox.Text);

        _twitchUnlockAllBtn.Click += (_, _) => SetAllUnlocked(_twitchGrid, true);
        _twitchLockAllBtn.Click += (_, _) => SetAllUnlocked(_twitchGrid, false);
        _twitchRedeemAllBtn.Click += (_, _) => SetAllRedeemed(_twitchGrid, true);
        _twitchRemoveAllBtn.Click += (_, _) => SetAllRedeemed(_twitchGrid, false);
        _twitchFilterBox.TextChanged += (_, _) => ApplyFilter(_twitchGrid, _twitchFilterBox.Text);

        _platformUnlockAllBtn.Click += (_, _) => SetAllUnlocked(_platformGrid, true);
        _platformLockAllBtn.Click += (_, _) => SetAllUnlocked(_platformGrid, false);
        _platformRedeemAllBtn.Click += (_, _) => SetAllRedeemed(_platformGrid, true);
        _platformRemoveAllBtn.Click += (_, _) => SetAllRedeemed(_platformGrid, false);
        _platformFilterBox.TextChanged += (_, _) => ApplyFilter(_platformGrid, _platformFilterBox.Text);
        _platformMxmlBrowseBtn.Click += OnBrowseMxml;

        _consistencyCheckBtn.Click += OnConsistencyCheck;

        // Commit checkbox edits immediately so that CollectRewardRows() reads the
        // current value. Without this, a checkbox click enters edit mode but the
        // cell Value stays stale until the user moves to another row.
        _seasonGrid.CurrentCellDirtyStateChanged += OnGridCellDirty;
        _twitchGrid.CurrentCellDirtyStateChanged += OnGridCellDirty;
        _platformGrid.CurrentCellDirtyStateChanged += OnGridCellDirty;

        // Raise DataModified when any checkbox cell value is committed so that
        // MainForm can detect unsaved changes and prompt before close.
        _seasonGrid.CellValueChanged += OnGridCellChanged;
        _twitchGrid.CellValueChanged += OnGridCellChanged;
        _platformGrid.CellValueChanged += OnGridCellChanged;
    }

    private static void OnGridCellDirty(object sender, EventArgs e)
    {
        if (sender is DataGridView grid && grid.IsCurrentCellDirty)
            grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
    }

    private void OnGridCellChanged(object sender, DataGridViewCellEventArgs e)
        => DataModified?.Invoke(this, EventArgs.Empty);

    private System.Windows.Forms.TableLayoutPanel _mainLayout;
    private System.Windows.Forms.Label _statusLabel;
    private ColorEmojiLabel _warningLabel;
    private System.Windows.Forms.Button _consistencyCheckBtn;
    private NMSE.UI.Panels.DoubleBufferedTabControl _tabControl;
    private System.Windows.Forms.TabPage _seasonPage;
    private System.Windows.Forms.TableLayoutPanel _seasonTabLayout;
    private System.Windows.Forms.FlowLayoutPanel _seasonToolbar;
    private System.Windows.Forms.Button _seasonUnlockAllBtn;
    private System.Windows.Forms.Button _seasonLockAllBtn;
    private System.Windows.Forms.Button _seasonRedeemAllBtn;
    private System.Windows.Forms.Button _seasonRemoveAllBtn;
    private System.Windows.Forms.Label _seasonFilterLabel;
    private System.Windows.Forms.TextBox _seasonFilterBox;
    private System.Windows.Forms.Button _seasonFilterClearBtn;
    private System.Windows.Forms.DataGridView _seasonGrid;
    private System.Windows.Forms.DataGridViewImageColumn _seasonIconColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn _seasonRewardIdColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn _seasonRewardNameColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn _seasonExpeditionColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn _seasonUnlockedColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn _seasonRedeemedColumn;
    private System.Windows.Forms.TabPage _twitchPage;
    private System.Windows.Forms.TableLayoutPanel _twitchTabLayout;
    private System.Windows.Forms.FlowLayoutPanel _twitchToolbar;
    private System.Windows.Forms.Button _twitchUnlockAllBtn;
    private System.Windows.Forms.Button _twitchLockAllBtn;
    private System.Windows.Forms.Button _twitchRedeemAllBtn;
    private System.Windows.Forms.Button _twitchRemoveAllBtn;
    private System.Windows.Forms.Label _twitchFilterLabel;
    private System.Windows.Forms.TextBox _twitchFilterBox;
    private System.Windows.Forms.Button _twitchFilterClearBtn;
    private System.Windows.Forms.DataGridView _twitchGrid;
    private System.Windows.Forms.DataGridViewImageColumn _twitchIconColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn _twitchRewardIdColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn _twitchRewardNameColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn _twitchUnlockedColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn _twitchRedeemedColumn;
    private System.Windows.Forms.TabPage _platformPage;
    private System.Windows.Forms.TableLayoutPanel _platformTabLayout;
    private System.Windows.Forms.Label _platformMxmlInfoLabel;
    private System.Windows.Forms.FlowLayoutPanel _platformMxmlFilePanel;
    private System.Windows.Forms.TextBox _platformMxmlPathBox;
    private System.Windows.Forms.Button _platformMxmlBrowseBtn;
    private System.Windows.Forms.Label _platformMxmlStatusLabel;
    private System.Windows.Forms.FlowLayoutPanel _platformToolbar;
    private System.Windows.Forms.Button _platformUnlockAllBtn;
    private System.Windows.Forms.Button _platformLockAllBtn;
    private System.Windows.Forms.Button _platformRedeemAllBtn;
    private System.Windows.Forms.Button _platformRemoveAllBtn;
    private System.Windows.Forms.Label _platformFilterLabel;
    private System.Windows.Forms.TextBox _platformFilterBox;
    private System.Windows.Forms.Button _platformFilterClearBtn;
    private System.Windows.Forms.DataGridView _platformGrid;
    private System.Windows.Forms.DataGridViewImageColumn _platformIconColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn _platformRewardIdColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn _platformRewardNameColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn _platformUnlockedColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn _platformRedeemedColumn;
}
