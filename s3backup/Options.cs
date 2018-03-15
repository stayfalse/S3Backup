using System;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace S3Backup
{
    class Options
    {
        public bool illegalArgument = false;

        private readonly string configFile;
        private int parallelParts;
        public int PartSize => (int)Math.Pow(2, 20) * 250;

        public bool DryRun { get; set; }
        public bool Purge { get; set; }
        public bool SizeOnly { get; set; }
        public int ParallelParts
        {
            get => (parallelParts == 0) ? 4 : parallelParts;
            set => parallelParts = value;
        }
        public int RecycleAge { get; set; }
        public string LocalPath { get; set; }
        public string BucketName { get; set; }
        public string RemotePath { get; set; }
        public ClientInformation ClientInfo => string.IsNullOrEmpty(configFile) ? GetClientInformation("appsettings.json") : GetClientInformation(configFile);

        private static int ParseArgument(string arg, int edge)
        {
            if (int.TryParse(arg, out int a))
            { 
                return (a < 1 || a > edge) ? -1 : a;
            }
            else
            { return 0; }
        }

        private static ClientInformation GetClientInformation(string configFile)
             => new ConfigurationBuilder().AddJsonFile(configFile).Build().Get<ClientInformation>() ?? new ClientInformation();

        public Options(string[] args)
        {
            CommandLineApplication app = new CommandLineApplication();
            app.Name = "S3Backup";
            app.HelpOption("-?|-h|--help");

            var localPathOption = app.Option("-l  | --local", "set Local Path", CommandOptionType.SingleValue);
            var bucketNameOption = app.Option("-b  | --bucket", "set Bucket Name", CommandOptionType.SingleValue);
            var remotePathOption = app.Option("-rp | --remotePath", "set Remote Path", CommandOptionType.SingleValue);
            var configFileOption = app.Option("-c  | --config", "set AWS config file path", CommandOptionType.SingleValue);

            var recycleAgeOption = app.Option("-ra | --recycleAge", "set recycle age in days (default is not set)", CommandOptionType.SingleValue);
            var parallelPartsOption = app.Option("-pp | --parallelParts", "ParallelParts", CommandOptionType.SingleValue);

            var sizeOnlyOption = app.Option("-s  | --sizeonly", "do not compare checksums", CommandOptionType.NoValue);
            var purgeOption = app.Option("-p  | --purge", "purge bucket contents before synchronizing (CAUTION!)", CommandOptionType.NoValue);
            var dryRunOption = app.Option("-d  | --dryRun", "DryRun", CommandOptionType.NoValue);

            app.Execute(args);
            
            if (!localPathOption.HasValue() || !bucketNameOption.HasValue() || !remotePathOption.HasValue())
            {
                illegalArgument = true;
            }
            else
            {
                RecycleAge = ParseArgument(recycleAgeOption.Value(), 366);
                ParallelParts = ParseArgument(parallelPartsOption.Value(), 64);
                if ( RecycleAge < 0 || ParallelParts < 0 )
                {
                    illegalArgument = true;
                }
                {
                        LocalPath = localPathOption.Value();
                        BucketName = bucketNameOption.Value();
                        RemotePath = remotePathOption.Value();
                        configFile = configFileOption.Value();

                        SizeOnly = sizeOnlyOption.HasValue();
                        Purge = purgeOption.HasValue();
                        DryRun = dryRunOption.HasValue();
                }
            }
        }
    }
}
