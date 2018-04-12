using System;

using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace S3Backup.Composition
{
    public sealed class ArgsParser : IArgsParser
    {
        private string _configFile;

        private int _parallelParts;

        private int _partSize;

        private int _recycleAge;

        private PartSize PartSize => (_partSize == 0) ? new PartSize((int)Math.Pow(2, 20) * 250) : new PartSize((int)Math.Pow(2, 20) * _partSize);

        private ParallelParts ParallelParts => (_parallelParts == 0) ? new ParallelParts(4) : new ParallelParts(_parallelParts);

        private ThresholdDate Threshold => new ThresholdDate((_recycleAge != 0) ? DateTime.Now.Subtract(new TimeSpan(_recycleAge, 0, 0, 0)) : default);

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

            var localPath = app.Option("-l  | --local", "set Local Path", CommandOptionType.SingleValue);
            var bucketName = app.Option("-b  | --bucket", "set Bucket Name", CommandOptionType.SingleValue);
            var remotePath = app.Option("-rp | --remotePath", "set Remote Path", CommandOptionType.SingleValue);
            var configFile = app.Option("-c  | --config", "set AWS config file path", CommandOptionType.SingleValue);

            var recycleAge = app.Option("-ra | --recycleAge", "set recycle age in days (default is default DateTime)", CommandOptionType.SingleValue);
            var parallelParts = app.Option("-pp | --parallelParts", "ParallelParts (defauls is 4)", CommandOptionType.SingleValue);
            var partSize = app.Option("-ps | --partSize", "Part Size in megabytes (default is 250 MB)", CommandOptionType.SingleValue);

            var sizeOnly = app.Option("-s  | --sizeonly", "do not compare checksums", CommandOptionType.NoValue);
            var purge = app.Option("-p  | --purge", "purge bucket contents before synchronizing (CAUTION!)", CommandOptionType.NoValue);
            var dryRun = app.Option("-d  | --dryRun", "DryRun", CommandOptionType.NoValue);

            app.Execute(args);

            var optionCases = OptionCases.None;
            if (!localPath.HasValue() || !bucketName.HasValue() || !remotePath.HasValue())
            {
                throw new Exception($"Command line argument is missing or invalid.");
            }
            else
            {
                _recycleAge = ParseArg(recycleAge);
                _parallelParts = ParseArg(parallelParts);
                _partSize = ParseArg(partSize);

                LocalPath = new LocalPath(localPath.Value());
                BucketName = new BucketName(bucketName.Value());
                RemotePath = new RemotePath(remotePath.Value());
                ConfigFile = configFile.Value();

                if (sizeOnly.HasValue())
                {
                    optionCases = optionCases | OptionCases.SizeOnly;
                }

                if (purge.HasValue())
                {
                    optionCases = optionCases | OptionCases.Purge;
                }

                if (dryRun.HasValue())
                {
                    optionCases = optionCases | OptionCases.DryRun;
                }
            }

            return (new Options(optionCases, LocalPath, RemotePath, PartSize, Threshold, ParallelParts),
                new AmazonOptions(ClientInformation, BucketName));
        }

        private static int ParseArg(CommandOption commandOption)
        {
            if (commandOption.HasValue())
            {
                if (int.TryParse(commandOption.Value(), out var value))
                {
                    return value;
                }
                else
                {
                    throw new Exception($"Command line argument {commandOption.Template} is invalid.");
                }
            }

            return default;
        }

        private ClientInformation GetClient() => new ConfigurationBuilder()
            .AddJsonFile(ConfigFile)
            .Build()
            .Get<ClientInformation>() ?? new ClientInformation();
    }
}
