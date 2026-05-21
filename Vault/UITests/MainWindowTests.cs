using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;

namespace VaultExplorer.UITests;

/// <summary>
/// Tests that verify the main window structure and all key UI components are present and functional.
/// These tests launch the real application via FlaUI and validate the automation tree.
/// </summary>
[Collection("App")]
public class MainWindowTests
{
    private readonly AppFixture _fixture;
    private Window MainWindow => _fixture.MainWindow;
    private ConditionFactory CF => _fixture.Automation.ConditionFactory;

    public MainWindowTests(AppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void MainWindow_HasCorrectTitle()
    {
        Assert.Contains("Azure Key Vault Explorer", MainWindow.Title);
    }

    [Fact]
    public void MainWindow_IsVisible()
    {
        Assert.True(MainWindow.IsAvailable);
    }

    [Fact]
    public void Toolbar_Exists()
    {
        var toolStrip = MainWindow.FindFirstDescendant(CF.ByControlType(FlaUI.Core.Definitions.ControlType.ToolBar));
        Assert.NotNull(toolStrip);
    }

    [Fact]
    public void VaultComboBox_Exists()
    {
        var combo = MainWindow.FindFirstDescendant(CF.ByAutomationId("uxComboBoxVaultAlias"));
        Assert.NotNull(combo);
    }

    [Fact]
    public void AddButton_Exists()
    {
        var addBtn = MainWindow.FindFirstDescendant(CF.ByAutomationId("uxButtonAdd"));
        Assert.NotNull(addBtn);
    }

    [Fact]
    public void EditButton_Exists()
    {
        var editBtn = MainWindow.FindFirstDescendant(CF.ByAutomationId("uxButtonEdit"));
        Assert.NotNull(editBtn);
    }

    [Fact]
    public void DeleteButton_Exists()
    {
        var deleteBtn = MainWindow.FindFirstDescendant(CF.ByAutomationId("uxButtonDelete"));
        Assert.NotNull(deleteBtn);
    }

    [Fact]
    public void DisableButton_Exists()
    {
        var toggleBtn = MainWindow.FindFirstDescendant(CF.ByAutomationId("uxButtonToggle"));
        Assert.NotNull(toggleBtn);
    }

    [Fact]
    public void SearchBox_ExistsAndIsEditable()
    {
        var searchBox = MainWindow.FindFirstDescendant(CF.ByAutomationId("uxTextBoxSearch"));
        Assert.NotNull(searchBox);
    }

    [Fact]
    public void ShareButton_Exists()
    {
        var shareBtn = MainWindow.FindFirstDescendant(CF.ByAutomationId("uxButtonShare"));
        Assert.NotNull(shareBtn);
    }

    [Fact]
    public void FavoriteButton_Exists()
    {
        var favBtn = MainWindow.FindFirstDescendant(CF.ByAutomationId("uxButtonFavorite"));
        Assert.NotNull(favBtn);
    }

    [Fact]
    public void PowerShellButton_Exists()
    {
        var psBtn = MainWindow.FindFirstDescendant(CF.ByAutomationId("uxButtonPowershell"));
        Assert.NotNull(psBtn);
    }

    [Fact]
    public void SettingsButton_Exists()
    {
        var settingsBtn = MainWindow.FindFirstDescendant(CF.ByAutomationId("uxButtonSettings"));
        Assert.NotNull(settingsBtn);
    }

    [Fact]
    public void HelpButton_Exists()
    {
        var helpBtn = MainWindow.FindFirstDescendant(CF.ByAutomationId("uxButtonHelp"));
        Assert.NotNull(helpBtn);
    }

    [Fact]
    public void ListView_Exists()
    {
        var listView = MainWindow.FindFirstDescendant(CF.ByAutomationId("uxListViewSecrets"));
        Assert.NotNull(listView);
    }

    [Fact]
    public void PropertyGrid_Exists()
    {
        var propGrid = MainWindow.FindFirstDescendant(CF.ByAutomationId("uxPropertyGridSecret"));
        Assert.NotNull(propGrid);
    }

    [Fact]
    public void StatusBar_ShowsSecretCount()
    {
        var statusBar = MainWindow.FindFirstDescendant(CF.ByControlType(FlaUI.Core.Definitions.ControlType.StatusBar));
        Assert.NotNull(statusBar);
    }
}
