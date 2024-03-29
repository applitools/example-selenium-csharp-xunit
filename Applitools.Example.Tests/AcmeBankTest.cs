using Applitools.Selenium;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System.Drawing;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Applitools.Example.Tests;

/// <summary>
/// Tests for the ACME Bank demo app.
/// </summary>
[Collection("Applitools collection")]
public class AcmeBankTest : IDisposable
{
    #pragma warning disable CS0162

    // Test-specific objects
    private WebDriver Driver;
    private Eyes Eyes;

    /// <summary>
    /// Sets up each test with its own ChromeDriver and Applitools Eyes objects.
    /// <summary>
    public AcmeBankTest(ApplitoolsFixture fixture, ITestOutputHelper output)
    {
        // Get the current test name
        #pragma warning disable CS8600
        #pragma warning disable CS8602
        var type = output.GetType();
        var testMember = type.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic);
        var test = (ITest)testMember.GetValue(output);
        #pragma warning restore CS8600
        #pragma warning restore CS8602

        // Open the browser with the ChromeDriver instance.
        // Even though this test will run visual checkpoints on different browsers in the Ultrafast Grid,
        // it still needs to run the test one time locally to capture snapshots.
        ChromeOptions options = new ChromeOptions();
        if (fixture.Headless) options.AddArgument("headless");
        
        if (ApplitoolsFixture.UseExecutionCloud)
        {
            // Open the browser remotely in the Execution Cloud.
            Driver = new RemoteWebDriver(new Uri(Eyes.GetExecutionCloudUrl()), options);
        }
        else
        {
            // Create a local WebDriver.
            Driver = new ChromeDriver(options);
        }

        // Set an implicit wait of 10 seconds.
        // For larger projects, use explicit waits for better control.
        // https://www.selenium.dev/documentation/webdriver/waits/
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

        // Create the Applitools Eyes object connected to the runner and set its configuration.
        Eyes = new Eyes(fixture.Runner);
        Eyes.SetConfiguration(fixture.Config);
        Eyes.SaveNewTests = true;

        // Open Eyes to start visual testing.
        // It is a recommended practice to set all four inputs:
        Eyes.Open(
        
            // WebDriver object to "watch".
            Driver,
            
            // The name of the application under test.
            // All tests for the same app should share the same app name.
            // Set this name wisely: Applitools features rely on a shared app name across tests.
            "ACME Bank Web App",
            
            // The name of the test case for the given application.
            // Additional unique characteristics of the test may also be specified as part of the test name,
            // such as localization information ("Home Page - EN") or different user permissions ("Login by admin").
            test?.TestCase.TestMethod.Method.Name,
            
            // The viewport size for the local browser.
            // Eyes will resize the web browser to match the requested viewport size.
            // This parameter is optional but encouraged in order to produce consistent results.
            new Size(1200, 600));
    }

    /// <summary>
    /// This test covers login for the Applitools demo site, which is a dummy banking app.
    /// The interactions use typical Selenium WebDriver calls,
    /// but the verifications use one-line snapshot calls with Applitools Eyes.
    /// If the page ever changes, then Applitools will detect the changes and highlight them in the Eyes Test Manager.
    /// Traditional assertions that scrape the page for text values are not needed here.
    /// <summary>
    [Fact]
    public void LogIntoBankAccount()
    {
        // Load the login page.
        Driver.Navigate().GoToUrl("https://demo.applitools.com");

        // Verify the full login page loaded correctly.
        Eyes.Check(Target.Window().Fully().WithName("Login page"));

        // Perform login.
        Driver.FindElement(By.Id("username")).SendKeys("applibot");
        Driver.FindElement(By.Id("password")).SendKeys("I<3VisualTests");
        Driver.FindElement(By.Id("log-in")).Click();

        // Verify the full main page loaded correctly.
        // This snapshot uses LAYOUT match level to avoid differences in closing time text.
        Eyes.Check(Target.Window().Fully().WithName("Main page").Layout());
    }

    /// <summary>
    /// Concludes the test by quitting the browser and closing Eyes.
    /// <summary>
    public void Dispose()
    {
        // Close Eyes to tell the server it should display the results.
        Eyes.Close();

        // Quit the WebDriver instance.
        Driver.Quit();

        // Warning: `Eyes.CloseAsync()` will NOT wait for visual checkpoints to complete.
        // You will need to check the Eyes Test Manager for visual results per checkpoint.
        // Note that "unresolved" and "failed" visual checkpoints will not cause the xUnit.net test to fail.

        // If you want the xUnit.net test to wait synchronously for all checkpoints to complete, then use `Eyes.Close()`.
        // If any checkpoints are unresolved or failed, then `Eyes.Close()` will make the xUnit.net test fail.
    }

    #pragma warning restore CS0162
}
