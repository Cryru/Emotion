using Emotion.Core.Utility.Coroutines;
using Emotion.Testing.VisualStudioAdapter;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Capabilities.TestFramework;
using Microsoft.Testing.Platform.Requests;
using System.Linq;
using System.Threading.Tasks;

namespace Emotion.Testing;

public static partial class TestExecutor
{
    public static async Task TestApplicationMain(string[] args, Configurator? config = null)
    {
        // Check if sub process.
#if AUTOBUILD
        if (CommandLineParser.FindArgument(args, "SubTestLinkId=", out string? linkId))
        {
            SubProcessEvaluation(linkId);
            return;
        }
#endif

        config = Init(args, config);

        ITestApplicationBuilder builder = await TestApplication.CreateBuilderAsync(args);
        builder.RegisterTestFramework(
            _ => new TestFrameworkCapabilities(Array.Empty<ITestFrameworkCapability>()),
            (_, _) => new VSTestAdapter(config)
        );

        using ITestApplication app = await builder.BuildAsync();
        await app.RunAsync();
    }

    internal static TestDiscoveryPair VSDiscoverTests(ITestExecutionFilter filter)
    {
        HashSet<string>? filterIds = null;
        if (filter is TestNodeUidListFilter uidFilter)
            filterIds = new(uidFilter.TestNodeUids.Select(x => x.Value));

        TestDiscoveryPair discoveredTests = DiscoverTests();

        if (filterIds != null)
            discoveredTests.TestsFromScenes.RemoveAll(x => !filterIds.Contains(x.Id));

        if (filterIds != null)
            discoveredTests.TestsFromClasses.RemoveAll(x => !filterIds.Contains(x.Id));

        return discoveredTests;
    }

    internal static void InternalExecuteTests(Configurator config, TestDiscoveryPair tests)
    {
        Engine.Start(config, () => EngineInitCoroutineAsync(tests));
    }

    private static IEnumerator EngineInitCoroutineAsync(TestDiscoveryPair testsToRun)
    {
        IRoutineWaiter testRoutine = Engine.Jobs.Add(RunDiscoveredTestsRoutineAsync(testsToRun));
        yield return testRoutine;
        Engine.Quit();
    }
}
