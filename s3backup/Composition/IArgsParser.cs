namespace S3Backup.Composition
{
    public interface IArgsParser
    {
        (Options, AmazonOptions) Parse(string[] args);
    }
}
