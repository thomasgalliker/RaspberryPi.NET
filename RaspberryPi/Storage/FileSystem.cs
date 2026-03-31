namespace RaspberryPi.Storage
{
    public class FileSystem : IFileSystem
    {
        public FileSystem(IFile file, IDirectory directory, IFileStreamFactory fileStreamFactory)
        {
            this.File = file;
            this.Directory = directory;
            this.FileStreamFactory = fileStreamFactory;
        }

        public IFile File { get; }

        public IDirectory Directory { get; }

        public IFileStreamFactory FileStreamFactory { get; }
    }
}
