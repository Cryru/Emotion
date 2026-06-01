using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestFramework;
using Microsoft.Testing.Platform.Requests;
using Microsoft.Testing.Platform.TestHost;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Emotion.Testing.VisualStudioAdapter;

[RequiresUnreferencedCode("Testing library")]
public class VSTestAdapter : ITestFramework, IDataProducer
{
    public string Uid => "Emotion.Testing";
    public string Version => "1.0.0";
    public string DisplayName => "Emotion Test Framework";
    public string Description => "Runs Emotion's built-in engine tests.";
    public Type[] DataTypesProduced => new[] {
        typeof(TestNodeUpdateMessage),
        typeof(SessionFileArtifact)
    };

    private Configurator _config;

    public VSTestAdapter(Configurator emotionConfig)
    {
        _config = emotionConfig;
    }

    public Task<CreateTestSessionResult> CreateTestSessionAsync(CreateTestSessionContext context)
    {
        return Task.FromResult(new CreateTestSessionResult { IsSuccess = true });
    }

    public Task<CloseTestSessionResult> CloseTestSessionAsync(CloseTestSessionContext context)
    {
        return Task.FromResult(new CloseTestSessionResult { IsSuccess = true });
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task ExecuteRequestAsync(ExecuteRequestContext context)
    {
        try
        {
            if (context.Request is DiscoverTestExecutionRequest discoverRequest)
            {
                await PublishDiscoveredTests(context, discoverRequest.Filter, discoverRequest.Session);
            }
            else if (context.Request is RunTestExecutionRequest runRequest)
            {
                await RunTests(context, runRequest);
            }
            else
            {
                throw new NotSupportedException($"Unsupported Emotion test request {context.Request.GetType()}.");
            }
        }
        finally
        {
            context.Complete();
        }
    }

    public Task<bool> IsEnabledAsync()
    {
        return Task.FromResult(true);
    }

    #region Functionality

    [RequiresUnreferencedCode("Calls Emotion.Testing.TestExecutor.VSDiscoverTests(Type)")]
    private async Task<TestDiscoveryPair> PublishDiscoveredTests(ExecuteRequestContext context, ITestExecutionFilter filter, TestSessionContext session)
    {
        TestDiscoveryPair tests = TestExecutor.VSDiscoverTests(filter);
        List<EmotionTestDescription> testsFlattened = [.. tests.TestsFromClasses, .. tests.TestsFromScenes];
        foreach (EmotionTestDescription test in testsFlattened)
        {
            await context.MessageBus.PublishAsync(
                this,
                new TestNodeUpdateMessage(
                    session.SessionUid,
                    CreateTestNode(test, DiscoveredTestNodeStateProperty.CachedInstance)
                )
            );
        }
        return tests;
    }

    private async Task RunTests(ExecuteRequestContext context, RunTestExecutionRequest request)
    {
        TestDiscoveryPair tests = await PublishDiscoveredTests(context, request.Filter, request.Session);

        TestExecutor.InternalExecuteTests(_config, tests);

        ConcurrentBag<EmotionTestResult> testResults = TestExecutor.LastTestResults;
        Dictionary<EmotionTestDescription, EmotionTestResult> resultMapping = testResults.ToDictionary((x) => x.Description);
        List<EmotionTestDescription> testsFlattened = [.. tests.TestsFromClasses, .. tests.TestsFromScenes];
        DateTimeOffset now = DateTimeOffset.UtcNow;
        foreach (EmotionTestDescription test in testsFlattened)
        {
            // Add tests that didnt run as missing.
            if (!resultMapping.TryGetValue(test, out EmotionTestResult? result))
            {
                result = new EmotionTestResult(test);
                result.SetFailed("Emotion test runner exited before reporting this test result.");
            }

            await context.MessageBus.PublishAsync(
                this,
                new TestNodeUpdateMessage(
                    request.Session.SessionUid,
                    CreateTestNode(result.Description,
                    CreateStateProperty(result), result)
                )
             );
        }
    }

    #endregion

    #region Helpers

    private static TestMethodIdentifierProperty CreateMethodIdentifier(EmotionTestDescription test)
    {
        MethodInfo method = test.Method;
        Type declaringType = test.DeclaringType;
        ParameterInfo[] parameters = method.GetParameters();
        string[] parameterTypeNames = new string[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            parameterTypeNames[i] = parameters[i].ParameterType.FullName ?? parameters[i].ParameterType.Name;
        }

        return new TestMethodIdentifierProperty(
            declaringType.Assembly.FullName ?? declaringType.Assembly.GetName().Name ?? "",
            declaringType.Namespace ?? "",
            declaringType.FullName ?? declaringType.Name,
            method.Name,
            0,
            parameterTypeNames,
            method.ReturnType.FullName ?? method.ReturnType.Name
        );
    }

    private static TestNode CreateTestNode(EmotionTestDescription test, IProperty state, EmotionTestResult? result = null)
    {
        var properties = new List<IProperty>
        {
            state,
            CreateMethodIdentifier(test)
        };

        if (!string.IsNullOrEmpty(test.FilePath) && test.LineNumber > 0)
        {
            LinePosition position = new(test.LineNumber, 0);
            properties.Add(new TestFileLocationProperty(test.FilePath, new LinePositionSpan(position, position)));
        }

        if (result != null)
        {
            properties.Add(new TimingProperty(new TimingInfo(result.StartTime, result.EndTime, result.Duration)));
        }

        return new TestNode
        {
            Uid = new TestNodeUid(test.Id),
            DisplayName = test.DisplayName,
            Properties = new PropertyBag(properties)
        };
    }

    private static IProperty CreateStateProperty(EmotionTestResult result)
    {
        if (result.Passed) return PassedTestNodeStateProperty.CachedInstance;

        Exception exception = CreateFailureException(result);
        if (exception is Assert.TestAssertException) return new FailedTestNodeStateProperty(exception);

        return new ErrorTestNodeStateProperty(exception);
    }

    private static Exception CreateFailureException(EmotionTestResult result)
    {
        string message = result.ErrorMessage ?? result.Exception?.Message ?? "Emotion test failed.";
        if (!string.IsNullOrEmpty(TestExecutor.TestRunFolder))
            message += $"{Environment.NewLine}{Environment.NewLine}Emotion TestResults:{Environment.NewLine}{TestExecutor.TestRunFolder}";

        if (result.Exception is Assert.TestAssertException)
            return new Assert.TestAssertException(message);

        return new Exception(message, result.Exception);
    }

    #endregion
}
