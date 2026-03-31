using System.IO;
using System.Text;
using FluentAssertions.Equivalency;
using RaspberryPi.Storage;

namespace RaspberryPi.Tests.Utils
{
    public class TestFileStreamFactory : IFileStreamFactory
    {
        private readonly string rootPath;

        public TestFileStreamFactory(string rootPath)
        {
            this.rootPath = rootPath;
        }

        public Stream Create(string path, FileMode mode)
        {
            return new FileStream(this.Combine(path), mode);
        }

        public Stream Create(string path, FileMode mode, FileAccess access)
        {
            return new FileStream(this.Combine(path), mode, access);
        }

        public Stream Create(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return new FileStream(this.Combine(path), mode, access, share);
        }

        public Stream Create(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            return new FileStream(this.Combine(path), mode, access, share, bufferSize);
        }

        public Stream Create(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)
        {
            return new FileStream(this.Combine(path), mode, access, share, bufferSize, options);
        }

        public Stream Create(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
        {
            return new FileStream(this.Combine(path), mode, access, share, bufferSize, useAsync);
        }

        public StreamReader CreateStreamReader(string path, FileMode mode, FileAccess access)
        {
            return new StreamReader(this.Create(path, mode, access));
        }

        public StreamWriter CreateStreamWriter(string path, FileMode mode, FileAccess access)
        {
            return new StreamWriter(this.Create(path, mode, access));
        }

        public StreamWriter CreateStreamWriter(Stream stream, Encoding encoding, int bufferSize, bool leaveOpen)
        {
            return new StreamWriter(stream, encoding, bufferSize, leaveOpen);
        }

        private string Combine(string path)
        {
            if (Path.IsPathRooted(path))
            {
                path = path[1..];
            }

            return Path.Combine(this.rootPath, path);
        }
    }
}
