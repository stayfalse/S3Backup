using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using SimpleInjector;

namespace S3Backup
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var options = new Options(args);
            if (!options.IllegalArgument)
            {
                var clientInfo = GetClientInformation(options.ConfigFile);
                var container = GetContainer(clientInfo, options.BucketName);
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

        private static Container GetContainer(ClientInformation clientinfo, string bucketName)
        {
            var container = new Container();
            container.Register<IAmazonFunctions>(() => new UseAmazon(bucketName, clientinfo));
            container.RegisterDecorator<IAmazonFunctions, AmazonFunctionsLoggingDecorator>();
            container.Register<ISynchronizationFunctions, SynchronizationFunctions>();
            container.RegisterDecorator<ISynchronizationFunctions, SynchronizationFunctionsLoggingDecorator>();
            return container;
        }
    }
}
