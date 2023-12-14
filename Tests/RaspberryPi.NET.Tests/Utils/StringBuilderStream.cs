using System;
using System.IO;
using System.Text;
using RaspberryPi.Extensions;

namespace RaspberryPi.Tests.Utils
{
    // Provides a writeable stream to a StringBuilder
    // Initially based on code from Simple.Web (https://github.com/markrendle/Simple.Web)
    public class StringBuilderStream : Stream
    {
        private readonly MemoryStream buffer = new MemoryStream();
        private readonly StreamReader bufferReader;
        private readonly StringBuilder stringBuilder;

        public StringBuilderStream() : this(new StringBuilder())
        {
        }

        public StringBuilderStream(Stream stream)
            : this(stream.GetString())
        {
        }

        public StringBuilderStream(string content) : this(new StringBuilder(content))
        {
        }

        public StringBuilderStream(StringBuilder stringBuilder)
        {
            this.stringBuilder = stringBuilder;

            // Copy the input stream into the current buffer
            var writer = new StreamWriter(this.buffer);
            writer.Write(stringBuilder.ToString());
            writer.Flush();

            this.buffer.Position = 0;

            this.bufferReader = new StreamReader(this.buffer, detectEncodingFromByteOrderMarks: true);
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => this.buffer.Length;

        public override long Position
        {
            get => this.buffer.Position;
            set => this.buffer.Position = value;
        }

        public override void Flush()
        {
            if (this.buffer.Length > 0)
            {
                this.buffer.Position = 0;
                this.stringBuilder.Clear();
                this.stringBuilder.Append(this.bufferReader.ReadToEnd());
                this.buffer.SetLength(0);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.buffer.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.buffer.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.buffer.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.buffer.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            this.Flush();
            base.Dispose(disposing);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.buffer.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.buffer.EndWrite(asyncResult);
            this.Flush();
        }

        public override string ToString()
        {
            return this.stringBuilder.ToString();
        }
    }
}