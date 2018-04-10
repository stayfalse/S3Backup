namespace S3Backup
{
    public interface IOptionsSource
    {
        Options Options { get; }

        AmazonOptions AmazonOptions { get; }
    }
}
