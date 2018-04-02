using System;

using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace S3Backup
{
    public class ArgsParser : IArgsParser
    {
        private string _configFile;

        private bool _illegalArgument = false;

        private int _parallelParts;

        private bool IllegalArgument
        {
            get => _illegalArgument;
            set => _illegalArgument = value;
        }

        private int PartSize => (int)Math.Pow(2, 20) * 250;

        private bool DryRun { get; set; }

        private bool Purge { get; set; }

        private bool SizeOnly { get; set; }

        private int ParallelParts
        {
            get => (_parallelParts == 0) ? 4 : _parallelParts;
            set => _parallelParts = value;
        }

        private int RecycleAge { get; set; }

        private string LocalPath { get; set; }

        private string BucketName { get; set; }

        private string RemotePath { get; set; }

        private string ConfigFile
        {
            get => string.IsNullOrEmpty(_configFile) ? "appsettings.json" : _configFile;
            set => _configFile = value;
        }

        private ClientInformation ClientInformation => ParseClient();

        public Options Parse(string[] args)
        {
            var app = new CommandLineApplication { Name = "S3Backup" };
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
                    ConfigFile = configFileOption.Value();

                    SizeOnly = sizeOnlyOption.HasValue();
                    Purge = purgeOption.HasValue();
                    DryRun = dryRunOption.HasValue();

                    RecycleAge = RecycleAge;
                    ParallelParts = ParallelParts;
                }
            }

            IllegalArgument = IllegalArgument;
            return new Options(IllegalArgument, DryRun, SizeOnly, Purge, BucketName, LocalPath, RemotePath, PartSize, RecycleAge, ParallelParts, ClientInformation);
        }

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

        private ClientInformation ParseClient() => new ConfigurationBuilder()
            .AddJsonFile(ConfigFile)
            .Build()
            .Get<ClientInformation>() ?? new ClientInformation();
    }
}
