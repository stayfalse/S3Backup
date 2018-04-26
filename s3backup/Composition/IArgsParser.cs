namespace S3Backup.Composition
{
    public interface IArgsParser
    {
        (Options, AmazonOptions, LogOptions) Parse(string[] args);
    }
}
