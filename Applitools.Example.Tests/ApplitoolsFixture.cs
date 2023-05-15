using Applitools.VisualGrid;
using Xunit;
using ScreenOrientation = Applitools.VisualGrid.ScreenOrientation;

namespace Applitools.Example.Tests;

/// <summary>
/// Fixture for one-time setup and cleanup
/// </summary>
public class ApplitoolsFixture : IDisposable
{
  #pragma warning disable CS0162

  // Test constants
  public const bool UseUltrafastGrid = true;
  public const bool UseExecutionCloud = false;

  // Test control inputs to read once and share for all tests
  public string? ApplitoolsApiKey;
  public bool Headless;

  // Applitools objects to share for all tests
  public BatchInfo Batch;
  public Configuration Config;
  public EyesRunner Runner;

  /// <summary>
  /// Sets up the configuration for running visual tests.
  /// The configuration is shared by all tests in a test suite, so it belongs in a collection fixture.
  /// If you have more than one test class, then you should abstract this configuration to avoid duplication.
  /// <summary>
  public ApplitoolsFixture()
  {
    // Read the Applitools API key from an environment variable.
    ApplitoolsApiKey = Environment.GetEnvironmentVariable("APPLITOOLS_API_KEY");

    // Read the headless mode setting from an environment variable.
    // Use headless mode for Continuous Integration (CI) execution.
    // Use headed mode for local development.
    Headless = Environment.GetEnvironmentVariable("HEADLESS")?.ToLower() == "true";

    if (UseUltrafastGrid)
    {
      // Create the runner for the Ultrafast Grid.
      // Concurrency refers to the number of visual checkpoints Applitools will perform in parallel.
      // Warning: If you have a free account, then concurrency will be limited to 1.
      Runner = new VisualGridRunner(new RunnerOptions().TestConcurrency(5));
    }
    else
    {
      // Create the Classic runner for local execution.
      Runner = new ClassicRunner();
    }

    // Create a new batch for tests.
    // A batch is the collection of visual checkpoints for a test suite.
    // Batches are displayed in the Eyes Test Manager, so use meaningful names.
    String runnerName = (UseUltrafastGrid) ? "Ultrafast Grid" : "Classic Runner";
    Batch = new BatchInfo($"Example: Selenium C# xUnit.net with the {runnerName}");

    // Create a configuration for Applitools Eyes.
    Config = new Configuration();

    // Set the Applitools API key so test results are uploaded to your account.
    // If you don't explicitly set the API key with this call,
    // then the SDK will automatically read the `APPLITOOLS_API_KEY` environment variable to fetch it.
    Config.SetApiKey(ApplitoolsApiKey);

    // Set the batch for the config.
    Config.SetBatch(Batch);

    if (UseUltrafastGrid)
    {
      // Add 3 desktop browsers with different viewports for cross-browser testing in the Ultrafast Grid.
      // Other browsers are also available, like Edge and IE.
      Config.AddBrowser(800, 600, BrowserType.CHROME);
      Config.AddBrowser(1600, 1200, BrowserType.FIREFOX);
      Config.AddBrowser(1024, 768, BrowserType.SAFARI);

      // Add 2 mobile emulation devices with different orientations for cross-browser testing in the Ultrafast Grid.
      // Other mobile devices are available, including iOS.
      Config.AddDeviceEmulation(DeviceName.Pixel_2, ScreenOrientation.Portrait);
      Config.AddDeviceEmulation(DeviceName.Nexus_10, ScreenOrientation.Landscape);
    }
  }

  /// <summary>
  /// Prints the final summary report for the test suite.
  /// <summary>
  public void Dispose()
  {
    // Close the batch and report visual differences to the console.
    // Note that it forces xUnit.net to wait synchronously for all visual checkpoints to complete.
    TestResultsSummary allTestResults = Runner.GetAllTestResults();
    Console.WriteLine(allTestResults);
  }

  #pragma warning restore CS0162
}

[CollectionDefinition("Applitools collection")]
public class ApplitoolsCollection : ICollectionFixture<ApplitoolsFixture> {}