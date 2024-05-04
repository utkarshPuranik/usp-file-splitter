using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace USP.FileSplitter
{
    public class FileSplitter : IFileSplitter
    {
        public async Task SplitFileAsync(string filePath, int parts, string outputFolder)
        {
            try
            {
                var originalChecksum = await GetChecksumAsync(filePath);
                var fileInfo = new FileInfo(filePath);
                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var fileSize = fileStream.Length;
                var partSize = (long)Math.Ceiling((double)fileSize / parts);

                var buffer = new byte[partSize];
                for (var i = 0; i < parts; i++)
                {
                    var partFilePath = Path.Combine(outputFolder, $"{fileInfo.Name}_{i + 1}.part");
                    await using var output = new FileStream(partFilePath, FileMode.Create, FileAccess.Write);
                    int bytesRead;
                    var bytesRemaining = partSize;
                    while (bytesRemaining > 0 && (bytesRead = await fileStream.ReadAsync(buffer, 0, (int)Math.Min(bytesRemaining, buffer.Length))) > 0)
                    {
                        await output.WriteAsync(buffer, 0, bytesRead);
                        bytesRemaining -= bytesRead;
                    }
                }

                Console.WriteLine($"File '{Path.GetFileName(filePath)}' successfully split into {parts} parts. Checksum - {originalChecksum}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error splitting file: {ex.Message}");
            }
        }

        public async Task CombineFilesAsync(string folderPath, string originalChecksum)
        {
            try
            {
                var partFiles = Directory.GetFiles(folderPath, "*.part");
                var filePath = ResolveFilePath(partFiles);

                Array.Sort(partFiles, StringComparer.InvariantCulture);

                await using var combinedFile = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                foreach (var partFile in partFiles)
                {
                    await using var partStream = new FileStream(partFile, FileMode.Open, FileAccess.Read);
                    //await partStream.CopyToAsync(combinedFile);

                    var buffer = new byte[8192];
                    int bytesRead;
                    while ((bytesRead = await partStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await combinedFile.WriteAsync(buffer, 0, bytesRead);
                    }
                }

                await combinedFile.FlushAsync();
                combinedFile.Close();

                var newChecksum = await GetChecksumAsync(filePath);

                Console.WriteLine($"New checksum: {newChecksum}");
                Console.WriteLine($"Original checksum: {originalChecksum}");
                Console.WriteLine(newChecksum == originalChecksum
                    ? "Checksums match. Files successfully combined."
                    : "Checksums do not match. Files may be corrupted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error combining files: {ex.Message}");
            }
        }

        private async Task<string> GetChecksumAsync(string filePath)
        {
            using var sha256 = SHA256.Create();
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var hashBytes = await sha256.ComputeHashAsync(fileStream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        private string ResolveFilePath(string[] partFiles)
        {
            if (partFiles.Length > 0)
            {
                var fileInfo = new FileInfo(partFiles[0]);
                var fileName = fileInfo.Name.Replace(".part", "");
                fileName = fileName[..^2];
                return fileInfo.DirectoryName != null ? Path.Combine(fileInfo.DirectoryName, fileName) : fileName;
            }

            throw new ArgumentNullException(nameof(partFiles));
        }
    }
}
