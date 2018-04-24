using System;
using System.Threading.Tasks;

using S3Backup.AmazonS3Functionality;
using S3Backup.Logging;
using S3Backup.SynchronizationImplementation;

using SimpleInjector;
using SimpleInjector.Diagnostics;

namespace S3Backup.Composition
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var container = new Container();

            container.RegisterSingleton<IConsoleLog, ConsoleLog>();
            container.RegisterSingleton<IFileLog, FileLog>();
            container.Register(typeof(ILog<>), typeof(CombinedLog<>), Lifestyle.Singleton);
            container.RegisterDecorator(typeof(ILog<>), typeof(LogMessageDecorator<>), Lifestyle.Singleton);

            container.RegisterSingleton<IArgsParser, ArgsParser>();
            container.RegisterDecorator<IArgsParser, ArgsParserExceptionHandler>(Lifestyle.Singleton);
            container.RegisterSingleton<IOptionsSource>(() => new OptionsSource(args, container.GetInstance<IArgsParser>()));

            container.RegisterSingleton<IAmazonFunctions, UseAmazon>();
            container.RegisterDecorator<IAmazonFunctions, AmazonFunctionsLoggingDecorator>(Lifestyle.Singleton);
            container.RegisterDecorator<IAmazonFunctions, AmazonFunctionsReliabilityDecorator>(Lifestyle.Singleton);
            container.RegisterSingleton<IAmazonFunctionsDryRunChecker, AmazonFunctionsDryRunChecker>();
            container.RegisterDecorator<IAmazonFunctionsDryRunChecker, AmazonFunctionsDryRunCheckerLoggingDecorator>(Lifestyle.Singleton);
            container.RegisterSingleton<IAmazonFunctionsForSynchronization, AmazonFunctionsForSynchronization>();

            container.RegisterSingleton<ISynchronizationFunctions, SynchronizationFunctions>();
            container.RegisterDecorator<ISynchronizationFunctions, SynchronizationFunctionsLoggingDecorator>(Lifestyle.Singleton);

            container.RegisterSingleton<ISynchronization, Synchronization>();
            container.RegisterDecorator<ISynchronization, SynchronizationLoggingDecorator>(Lifestyle.Singleton);

            try
            {
                container.Verify(VerificationOption.VerifyAndDiagnose);
                var results = Analyzer.Analyze(container);
                if (results.Length != 0)
                {
                    throw new DiagnosticVerificationException(results);
                }

                await container.GetInstance<ISynchronization>().Synchronize().ConfigureAwait(false);
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"Unhandled exception occurred: {exception.Message}");
                return -1;
            }
        }
    }
}
