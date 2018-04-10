using System;

namespace S3Backup.Composition
{
    public class OptionsSource : IOptionsSource
    {
        private readonly Lazy<(Options, AmazonOptions)> _lazyOptions;

        public OptionsSource(string[] args, IArgsParser parser)
        {
            _lazyOptions = new Lazy<(Options, AmazonOptions)>(() => parser.Parse(args));

        }

        public Options Options => _lazyOptions.Value.Item1;

        public AmazonOptions AmazonOptions => _lazyOptions.Value.Item2;
    }
}
