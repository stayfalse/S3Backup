using System.Threading.Tasks;

using SimpleInjector;
using SimpleInjector.Diagnostics;

namespace S3Backup.Composition
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var container = new Container();
            container.RegisterSingleton<IArgsParser, ArgsParser>();
            container.RegisterSingleton<IOptionsSource>(() => new OptionsSource(args, container.GetInstance<IArgsParser>()));

            container.RegisterSingleton<ILog<ConsoleLog>, ConsoleLog>();
            container.RegisterSingleton<ILog<FileLog>, FileLog>();
            container.RegisterSingleton<ILog<IAmazonFunctions>, CombinedLog<IAmazonFunctions>>();
            container.RegisterSingleton<ILog<IAmazonFunctionsDryRunChecker>, CombinedLog<IAmazonFunctionsDryRunChecker>>();
            container.RegisterSingleton<ILog<ISynchronizationFunctions>, CombinedLog<ISynchronizationFunctions>>();
            container.RegisterSingleton<ILog<ISynchronization>, CombinedLog<ISynchronization>>();
            container.RegisterDecorator(typeof(ILog<>), typeof(LogMessageDecorator<>), Lifestyle.Singleton);

            container.RegisterSingleton<IAmazonFunctions, UseAmazon>();
            container.RegisterDecorator<IAmazonFunctions, AmazonFunctionsLoggingDecorator>(Lifestyle.Singleton);
            container.RegisterSingleton<IAmazonFunctionsDryRunChecker, AmazonFunctionsDryRunChecker>();
            container.RegisterDecorator<IAmazonFunctionsDryRunChecker, AmazonFunctionsDryRunCheckerLoggingDecorator>(Lifestyle.Singleton);
            container.RegisterSingleton<IAmazonFunctionsForSynchronization, AmazonFunctionsForSynchronization>();

            container.RegisterSingleton<ISynchronizationFunctions, SynchronizationFunctions>();
            container.RegisterDecorator<ISynchronizationFunctions, SynchronizationFunctionsLoggingDecorator>(Lifestyle.Singleton);

            container.RegisterSingleton<ISynchronization, Synchronization>();
            container.RegisterDecorator<ISynchronization, SynchonizationLoggingDecorator>(Lifestyle.Singleton);

            container.Verify(VerificationOption.VerifyAndDiagnose);
            var results = Analyzer.Analyze(container);
            if (results.Length != 0)
            {
                throw new DiagnosticVerificationException(results);
            }

            await container.GetInstance<ISynchronization>().Synchronize().ConfigureAwait(false);
        }
    }
}
