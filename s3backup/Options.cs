using System;

using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace S3Backup
{
    public sealed class Options
    {
        private readonly string _configFile;

        private bool _illegalArgument = false;

        private int _parallelParts;

        public Options(string[] args)
        {
            var app = new CommandLineApplication();
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
                IllegalArgument = true;
            }
            else
            {
                RecycleAge = ParseArgument(recycleAgeOption.Value(), 366);
                ParallelParts = ParseArgument(parallelPartsOption.Value(), 64);
                if (RecycleAge < 0 || ParallelParts < 0)
                {
                    IllegalArgument = true;
                }

                {
                    LocalPath = localPathOption.Value();
                    BucketName = bucketNameOption.Value();
                    RemotePath = remotePathOption.Value();
                    _configFile = configFileOption.Value();

                    SizeOnly = sizeOnlyOption.HasValue();
                    Purge = purgeOption.HasValue();
                    DryRun = dryRunOption.HasValue();
                }
            }
        }

        public bool IllegalArgument
        {
            get => _illegalArgument;

            set
            {
                _illegalArgument = value;
            }
        }

        public int PartSize => (int)Math.Pow(2, 20) * 250;

        public bool DryRun { get; private set; }

        public bool Purge { get; private set; }

        public bool SizeOnly { get; private set; }

        public int ParallelParts
        {
            get => (_parallelParts == 0) ? 4 : _parallelParts;
            private set => _parallelParts = value;
        }

        public int RecycleAge { get; private set; }

        public string LocalPath { get; private set; }

        public string BucketName { get; private set; }

        public string RemotePath { get; private set; }

        public ClientInformation ClientInfo => string.IsNullOrEmpty(_configFile) ? GetClientInformation("appsettings.json") : GetClientInformation(_configFile);

        private static int ParseArgument(string arg, int edge)
        {
            if (int.TryParse(arg, out var a))
            {
                return (a < 1 || a > edge) ? -1 : a;
            }
            else
            {
                return 0;
            }
        }

        private static ClientInformation GetClientInformation(string configFile)
             => new ConfigurationBuilder().AddJsonFile(configFile).Build().Get<ClientInformation>() ?? new ClientInformation();
    }
}
