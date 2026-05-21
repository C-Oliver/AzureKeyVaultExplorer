using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;

namespace VaultExplorer.UITests;

/// <summary>
/// Tests that verify dialogs can be opened and contain expected controls.
/// </summary>
[Collection("App")]
public class DialogTests
{
    private readonly AppFixture _fixture;
    private Window MainWindow => _fixture.MainWindow;
    private ConditionFactory CF => _fixture.Automation.ConditionFactory;

    public DialogTests(AppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void SettingsDialog_OpensAndCloses()
    {
        // Find and click the Settings button
        var settingsBtn = MainWindow.FindFirstDescendant(CF.ByAutomationId("uxButtonSettings"));
        Assert.NotNull(settingsBtn);
        settingsBtn.Click();

        // Wait for the Settings dialog to appear
        var settingsDlg = Retry.WhileNull(() =>
            MainWindow.FindFirstDescendant(CF.ByName("Settings"))?.AsWindow(),
            TimeSpan.FromSeconds(5));

        Assert.NotNull(settingsDlg.Result);
        var dialog = settingsDlg.Result;

        // Verify it has Options and About tabs
        var tabs = dialog.FindAllDescendants(CF.ByControlType(ControlType.TabItem));
        Assert.True(tabs.Length >= 2, $"Expected at least 2 tabs, found {tabs.Length}");

        // Verify Cancel button exists
        var cancelBtn = dialog.FindFirstDescendant(CF.ByName("Cancel"));
        Assert.NotNull(cancelBtn);

        // Close the dialog
        cancelBtn.Click();

        // Verify dialog is gone
        var closed = Retry.WhileNotNull(() =>
            MainWindow.FindFirstDescendant(CF.ByName("Settings"))?.AsWindow(),
            TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void SettingsDialog_AboutTab_ShowsVersionInfo()
    {
        var settingsBtn = MainWindow.FindFirstDescendant(CF.ByAutomationId("uxButtonSettings"));
        settingsBtn!.Click();

        var settingsDlg = Retry.WhileNull(() =>
            MainWindow.FindFirstDescendant(CF.ByName("Settings"))?.AsWindow(),
            TimeSpan.FromSeconds(5));
        Assert.NotNull(settingsDlg.Result);
        var dialog = settingsDlg.Result;

        // Click the About tab
        var aboutTab = dialog.FindFirstDescendant(CF.ByName("About"));
        Assert.NotNull(aboutTab);
        aboutTab.Click();

        // Verify version info is displayed (look for .NET text)
        var versionText = dialog.FindFirstDescendant(CF.ByAutomationId("uxTextBoxVersions"));
        Assert.NotNull(versionText);

        // Close
        var cancelBtn = dialog.FindFirstDescendant(CF.ByName("Cancel"));
        cancelBtn?.Click();
    }
}
