using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using SimpleInjector;
using SimpleInjector.Diagnostics;

namespace S3Backup
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var container = GetContainer(args);

            var options = container.GetInstance<IOptionsSource>();
            if (!options.Options.IllegalArgument)
            {
                var synchronization = new Synchronization(options, container.GetInstance<IAmazonFunctions>(), container.GetInstance<ISynchronizationFunctions>());
                await synchronization.Synchronize().ConfigureAwait(false);
            }
            else
            {
                Log.PutOut($"Synchronization can not be started in case of incorrect command line arguments");
            }
        }

        private static ClientInformation GetClientInformation(string configFile)
            => new ConfigurationBuilder()
            .AddJsonFile(configFile)
            .Build()
            .Get<ClientInformation>() ?? new ClientInformation();

        private static Container GetContainer(string[] args)
        {
            var container = new Container();
            container.Register<IArgsParser, ArgsParser>();
            container.RegisterSingleton<IOptionsSource>(() => new OptionsSource(args, container.GetInstance<IArgsParser>()));
            container.Register<IAmazonFunctions, UseAmazon>();
            container.RegisterDecorator<IAmazonFunctions, AmazonFunctionsLoggingDecorator>();
            container.Register<ISynchronizationFunctions, SynchronizationFunctions>();
            container.RegisterDecorator<ISynchronizationFunctions, SynchronizationFunctionsLoggingDecorator>();
            container.Verify(VerificationOption.VerifyAndDiagnose);
            var results = Analyzer.Analyze(container);
            if (results.Length != 0)
            {
                throw new DiagnosticVerificationException(results);
            }

            return container;
        }
    }
}
