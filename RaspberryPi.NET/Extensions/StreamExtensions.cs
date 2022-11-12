using System.IO;
using System.Text;

namespace RaspberryPi.Extensions
{
    internal static class StreamExtensions
    {
        /// <summary>
        /// Rewind stream to first position.
        /// </summary>
        /// <param name="stream">Stream to rewind.</param>
        /// <returns>The same stream as <paramref name="stream"/> (just for convenience).</returns>
        public static T Rewind<T>(this T stream) where T : Stream
        {
            if (stream.CanSeek && stream.Position != 0)
            {
                stream.Seek(0L, SeekOrigin.Begin);
            }

            return stream;
        }

        /// <summary>
        /// Returns the copy of <paramref name="sourceStream"/>.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        public static MemoryStream Copy(this Stream sourceStream, bool writable = false)
        {
            var targetStream = new MemoryStream(new byte[1024], writable);
            sourceStream.Rewind().CopyTo(targetStream);
            return targetStream.Rewind();
        }
        
        public static MemoryStream Copy(this MemoryStream sourceStream, bool writable = false)
        {
            return new MemoryStream(sourceStream.ToArray(), writable);
        }

        public static string GetString(this Stream stream)
        {
            return stream.GetString(Encoding.UTF8);
        }

        public static string GetString(this Stream stream, Encoding encoding)
        {
            using (var reader = new StreamReader(stream.Rewind(), encoding))
            {
                var text = reader.ReadToEnd();
                return text;
            }
        }

        public static string GetString(this MemoryStream memoryStream)
        {
            return memoryStream.GetString(Encoding.UTF8);
        }

        public static string GetString(this MemoryStream memoryStream, Encoding encoding)
        {
            return encoding.GetString(memoryStream.ToArray());
        }
    }
}