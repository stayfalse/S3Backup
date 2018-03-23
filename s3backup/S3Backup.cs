using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

namespace S3Backup
{
    public static class S3Backup
    {
        public static async Task Main(string[] args)
        {
            var options = new Options(args);
            if (!options.IllegalArgument)
            {
                var clientInfo = GetClientInformation(options.ConfigFile);
                var synchronization = new Synchronization(options, GetAmazonFunctions(options.BucketName, clientInfo));
                await synchronization.Synchronize().ConfigureAwait(false);
            }
            else
            {
                Log.PutOut("Synchronization can not be started in case of incorrect command line arguments");
            }
        }

        private static ClientInformation GetClientInformation(string configFile)
            => new ConfigurationBuilder()
            .AddJsonFile(configFile)
            .Build()
            .Get<ClientInformation>() ?? new ClientInformation();

        private static IAmazonFunctions GetAmazonFunctions(string bucketName, ClientInformation clientInfo)
        {
            if (clientInfo is null)
            {
                throw new ArgumentNullException(nameof(clientInfo));
            }

            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            return new UseAmazon(bucketName, clientInfo);
        }
    }
}
