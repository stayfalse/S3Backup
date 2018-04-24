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

        private ThresholdDate Threshold => new ThresholdDate((_recycleAge > 0) ? DateTime.Now.Subtract(new TimeSpan(_recycleAge, 0, 0, 0)) : default);

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

            var localPath = app.Option("-l  | --local", "Set Local Path (required parameter)", CommandOptionType.SingleValue);
            var bucketName = app.Option("-b  | --bucket", "Set Bucket Name (required parameter)", CommandOptionType.SingleValue);
            var remotePath = app.Option("-rp | --remotePath", "Set Remote Path (required parameter)", CommandOptionType.SingleValue);
            var configFile = app.Option("-c  | --config", "Set AWS config file path (default is appsettings.json)", CommandOptionType.SingleValue);

            var recycleAge = app.Option("-ra | --recycleAge", "Set recycle age in days (default is default DateTime)", CommandOptionType.SingleValue);
            var parallelParts = app.Option("-pp | --parallelParts", "Set number of Parallel Parts (defauls is 4)", CommandOptionType.SingleValue);
            var partSize = app.Option("-ps | --partSize", "Set Part Size in megabytes (default is 250 MB)", CommandOptionType.SingleValue);

            var sizeOnly = app.Option("-s  | --sizeonly", "Do not compare checksums", CommandOptionType.NoValue);
            var purge = app.Option("-p  | --purge", "Purge bucket contents before synchronizing (CAUTION!)", CommandOptionType.NoValue);
            var dryRun = app.Option("-d  | --dryRun", "Initialize dry run", CommandOptionType.NoValue);

            app.Execute(args);

            var optionCases = OptionCases.None;
            try
            {
                if (!localPath.HasValue() || !bucketName.HasValue() || !remotePath.HasValue())
                {
                    throw new IllegalArgumentException($"Command line argument is missing (one or several required parameters).");
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
                }

                return (new Options(optionCases, LocalPath, RemotePath, PartSize, Threshold, ParallelParts),
                    new AmazonOptions(ClientInformation, BucketName, dryRun.HasValue()));
            }
            catch (IllegalArgumentException)
            {
                app.ShowHelp();
                throw;
            }
        }

        private static int ParseArg(CommandOption commandOption)
        {
            if (commandOption is null)
            {
                throw new ArgumentNullException(nameof(commandOption));
            }

            if (commandOption.HasValue())
            {
                if (int.TryParse(commandOption.Value(), out var value))
                {
                    return value;
                }
                else
                {
                    throw new IllegalArgumentException($"Command line argument {commandOption.Template} is invalid.");
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
