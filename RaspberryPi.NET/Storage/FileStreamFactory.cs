using System;
using System.IO;
using System.Text;

namespace RaspberryPi.Storage
{
    [Serializable]
    internal sealed class FileStreamFactory : IFileStreamFactory
    {
        public Stream Create(string path, FileMode mode)
        {
            return new FileStream(path, mode);
        }

        /// <inheritdoc />
        public Stream Create(string path, FileMode mode, FileAccess access)
        {
            return new FileStream(path, mode, access);
        }

        /// <inheritdoc />
        public Stream Create(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return new FileStream(path, mode, access, share);
        }

        /// <inheritdoc />
        public Stream Create(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            return new FileStream(path, mode, access, share, bufferSize);
        }

        /// <inheritdoc />
        public Stream Create(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)
        {
            return new FileStream(path, mode, access, share, bufferSize, options);
        }

        /// <inheritdoc />
        public Stream Create(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
        {
            return new FileStream(path, mode, access, share, bufferSize, useAsync);
        }

        /// <inheritdoc />
        public StreamReader CreateStreamReader(string path, FileMode mode, FileAccess access)
        {
            return new StreamReader(this.Create(path, mode, access));
        }

        /// <inheritdoc />
        public StreamWriter CreateStreamWriter(string path, FileMode mode, FileAccess access)
        {
            return new StreamWriter(this.Create(path, mode, access));
        }

        /// <inheritdoc />
        public StreamWriter CreateStreamWriter(Stream stream, Encoding encoding, int bufferSize, bool leaveOpen)
        {
            return new StreamWriter(stream, encoding, bufferSize, leaveOpen);
        }
    }
}