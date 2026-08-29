namespace NMSE.UI.Panels;

partial class RecipePanel
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
        this.SuspendLayout();
        this.Name = "RecipePanel";
        this.Size = new System.Drawing.Size(800, 600);
        this.ResumeLayout(false);
    }

    #endregion

    private void SetupLayout()
    {
        TableLayoutPanel mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10),
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Filter/Search bar
        FlowLayoutPanel filterPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
        _typeFilterLabel = new Label { Text = "Type:", AutoSize = true, Padding = new Padding(0, 5, 5, 0) };
        filterPanel.Controls.Add(_typeFilterLabel);
        _filterCombo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 120,
            Items = { "All", "Refining", "Cooking" }
        };
        _filterCombo.SelectedIndex = 0;
        _filterCombo.SelectedIndexChanged += OnFilterChanged;
        filterPanel.Controls.Add(_filterCombo);

        filterPanel.Controls.Add(new Label { Text = "  Search:", AutoSize = true, Padding = new Padding(0, 5, 5, 0) });
        _searchBox = new TextBox { Width = 200 };
        _searchBox.TextChanged += OnFilterChanged;
        filterPanel.Controls.Add(_searchBox);

        Button clearFilterBtn = new Button { Text = "X", Width = 28, Height = 23 };
        clearFilterBtn.Click += (s, e) =>
        {
            _searchBox.Text = "";
            _filterCombo.SelectedIndex = 0;
        };
        filterPanel.Controls.Add(clearFilterBtn);

        _nmsRecipesBtn = new Button { Text = "NMS Recipes (Web)", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(10, 0, 0, 0) };

        _nmsRecipesBtn.Click += (s, e) =>
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://nomansskyrecipes.com/",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to open URL: {ex.Message}");
            }
        };
        filterPanel.Controls.Add(_nmsRecipesBtn);

        mainLayout.Controls.Add(filterPanel, 0, 0);

        // Recipe grid
        _recipeGrid = new DataGridView
        {
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            Dock = DockStyle.Fill,
        };
        _recipeGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Inputs", HeaderText = "Ingredients", FillWeight = 30 });
        _recipeGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Arrow", HeaderText = "▶", FillWeight = 2 });
        _recipeGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Output", HeaderText = "Result", FillWeight = 20 });
        _recipeGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Time", HeaderText = "Time (s)", FillWeight = 8 });
        _recipeGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "Type", FillWeight = 8 });
        _recipeGrid.SelectionChanged += OnRecipeSelected;
        _recipeGrid.CellPainting += OnRecipeGridCellPainting;
        mainLayout.Controls.Add(_recipeGrid, 0, 1);

        // Detail label
        _detailLabel = new Label
        {
            AutoSize = true,
            Text = NMSE.Data.UiStrings.Get("recipe.details_placeholder"),
            Padding = new Padding(0, 5, 0, 5),
        };
        mainLayout.Controls.Add(_detailLabel, 0, 2);

        Controls.Add(mainLayout);
    }

    private System.Windows.Forms.ComboBox _filterCombo;
    private System.Windows.Forms.Label _typeFilterLabel;
    private System.Windows.Forms.TextBox _searchBox;
    private System.Windows.Forms.DataGridView _recipeGrid;
    private System.Windows.Forms.Label _detailLabel;
    private System.Windows.Forms.Button _nmsRecipesBtn;
}
