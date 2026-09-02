using DiscUtils.Udf;

namespace FreshWin.Deployment.IsoManagement
{
    public static class IsoExtraction
    {
        public static async Task Extract(string isoPath, string destination, IProgress<IsoExtractionProgress>? progress = null, bool overwriteExistingFiles = false, CancellationToken cancellationToken = default)
        {
            using FileStream isoStream = File.OpenRead(isoPath);
            using UdfReader reader = new(isoStream);

            long totalBytes = GetTotalSize(reader, "");
            var state = new ExtractionState();

            await ExtractDirectory(reader, "", destination, state, totalBytes, progress, overwriteExistingFiles, cancellationToken);
        }

        private static async Task ExtractDirectory(UdfReader reader, string sourceDir, string destDir, ExtractionState state, long totalBytes, IProgress<IsoExtractionProgress>? progress, bool overwriteExistingFiles, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(destDir);

            foreach (string filePath in reader.GetFiles(sourceDir))
            {
                cancellationToken.ThrowIfCancellationRequested();

                string destPath = Path.Combine(destDir, Path.GetFileName(filePath));
                long expectedLength = reader.GetFileLength(filePath);

                if (!overwriteExistingFiles && File.Exists(destPath))
                {
                    state.CopiedBytes += expectedLength;
                    progress?.Report(new IsoExtractionProgress(state.CopiedBytes, totalBytes));
                    continue;
                }

                using Stream source = reader.OpenFile(filePath, FileMode.Open);
                await ExtractFileTransactional(source, destPath, expectedLength, state, totalBytes, progress, overwriteExistingFiles, cancellationToken);
            }

            foreach (string subDir in reader.GetDirectories(sourceDir))
            {
                await ExtractDirectory(
                    reader, subDir,
                    Path.Combine(destDir, Path.GetFileName(subDir)),
                    state, totalBytes, progress, overwriteExistingFiles, cancellationToken);
            }
        }

        private static async Task ExtractFileTransactional(Stream source, string destPath, long expectedLength, ExtractionState state, long totalBytes, IProgress<IsoExtractionProgress>? progress, bool overwriteExistingFiles, CancellationToken cancellationToken)
        {
            string tempPath = destPath + ".tmp-" + Guid.NewGuid().ToString("N");

            try
            {
                using (FileStream dest = File.Open(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    await CopyWithProgress(source, dest, state, totalBytes, progress, cancellationToken);
                }

                File.Move(tempPath, destPath, overwriteExistingFiles);
            }
            catch
            {
                File.Delete(tempPath);
                throw;
            }
        }

        private static async Task CopyWithProgress(Stream source, Stream dest, ExtractionState state, long totalBytes, IProgress<IsoExtractionProgress>? progress, CancellationToken cancellationToken)
        {
            const int bufferSize = 4 * 1024 * 1024;
            byte[] buffer = new byte[bufferSize];
            var lastReport = DateTime.MinValue;

            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
            {
                await dest.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                state.CopiedBytes += bytesRead;

                var now = DateTime.UtcNow;
                if (progress is not null && (now - lastReport).TotalMilliseconds >= 100)
                {
                    progress.Report(new IsoExtractionProgress(state.CopiedBytes, totalBytes));
                    lastReport = now;
                }
            }

            progress?.Report(new IsoExtractionProgress(state.CopiedBytes, totalBytes));
        }

        private sealed class ExtractionState
        {
            public long CopiedBytes { get; set; }
        }

        private static long GetTotalSize(UdfReader reader, string dir)
        {
            long size = reader.GetFiles(dir).Sum(reader.GetFileLength);
            return size + reader.GetDirectories(dir).Sum(sub => GetTotalSize(reader, sub));
        }
    }
}
