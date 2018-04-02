using System;
using System.Collections.Generic;
using System.Text;

namespace S3Backup
{
    public class OptionsSource : IOptionsSource
    {
        private readonly Lazy<Options> _options;

        public OptionsSource(string[] args, IArgsParser parser)
        {
            _options = new Lazy<Options>(() => parser.Parse(args));
        }

        public Options Options => _options.Value;
    }
}
