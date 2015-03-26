using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

/// <summary>
/// Wraps test cases for FactAttribute and TheoryAttribute so the test case runs in the STA Thread
/// </summary>
[DebuggerDisplay(@"\{ class = {TestMethod.TestClass.Class.Name}, method = {TestMethod.Method.Name}, display = {DisplayName}, skip = {SkipReason} \}")]
public class STATestCase : LongLivedMarshalByRefObject, IXunitTestCase
{
    IXunitTestCase testCase;

    public STATestCase(IXunitTestCase testCase)
    {
        this.testCase = testCase;
    }

    /// <summary/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Called by the de-serializer", error: true)]
    public STATestCase()
    {
    }

    public IMethodInfo Method
    {
        get { return testCase.Method; }
    }

    public Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments,
        ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
    {
        var tcs = new TaskCompletionSource<RunSummary>();
        var thread = new Thread(() =>
        {
            try
            {
                var testCaseTask = testCase.RunAsync(diagnosticMessageSink, messageBus, constructorArguments, aggregator,
                    cancellationTokenSource);
                tcs.SetResult(testCaseTask.Result);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return tcs.Task;
    }

    public string DisplayName
    {
        get { return testCase.DisplayName; }
    }

    public string SkipReason
    {
        get { return testCase.SkipReason; }
    }

    public ISourceInformation SourceInformation
    {
        get { return testCase.SourceInformation; }
        set { testCase.SourceInformation = value; }
    }

    public ITestMethod TestMethod
    {
        get { return testCase.TestMethod; }
    }

    public object[] TestMethodArguments
    {
        get { return testCase.TestMethodArguments; }
    }

    public Dictionary<string, List<string>> Traits
    {
        get { return testCase.Traits; }
    }

    public string UniqueID
    {
        get { return testCase.UniqueID; }
    }

    public void Deserialize(IXunitSerializationInfo info)
    {
        testCase = info.GetValue<IXunitTestCase>("InnerTestCase");
    }

    public void Serialize(IXunitSerializationInfo info)
    {
        info.AddValue("InnerTestCase", testCase);
    }
}