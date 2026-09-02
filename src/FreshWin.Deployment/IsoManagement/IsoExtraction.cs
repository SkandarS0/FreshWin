using DiscUtils.Udf;

namespace FreshWin.Deployment.IsoManagement
{
    public static class IsoExtraction
    {
        public static void Extract(string isoPath, string destination, IProgress<IsoExtractionProgress>? progress = null)
        {
            using FileStream isoStream = File.OpenRead(isoPath);
            using UdfReader reader = new(isoStream);

            long totalBytes = GetTotalSize(reader, "");
            long copiedBytes = 0;

            ExtractDirectory(reader, "", destination, ref copiedBytes, totalBytes, progress);
        }

        private static void ExtractDirectory(UdfReader reader, string sourceDir, string destDir, ref long copiedBytes, long totalBytes, IProgress<IsoExtractionProgress>? progress)
        {
            Directory.CreateDirectory(destDir);

            foreach (string filePath in reader.GetFiles(sourceDir))
            {
                string destPath = Path.Combine(destDir, Path.GetFileName(filePath));

                using Stream source = reader.OpenFile(filePath, FileMode.Open);
                using FileStream dest = File.Create(destPath);

                CopyWithProgress(source, dest, ref copiedBytes, totalBytes, progress);
            }

            foreach (string subDir in reader.GetDirectories(sourceDir))
            {
                ExtractDirectory(
                    reader, subDir,
                    Path.Combine(destDir, Path.GetFileName(subDir)),
                    ref copiedBytes, totalBytes, progress);
            }
        }

        private static void CopyWithProgress(Stream source, Stream dest, ref long copiedBytes, long totalBytes, IProgress<IsoExtractionProgress>? progress)
        {
            const int bufferSize = 4 * 1024 * 1024; // 4 MB
            byte[] buffer = new byte[bufferSize];
            var lastReport = DateTime.MinValue;

            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                dest.Write(buffer, 0, bytesRead);
                copiedBytes += bytesRead;

                var now = DateTime.UtcNow;
                if (progress is not null && (now - lastReport).TotalMilliseconds >= 100)
                {
                    progress.Report(new IsoExtractionProgress(copiedBytes, totalBytes));
                    lastReport = now;
                }
            }

            progress?.Report(new IsoExtractionProgress(copiedBytes, totalBytes));
        }

        private static long GetTotalSize(UdfReader reader, string dir)
        {
            long size = reader.GetFiles(dir).Sum(reader.GetFileLength);
            return size + reader.GetDirectories(dir).Sum(sub => GetTotalSize(reader, sub));
        }
    }
}
