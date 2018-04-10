using System;

using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace S3Backup.Composition
{
    public class ArgsParser : IArgsParser
    {
        private string _configFile;

        private bool _illegalArgument = false;

        private int _parallelParts;

        private int _partSize;

        private PartSize PartSize
        {
            get => (_partSize == 0) ? new PartSize((int)Math.Pow(2, 20) * 250) : new PartSize((int)Math.Pow(2, 20) * _partSize);
            set => _partSize = value;
        }

        private ParallelParts ParallelParts
        {
            get => (_parallelParts == 0) ? new ParallelParts(4) : new ParallelParts(_parallelParts);
            set => _parallelParts = value;
        }

        private RecycleAge RecycleAge { get; set; }

        private LocalPath LocalPath { get; set; }

        private BucketName BucketName { get; set; }

        private RemotePath RemotePath { get; set; }

        private string ConfigFile
        {
            get => string.IsNullOrEmpty(_configFile) ? "appsettings.json" : _configFile;
            set => _configFile = value;
        }

        private ClientInformation ClientInformation => GetClient();

        public (Options, AmazonOptions) Parse(string[] args)
        {
            var app = new CommandLineApplication { Name = "S3Backup" };
            app.HelpOption("-?|-h|--help");

            var localPathOption = app.Option("-l  | --local", "set Local Path", CommandOptionType.SingleValue);
            var bucketNameOption = app.Option("-b  | --bucket", "set Bucket Name", CommandOptionType.SingleValue);
            var remotePathOption = app.Option("-rp | --remotePath", "set Remote Path", CommandOptionType.SingleValue);
            var configFileOption = app.Option("-c  | --config", "set AWS config file path", CommandOptionType.SingleValue);

            var recycleAgeOption = app.Option("-ra | --recycleAge", "set recycle age in days (default is not set)", CommandOptionType.SingleValue);
            var parallelPartsOption = app.Option("-pp | --parallelParts", "ParallelParts (defauls is 4)", CommandOptionType.SingleValue);
            var partSizeOption = app.Option("-ps | --partSize", "Part Size in megabytes (default is 250 MB)", CommandOptionType.SingleValue);

            var sizeOnlyOption = app.Option("-s  | --sizeonly", "do not compare checksums", CommandOptionType.NoValue);
            var purgeOption = app.Option("-p  | --purge", "purge bucket contents before synchronizing (CAUTION!)", CommandOptionType.NoValue);
            var dryRunOption = app.Option("-d  | --dryRun", "DryRun", CommandOptionType.NoValue);

            app.Execute(args);

            var optionCases = OptionCases.None;
            if (!localPathOption.HasValue() || !bucketNameOption.HasValue() || !remotePathOption.HasValue())
            {
                _illegalArgument = true;
            }
            else
            {
                if (int.TryParse(recycleAgeOption.Value(), out var valueRA) && recycleAgeOption.HasValue())
                {
                    RecycleAge = new RecycleAge(valueRA);
                }
                else
                {
                    _illegalArgument = recycleAgeOption.HasValue() ? true : _illegalArgument;
                }

                if (int.TryParse(parallelPartsOption.Value(), out var valuePP))
                {
                    ParallelParts = new ParallelParts(valuePP);
                }
                else
                {
                    _illegalArgument = recycleAgeOption.HasValue() ? true : _illegalArgument;
                }

                LocalPath = new LocalPath(localPathOption.Value());
                BucketName = new BucketName(bucketNameOption.Value());
                RemotePath = new RemotePath(remotePathOption.Value());
                ConfigFile = configFileOption.Value();

                if (sizeOnlyOption.HasValue())
                {
                    optionCases = optionCases | OptionCases.SizeOnly;
                }

                if (purgeOption.HasValue())
                {
                    optionCases = optionCases | OptionCases.Purge;
                }

                if (dryRunOption.HasValue())
                {
                    optionCases = optionCases | OptionCases.DryRun;
                }
            }

            return (new Options(_illegalArgument, optionCases, LocalPath, RemotePath, PartSize, RecycleAge, ParallelParts),
                new AmazonOptions(ClientInformation, BucketName));
        }

        private ClientInformation GetClient() => new ConfigurationBuilder()
            .AddJsonFile(ConfigFile)
            .Build()
            .Get<ClientInformation>() ?? new ClientInformation();
    }
}
