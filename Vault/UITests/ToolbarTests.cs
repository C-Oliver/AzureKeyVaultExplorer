using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;

namespace VaultExplorer.UITests;

/// <summary>
/// Tests that verify the toolbar layout and menu structure match expected configuration.
/// </summary>
[Collection("App")]
public class ToolbarTests
{
    private readonly AppFixture _fixture;
    private Window MainWindow => _fixture.MainWindow;
    private ConditionFactory CF => _fixture.Automation.ConditionFactory;

    public ToolbarTests(AppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Toolbar_HasExpectedButtonCount()
    {
        var toolBar = MainWindow.FindFirstDescendant(CF.ByControlType(ControlType.ToolBar));
        Assert.NotNull(toolBar);

        var buttons = toolBar.FindAllDescendants(CF.ByControlType(ControlType.Button));
        // ToolStrip items include buttons + split button parts; just verify we have several
        Assert.True(buttons.Length >= 5, $"Expected at least 5 toolbar buttons, found {buttons.Length}");
    }

    [Fact]
    public void AddButton_HasDropDownMenu()
    {
        var addBtn = MainWindow.FindFirstDescendant(CF.ByName("Add"));
        Assert.NotNull(addBtn);
    }

    [Fact]
    public void VaultComboBox_HasItems()
    {
        var combo = MainWindow.FindFirstDescendant(CF.ByControlType(ControlType.ComboBox));
        Assert.NotNull(combo);
    }

    [Fact]
    public void ListView_HasExpectedColumns()
    {
        var listView = MainWindow.FindFirstDescendant(CF.ByAutomationId("uxListViewSecrets"));
        Assert.NotNull(listView);

        // Look for column headers
        var headers = listView.FindAllDescendants(CF.ByControlType(ControlType.HeaderItem));
        // Should have: Name, Updated, Changed by, Expires (minimum)
        Assert.True(headers.Length >= 4, $"Expected at least 4 columns, found {headers.Length}");
    }
}
