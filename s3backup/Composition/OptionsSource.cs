using System;

namespace S3Backup.Composition
{
    public sealed class OptionsSource : IOptionsSource
    {
        private readonly Lazy<(Options, AmazonOptions, LogOptions)> _lazyOptions;

        public OptionsSource(string[] args, IArgsParser parser)
        {
            _lazyOptions = new Lazy<(Options, AmazonOptions, LogOptions)>(() => parser.Parse(args));
        }

        public Options Options => _lazyOptions.Value.Item1;

        public AmazonOptions AmazonOptions => _lazyOptions.Value.Item2;

        public LogOptions LogOptions => _lazyOptions.Value.Item3;
    }
}
