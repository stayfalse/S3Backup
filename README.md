# S3Backup
Backup tool for Amazon S3

Example of command line parameters to run backup(order isn’t strict):
--local=local_path --bucket=bucket_name --remote_path=prefix  --config=path_to_credentials_file

Local Path, Bucket Name and Remote Path(prefix) is required parameters to run backup.

Your AWS credentials file should contain "ServiceURL", "AccessKey" and "SecretKey” fields.

For more options and short versions of commands run -? | -h | --help.
