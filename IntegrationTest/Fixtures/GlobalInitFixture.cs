//using System.Diagnostics.CodeAnalysis;
//using DotNet.Testcontainers;
//using FluentAssertions;
//using IntegrationTest.Fixtures;
//using IntegrationTest.Utils;
//using Xunit.Abstractions;
//using Xunit.Sdk;

//[assembly: TestFramework($"IntegrationTest.Fixtures.{nameof(GlobalInitFixture)}", "IntegrationTest")]

//namespace IntegrationTest.Fixtures;

//#pragma warning disable S3881 // "IDisposable" should be implemented correctly
//[ExcludeFromCodeCoverage]
//public class GlobalInitFixture : XunitTestFramework, IDisposable
//#pragma warning restore S3881 // "IDisposable" should be implemented correctly
//{
//    public GlobalInitFixture(IMessageSink logger) : base(logger)
//    {
//        AssertionOptions.FormattingOptions.MaxLines = 200;
//        AssertionOptions.FormattingOptions.UseLineBreaks = true;

//        ConsoleLogger.Instance.DebugLogLevelEnabled = true;

//        //var validatorImageBuilder = new ValidatorImageBuilder(logger);
//        //validatorImageBuilder.CreateValidatorImageFromDockerFileAsync().Await();

//        Network = NetworkResolverUtils.GetNetwork();
//    }

//    public static string Network { get; private set; } = null!;

//    public new void Dispose()
//    {
//        base.Dispose();
//        GC.SuppressFinalize(this);
//    }
//}
