using DiscUtils.Udf;
using FreshWin.Deployment.IsoManagement.Exceptions;

namespace FreshWin.Deployment.IsoManagement.Extensions
{
    public static class FileInfoExtensions
    {
        static FileInfoExtensions()
        {

        }
        public static bool IsIsoUdf(this FileInfo isoPath)
        {
            using var isoStream = isoPath.OpenRead();
            return UdfReader.Detect(isoStream);
        }

        public static bool IsNotIsoUdf(this FileInfo isoPath) => !isoPath.IsIsoUdf();

        public static async Task ExtractIsoToDirectory(this FileInfo isoPath, DirectoryInfo destinationPath, IProgress<IsoExtractionProgress>? progress = null, bool overwriteExistingFiles = false, CancellationToken cancellationToken = default)
        {
            if (!isoPath.Exists)
            {
                throw new FileNotFoundException($"The specified file does not exist: {isoPath.FullName}");
            }
            if (!destinationPath.Exists)
            {
                throw new DirectoryNotFoundException($"The specified destination directory does not exist: {destinationPath.FullName}");
            }
            // Verify if the ISO file is UDF
            if (isoPath.IsNotIsoUdf())
            {
                throw new IsoFileNotUdfException(isoPath);
            }

            // Perform the extraction
            await IsoExtraction.Extract(isoPath.FullName, destinationPath.FullName, progress, overwriteExistingFiles, cancellationToken);
        }
    }
}
