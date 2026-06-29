using System.IO.Compression;

namespace AssetWebApi.Helpers
{
    public class TempFolder : IDisposable
    {
        public string Folder { get; }

        public TempFolder(string workingDirectory)
        {
            string folder;
            do // Loop to handle edge cases where file names could match others. No an AI did not suggest this.
            {
                folder = Path.Combine(workingDirectory, Path.GetRandomFileName());
            } while (Directory.Exists(folder));

            Folder = folder;
            Directory.CreateDirectory(Folder);
        }

        public MemoryStream ToZipStream()
        {
            var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var file in Directory.EnumerateFiles(Folder, "*", SearchOption.AllDirectories))
                {
                    archive.CreateEntryFromFile(file, Path.GetRelativePath(Folder, file), CompressionLevel.Optimal);
                }
            }

            stream.Position = 0;

            return stream;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (Directory.Exists(Folder) && disposing)
                {
                    Directory.Delete(Folder, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not delete temp folder {Folder} for reason {ex.Message}");
            }

        }
    }
}