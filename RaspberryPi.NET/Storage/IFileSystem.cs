namespace RaspberryPi.Storage
{
    public interface IFileSystem
    {
        IFile File { get; }
        
        IDirectory Directory { get; }

        IFileStreamFactory FileStreamFactory { get; }
    }
}