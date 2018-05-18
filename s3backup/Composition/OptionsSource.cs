using System;

namespace S3Backup.Composition
{
    public sealed class OptionsSource : IOptionsSource
    {
        private readonly Lazy<(Options Options, AmazonOptions AmazonOptions, LogOptions LogOptions)> _lazyOptions;

        public OptionsSource(string[] args, IArgsParser parser)
        {
            _lazyOptions = new Lazy<(Options, AmazonOptions, LogOptions)>(() => parser.Parse(args));
        }

        public Options Options => _lazyOptions.Value.Options;

        public AmazonOptions AmazonOptions => _lazyOptions.Value.AmazonOptions;

        public LogOptions LogOptions => _lazyOptions.Value.LogOptions;
    }
}
