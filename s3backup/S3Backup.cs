using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.S3.Model;

namespace S3Backup
{
    class Program
    {
        static Dictionary<string, FileInfo> GetFiles(string localPath)
        {
            var files = new DirectoryInfo(localPath)
                .GetFiles("*", SearchOption.AllDirectories);
            var filesInfo = new Dictionary<string, FileInfo>();
            foreach (FileInfo f in files)
            {
                filesInfo.Add(f.Name, f);
            }
            return filesInfo;
        }
        static string ComputeLocalETag(FileInfo file, int partSize)
        {
            BinaryReader br = new BinaryReader(new FileStream(file.FullName, FileMode.Open));
            int sumIndex = 0;
            var parts = 0;
            string localETag = "\"";

            var md5 = MD5.Create();
            var hashLength = md5.HashSize / 8;
            var n = (file.Length / partSize) * hashLength + ((file.Length % partSize != 0) ? hashLength : 0) ;
            byte[] sum = new byte[n];
                int a = (file.Length > partSize) ? partSize : (int)file.Length;
                while (sumIndex < sum.Length)
                {
                    md5.ComputeHash(br.ReadBytes(a)).CopyTo(sum, sumIndex);
                    parts++;
                    if (parts * partSize > file.Length)
                    { a = (int)file.Length % partSize; }
                    sumIndex += hashLength;
                }
                if (parts > 1) { sum = md5.ComputeHash(sum); }

                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < sum.Length; i++)
                {
                    sBuilder.Append(sum[i].ToString("x2"));
                }
                localETag += $"{sBuilder.ToString()}";
            localETag += (parts > 1) ? $"-{parts}\"" : "\"";

            return localETag;
        }

        static async Task Synchronize(Options options)
        {
            Log.PutOut($"Synchronization started");

            var threshold = (options.RecycleAge != 0) ? DateTime.Now.Subtract(new TimeSpan(options.RecycleAge, 0, 0, 0)) : default(DateTime);
            var useAmazon = new UseAmazon(options.BucketName, options.ClientInfo);

            if (options.Purge && !options.DryRun)
            {
                Log.PutOut($"Purge bucket whis remote path {options.RemotePath}");
                await useAmazon.Purge(options.RemotePath);
            }

            var objects = await useAmazon.GetS3ObjectsList(options.RemotePath);
            Log.PutOut($"AmazonS3ObjectsList received (BucketName: {options.BucketName})");

            var filesInfo = GetFiles(options.LocalPath);
            Log.PutOut($"FileInfo dictionary received (LocalPath: {options.LocalPath})");

            foreach (S3Object obj in objects)
            {
                if (filesInfo.TryGetValue(obj.Key, out FileInfo f))
                {
                    Log.PutOut($"Matching object {obj.Key} and file {f.Name} started");
                    filesInfo.Remove(f.Name);

                    if (f.Length == obj.Size)
                    {
                        Log.PutOut($"Size {obj.Key} {f.Name} matched");
                        if (options.SizeOnly) continue;
                        if (obj.ETag == ComputeLocalETag(f, options.PartSize))
                        {
                            Log.PutOut($"Hash {obj.Key} {f.Name} matched");
                            continue;
                        }
                    }

                    if (!options.DryRun)
                    {
                         await useAmazon.UploadObjectToBucket(f, options.LocalPath, options.PartSize);
                         Log.PutOut($"Mismatched {f.Name} uploaded");
                         continue;
                    }
                    Log.PutOut($"Mismatched {f.Name} upload skiped");
                }
                else
                {
                    Log.PutOut($"File for object key {obj.Key} not found");
                    if (!options.DryRun && obj.LastModified < threshold)
                    {
                        Log.PutOut($"Delete object {obj.Key} last modified = {obj.LastModified}");
                        await useAmazon.DeleteObject(obj.Key);
                        continue;
                    }
                    Log.PutOut($"Skip deleting {obj.Key}");
                }
            }
            Log.PutOut($"Bucket does not contain {filesInfo.Count} objects");
            if (!options.DryRun)
            {
                foreach (var f in filesInfo)
                {
                    Log.PutOut($"Upload {f.Key}");
                    await useAmazon.UploadObjectToBucket(f.Value, options.LocalPath, options.PartSize);
                    Log.PutOut("Uploaded");
                }
            }
            else
            { Log.PutOut("Skip upload");  }
            Log.PutOut(((options.DryRun) ? "DryRun ": "") + "Synchronization completed");
        }

        static async Task Main(string[] args)
        {
            var options = new Options(args);
            if (!options.illegalArgument)
            {
                await Synchronize(options).ConfigureAwait(false);
            }
            else
            { Log.PutOut("Synchronization can not be started in case of incorrect command line arguments"); }

        }
    }
}
