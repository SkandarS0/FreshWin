using DiscUtils.Udf;

namespace FreshWin.Deployment.IsoManagement
{
    public static class IsoExtraction
    {
        public static async Task Extract(string isoPath, string destination, IProgress<IsoExtractionProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            using FileStream isoStream = File.OpenRead(isoPath);
            using UdfReader reader = new(isoStream);

            long totalBytes = GetTotalSize(reader, "");
            var state = new ExtractionState();

            await ExtractDirectory(reader, "", destination, state, totalBytes, progress, cancellationToken);
        }

        private static async Task ExtractDirectory(UdfReader reader, string sourceDir, string destDir, ExtractionState state, long totalBytes, IProgress<IsoExtractionProgress>? progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(destDir);

            foreach (string filePath in reader.GetFiles(sourceDir))
            {
                cancellationToken.ThrowIfCancellationRequested();

                string destPath = Path.Combine(destDir, Path.GetFileName(filePath));

                using Stream source = reader.OpenFile(filePath, FileMode.Open);
                using FileStream dest = File.Create(destPath);

                await CopyWithProgress(source, dest, state, totalBytes, progress, cancellationToken);
            }

            foreach (string subDir in reader.GetDirectories(sourceDir))
            {
                await ExtractDirectory(
                    reader, subDir,
                    Path.Combine(destDir, Path.GetFileName(subDir)),
                    state, totalBytes, progress, cancellationToken);
            }
        }

        private static async Task CopyWithProgress(Stream source, Stream dest, ExtractionState state, long totalBytes, IProgress<IsoExtractionProgress>? progress, CancellationToken cancellationToken)
        {
            const int bufferSize = 4 * 1024 * 1024; // 4 MB
            byte[] buffer = new byte[bufferSize];
            var lastReport = DateTime.MinValue;

            int bytesRead;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                bytesRead = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (bytesRead <= 0)
                {
                    break;
                }

                cancellationToken.ThrowIfCancellationRequested();
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
