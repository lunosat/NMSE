using NMSE.Core;
using NMSE.UI.Controls;
using NMSE.UI.Panels;

namespace NMSE.UI;

/// <summary>
/// Walks a Form or Control tree and applies the current <see cref="ThemeColors.Palette"/>
/// colours. Leverages WinForms ambient property propagation for ForeColor/BackColor
/// so controls with explicit semantic colours (e.g. warning labels) are not overwritten.
/// </summary>
internal static class ThemeApplicator
{
	// Track original BorderStyle for panels so we can restore them in light mode.
	private static readonly Dictionary<Panel, BorderStyle> _panelBorders = new();

	/// <summary>
	/// Applies the active theme palette to the entire form, including menu strip,
	/// tool strips, status strip, and all child controls.
	/// </summary>
	internal static void ApplyToForm(Form form)
	{
		var p = ThemeColors.Get(ThemeManager.Effective == AppTheme.Dark ? "Dark" : "Light");

		form.BackColor = p.Background;
		form.ForeColor = p.ForegroundText;

		form.SuspendLayout();
		foreach (Control child in form.Controls)
			ApplyToControl(child, p);
		form.ResumeLayout(false);
	}

	/// <summary>
	/// Applies the current theme colours to a single control and its children.
	/// Use this when creating controls dynamically after the initial theme application.
	/// </summary>
	internal static void ApplyToControlTree(Control control)
	{
		var p = ThemeColors.Get(ThemeManager.Effective == AppTheme.Dark ? "Dark" : "Light");
		ApplyToControl(control, p);
	}

	private static void ApplyToControl(Control control, ThemeColors.Palette p)
	{
		switch (control)
		{
			case TabControl tab:
				ApplyTabControl(tab, p);
				break;

			case TabPage tabPage:
				tabPage.BackColor = p.Background;
				break;

			case SplitContainer split:
				split.BackColor = p.SplitterColor;
				break;

			case DataGridView grid:
				ApplyDataGridView(grid, p);
				break;

			case TextBox textBox:
				textBox.BackColor = p.InputBackground;
				textBox.ForeColor = p.InputForeground;
				break;

			case RichTextBox rtb:
				rtb.BackColor = p.InputBackground;
				rtb.ForeColor = p.InputForeground;
				break;

			case ComboBox combo:
				if (ThemeManager.Effective == AppTheme.Dark)
				{
					combo.FlatStyle = FlatStyle.Flat;
					combo.BackColor = p.InputBackground;
					combo.ForeColor = p.InputForeground;
				}
				else
				{
					combo.FlatStyle = FlatStyle.Standard;
					combo.ResetBackColor();
					combo.ResetForeColor();
				}
				break;

			case Button button:
				if (ThemeManager.Effective == AppTheme.Dark)
				{
					button.BackColor = p.ButtonBackground;
					button.ForeColor = p.ButtonForeground;
					button.FlatStyle = FlatStyle.Flat;
					button.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
				}
				else
				{
					button.UseVisualStyleBackColor = true;
					button.ResetBackColor();
					button.ResetForeColor();
					button.FlatStyle = button.FlatAppearance.BorderSize == 0
						? FlatStyle.Flat
						: FlatStyle.Standard;
				}
				break;

			case NumericUpDown numeric:
				numeric.BackColor = p.InputBackground;
				numeric.ForeColor = p.InputForeground;
				break;

			case DateTimePicker dtp:
				ApplyDateTimePicker(dtp, p);
				break;

			case ListBox listBox:
				listBox.BackColor = p.InputBackground;
				listBox.ForeColor = p.InputForeground;
				break;

			case ListView listView:
				listView.BackColor = p.InputBackground;
				listView.ForeColor = p.InputForeground;
				break;

			case TreeView treeView:
				treeView.BackColor = p.InputBackground;
				treeView.ForeColor = p.InputForeground;
				break;

			case PropertyGrid propGrid:
				propGrid.BackColor = p.InputBackground;
				propGrid.HelpBackColor = p.Background;
				propGrid.HelpForeColor = p.ForegroundText;
				break;

			case MenuStrip menuStrip:
				ApplyMenuStrip(menuStrip, p);
				break;

			case StatusStrip statusStrip:
				ApplyStatusStrip(statusStrip, p);
				break;

			case ToolStrip toolStrip:
				ApplyToolStrip(toolStrip, p);
				break;

			case LinkLabel linkLabel:
				linkLabel.LinkColor = p.InfoBlue;
				break;

			case JsonSyntaxTextBox jsonBox:
				jsonBox.ApplyThemePalette(p);
				break;

			case InvariantNumericTextBox numericBox:
				numericBox.ApplyThemePalette(p);
				ApplyToControl(numericBox.TextBox, p);
				ApplyToControl(numericBox.Spinner, p);
				break;

			case InventoryGridPanel gridPanel:
				gridPanel.ApplyThemePalette(p);
				break;

			case UserControl userControl:
				userControl.BackColor = p.Background;
				break;

			case Label label:
				if (ThemeManager.Effective == AppTheme.Dark)
					label.ForeColor = p.ForegroundText;
				else
					label.ResetForeColor();
				break;

			case Panel panel:
				if (panel.Tag is string tag && tag == "theme:custom")
					break;

				panel.BackColor = p.Background;
				ApplyPanelBorder(panel, p);
				break;
		}

		foreach (Control child in control.Controls)
			ApplyToControl(child, p);
	}

	private static void ApplyDataGridView(DataGridView grid, ThemeColors.Palette p)
	{
		grid.BackgroundColor = p.GridBackground;
		grid.GridColor = p.GridLineColor;

		grid.DefaultCellStyle.BackColor = p.GridCellBackground;
		grid.DefaultCellStyle.ForeColor = p.GridCellForeground;
		grid.DefaultCellStyle.SelectionBackColor = p.SelectionBackground;

		grid.ColumnHeadersDefaultCellStyle.BackColor = p.GridHeaderBackground;
		grid.ColumnHeadersDefaultCellStyle.ForeColor = p.GridHeaderForeground;
		grid.RowHeadersDefaultCellStyle.BackColor = p.GridHeaderBackground;
		grid.RowHeadersDefaultCellStyle.ForeColor = p.GridHeaderForeground;

		if (ThemeManager.Effective == AppTheme.Dark)
			grid.EnableHeadersVisualStyles = false;
		else
			grid.EnableHeadersVisualStyles = true;
	}

	private static void ApplyTabControl(TabControl tab, ThemeColors.Palette p)
	{
		tab.BackColor = p.Background;

		// DoubleBufferedTabControl handles its own tab headers via OwnerDraw.
		// For plain TabControls, use FlatButtons appearance to suppress the
		// OS-drawn 3D border which is always light.
		if (tab is not DoubleBufferedTabControl)
		{
			if (ThemeManager.Effective == AppTheme.Dark)
				tab.Appearance = TabAppearance.FlatButtons;
			else
				tab.Appearance = TabAppearance.Normal;
		}
	}

	/// <summary>
	/// Handles border styling for panels. In dark mode, <see cref="BorderStyle.FixedSingle"/>
	/// and <see cref="BorderStyle.Fixed3D"/> are drawn by the OS in light colours,
	/// so we switch to <see cref="BorderStyle.None"/> and use the panel's own
	/// <c>Paint</c> event to draw a dark border instead.
	/// </summary>
	private static void ApplyPanelBorder(Panel panel, ThemeColors.Palette p)
	{
		if (ThemeManager.Effective == AppTheme.Dark)
		{
			if (panel.BorderStyle != BorderStyle.None)
			{
				_panelBorders[panel] = panel.BorderStyle;
				panel.BorderStyle = BorderStyle.None;
				panel.Paint -= PaintDarkPanelBorder;
				panel.Paint += PaintDarkPanelBorder;
			}
		}
		else
		{
			if (_panelBorders.TryGetValue(panel, out var original))
			{
				panel.Paint -= PaintDarkPanelBorder;
				panel.BorderStyle = original;
				_panelBorders.Remove(panel);
			}
		}
	}

	private static void PaintDarkPanelBorder(object? sender, PaintEventArgs e)
	{
		if (sender is not Panel panel) return;
		var g = e.Graphics;

		using var pen = new Pen(ThemeColors.Dark.MenuBorder);
		var rect = new Rectangle(0, 0, panel.ClientSize.Width - 1, panel.ClientSize.Height - 1);
		g.DrawRectangle(pen, rect);
	}

	private static void ApplyDateTimePicker(DateTimePicker dtp, ThemeColors.Palette p)
	{
		if (ThemeManager.Effective == AppTheme.Dark)
		{
			dtp.BackColor = p.InputBackground;
			dtp.ForeColor = p.InputForeground;
			dtp.CalendarMonthBackground = p.InputBackground;
			dtp.CalendarForeColor = p.InputForeground;
			dtp.CalendarTitleBackColor = p.GridHeaderBackground;
			dtp.CalendarTitleForeColor = p.InputForeground;
			dtp.CalendarTrailingForeColor = p.SecondaryText;
		}
		else
		{
			dtp.ResetBackColor();
			dtp.ResetForeColor();
			dtp.CalendarMonthBackground = SystemColors.Window;
			dtp.CalendarForeColor = SystemColors.ControlText;
			dtp.CalendarTitleBackColor = SystemColors.ActiveCaption;
			dtp.CalendarTitleForeColor = SystemColors.ActiveCaptionText;
			dtp.CalendarTrailingForeColor = SystemColors.GrayText;
		}
	}

	private static void ApplyToolStrip(ToolStrip strip, ThemeColors.Palette p)
	{
		if (ThemeManager.Effective == AppTheme.Dark)
		{
			strip.RenderMode = ToolStripRenderMode.Professional;
			strip.Renderer = new ToolStripProfessionalRenderer(new ThemeColorTable(p));
			strip.BackColor = p.MenuBackground;
			strip.ForeColor = p.MenuForeground;
		}
		else
		{
			strip.RenderMode = ToolStripRenderMode.System;
			strip.Renderer = new ToolStripSystemRenderer();
			strip.ResetBackColor();
			strip.ResetForeColor();
		}

		// Theme any ToolStripComboBox or ToolStripTextBox hosted controls
		foreach (ToolStripItem item in strip.Items)
			ApplyToToolStripItem(item, p);
	}

	private static void ApplyMenuStrip(MenuStrip menu, ThemeColors.Palette p)
	{
		if (ThemeManager.Effective == AppTheme.Dark)
		{
			menu.RenderMode = ToolStripRenderMode.Professional;
			menu.Renderer = new ToolStripProfessionalRenderer(new ThemeColorTable(p));
			menu.BackColor = p.MenuBackground;
			menu.ForeColor = p.MenuForeground;
		}
		else
		{
			menu.RenderMode = ToolStripRenderMode.System;
			menu.Renderer = new ToolStripSystemRenderer();
			menu.ResetBackColor();
			menu.ResetForeColor();
		}

		ApplyToToolStripItems(menu.Items, p);
	}

	private static void ApplyToToolStripItems(ToolStripItemCollection items, ThemeColors.Palette p)
	{
		foreach (ToolStripItem item in items)
		{
			ApplyToToolStripItem(item, p);
			if (item is ToolStripMenuItem menuItem)
				ApplyToToolStripItems(menuItem.DropDownItems, p);
		}
	}

	internal static void ApplyToToolStripItem(ToolStripItem item, ThemeColors.Palette p)
	{
		// Theme the hosted control inside ToolStripControlHost items (e.g. ToolStripComboBox)
		if (item is ToolStripControlHost host && host.Control is ComboBox hostCombo)
		{
			if (ThemeManager.Effective == AppTheme.Dark)
			{
				hostCombo.FlatStyle = FlatStyle.Flat;
				hostCombo.BackColor = p.InputBackground;
				hostCombo.ForeColor = p.InputForeground;
			}
			else
			{
				hostCombo.FlatStyle = FlatStyle.Standard;
				hostCombo.ResetBackColor();
				hostCombo.ResetForeColor();
			}
		}

		if (ThemeManager.Effective == AppTheme.Light)
		{
			item.ResetBackColor();
			item.ResetForeColor();
			return;
		}

		item.BackColor = p.MenuBackground;
		item.ForeColor = p.MenuForeground;
	}

	private static void ApplyStatusStrip(StatusStrip status, ThemeColors.Palette p)
	{
		if (ThemeManager.Effective == AppTheme.Dark)
		{
			status.RenderMode = ToolStripRenderMode.Professional;
			status.Renderer = new ToolStripProfessionalRenderer(new ThemeColorTable(p));
			status.BackColor = p.StatusBarBackground;
			status.ForeColor = p.StatusBarForeground;
		}
		else
		{
			status.RenderMode = ToolStripRenderMode.System;
			status.Renderer = new ToolStripSystemRenderer();
			status.ResetBackColor();
			status.ResetForeColor();
		}

		foreach (ToolStripItem item in status.Items)
		{
			item.ForeColor = ThemeManager.Effective == AppTheme.Dark
				? p.StatusBarForeground
				: SystemColors.ControlText;
		}
	}

	internal static void ApplyToDialogForm(Form form)
	{
		var p = ThemeColors.Get(ThemeManager.Effective == AppTheme.Dark ? "Dark" : "Light");
		form.BackColor = p.Background;
		form.ForeColor = p.ForegroundText;

		form.SuspendLayout();
		foreach (Control child in form.Controls)
			ApplyToControl(child, p);
		form.ResumeLayout(false);
	}

	private sealed class ThemeColorTable : ProfessionalColorTable
	{
		private readonly ThemeColors.Palette _p;

		internal ThemeColorTable(ThemeColors.Palette p) => _p = p;

		public override Color MenuStripGradientBegin => _p.MenuBackground;
		public override Color MenuStripGradientEnd => _p.MenuBackground;
		public override Color MenuItemSelected => _p.MenuHighlightBackground;
		public override Color MenuItemBorder => _p.MenuBorder;
		public override Color MenuItemSelectedGradientBegin => _p.MenuHighlightBackground;
		public override Color MenuItemSelectedGradientEnd => _p.MenuHighlightBackground;
		public override Color MenuBorder => _p.MenuBorder;
		public override Color ToolStripDropDownBackground => _p.MenuBackground;
		public override Color ToolStripBorder => _p.ToolStripBorder;
		public override Color ToolStripContentPanelGradientBegin => _p.MenuBackground;
		public override Color ToolStripContentPanelGradientEnd => _p.MenuBackground;
		public override Color ToolStripPanelGradientBegin => _p.MenuBackground;
		public override Color ToolStripPanelGradientEnd => _p.MenuBackground;
		public override Color ToolStripGradientBegin => _p.MenuBackground;
		public override Color ToolStripGradientEnd => _p.MenuBackground;
		public override Color ImageMarginGradientBegin => _p.MenuBackground;
		public override Color ImageMarginGradientEnd => _p.MenuBackground;
		public override Color ImageMarginGradientMiddle => _p.MenuBackground;
		public override Color CheckBackground => _p.MenuBackground;
		public override Color CheckSelectedBackground => _p.MenuHighlightBackground;
		public override Color CheckPressedBackground => _p.MenuHighlightBackground;
		public override Color ButtonSelectedHighlight => _p.MenuHighlightBackground;
		public override Color ButtonSelectedHighlightBorder => _p.MenuBorder;
		public override Color ButtonCheckedHighlight => _p.MenuHighlightBackground;
		public override Color ButtonCheckedHighlightBorder => _p.MenuBorder;
		public override Color ButtonPressedHighlight => _p.MenuHighlightBackground;
		public override Color ButtonPressedHighlightBorder => _p.MenuBorder;
		public override Color SeparatorDark => _p.MenuBorder;
		public override Color SeparatorLight => _p.MenuBackground;
		public override Color GripDark => _p.MenuBorder;
		public override Color GripLight => _p.MenuBackground;
		public override Color OverflowButtonGradientBegin => _p.MenuBackground;
		public override Color OverflowButtonGradientEnd => _p.MenuBackground;
		public override Color OverflowButtonGradientMiddle => _p.MenuBackground;
	}
}
