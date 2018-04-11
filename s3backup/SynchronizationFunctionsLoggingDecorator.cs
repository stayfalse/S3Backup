﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup
{
    public class SynchronizationFunctionsLoggingDecorator : ISynchronizationFunctions
    {
        private readonly ISynchronizationFunctions _inner;

        public SynchronizationFunctionsLoggingDecorator(ISynchronizationFunctions inner)
        {
            _inner = inner;
        }

        public Dictionary<string, FileInfo> GetFiles(LocalPath localPath)
        {
            var files = _inner.GetFiles(localPath);
            Log.PutOut($"FileInfo dictionary received (LocalPath: {localPath})");
            return files;
        }

        public async Task<bool> TryUploadMissingFiles(Dictionary<string, FileInfo> filesInfo, bool dryRun, LocalPath localPath, PartSize partSize)
        {
            Log.PutOut($"Bucket does not contain {filesInfo.Count} objects");
            if (await _inner.TryUploadMissingFiles(filesInfo, dryRun, localPath, partSize).ConfigureAwait(false))
            {
                return true;
            }
            else
            {
                Log.PutOut($"Skip upload");
                return false;
            }
        }

        public async Task<bool> TryUploadMismatchedFile(FileInfo fileInfo, bool dryRun, LocalPath localPath, PartSize partSize)
        {
            if (await _inner.TryUploadMismatchedFile(fileInfo, dryRun, localPath, partSize).ConfigureAwait(false))
            {
                return true;
            }
            else
            {
                Log.PutOut($"Mismatched {fileInfo.Name} upload skiped");
                return false;
            }
        }

        public async Task<bool> TryDeleteMismatchedObject(S3ObjectInfo s3Object, bool dryRun, DateTime threshold)
        {
            Log.PutOut($"File for object key {s3Object.Key} not found");
            if (await _inner.TryDeleteMismatchedObject(s3Object, dryRun, threshold).ConfigureAwait(false))
            {
                return true;
            }
            else
            {
                Log.PutOut($"Skip deleting {s3Object.Key}");
                return false;
            }
        }

        public bool EqualSize(S3ObjectInfo s3Object, FileInfo fileInfo)
        {
            if (_inner.EqualSize(s3Object, fileInfo))
            {
                Log.PutOut($"Size {s3Object.Key} {fileInfo.Name} matched");
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool EqualETag(S3ObjectInfo s3Object, FileInfo fileInfo, PartSize partSize)
        {
            if (_inner.EqualETag(s3Object, fileInfo, partSize))
            {
                Log.PutOut($"Hash {s3Object.Key} {fileInfo.Name} matched");
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
