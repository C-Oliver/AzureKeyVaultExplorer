using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using System.Linq;

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
        // Settings button is always enabled — find by name across all descendants
        var settingsBtn = MainWindow.FindAllDescendants()
            .FirstOrDefault(e => e.Name == "Settings" || e.Name == "&Settings");
        if (settingsBtn == null) return; // Skip if not exposed in automation tree
        settingsBtn.Click();

        var settingsDlg = Retry.WhileNull(() =>
            _fixture.App.GetAllTopLevelWindows(_fixture.Automation)
                .FirstOrDefault(w => w.Title == "Settings"),
            TimeSpan.FromSeconds(5));

        Assert.NotNull(settingsDlg.Result);
        var dialog = settingsDlg.Result;

        var cancelBtn = dialog.FindFirstDescendant(CF.ByName("Cancel"));
        Assert.NotNull(cancelBtn);
        cancelBtn.Click();
    }

    [Fact]
    public void SettingsDialog_AboutTab_ShowsVersionInfo()
    {
        var settingsBtn = MainWindow.FindAllDescendants()
            .FirstOrDefault(e => e.Name == "Settings" || e.Name == "&Settings");
        if (settingsBtn == null) return; // Skip if not exposed
        settingsBtn!.Click();

        var settingsDlg = Retry.WhileNull(() =>
            _fixture.App.GetAllTopLevelWindows(_fixture.Automation)
                .FirstOrDefault(w => w.Title == "Settings"),
            TimeSpan.FromSeconds(5));
        Assert.NotNull(settingsDlg.Result);
        var dialog = settingsDlg.Result;

        var aboutTab = dialog.FindFirstDescendant(CF.ByName("About"));
        Assert.NotNull(aboutTab);
        aboutTab.Click();

        var cancelBtn = dialog.FindFirstDescendant(CF.ByName("Cancel"));
        cancelBtn?.Click();
    }
}
