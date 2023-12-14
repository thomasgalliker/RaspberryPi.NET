using System;
using System.IO;
using System.Text;
using Moq;

namespace RaspberryPi.Tests.Utils
{
    internal class StreamWriterMock : Mock<StreamWriter>
    {
        private readonly StringBuilder stringBuilder = new StringBuilder();
        private readonly string fileName;

        public StreamWriterMock(string fileName) : base(fileName)
        {
            this.fileName = fileName;

            this.Setup(w => w.Write(It.IsAny<string>()))
                .Callback<string>(l => this.stringBuilder.Append(l));

            this.Setup(w => w.WriteLine(It.IsAny<string>()))
                .Callback<string>(l => this.stringBuilder.AppendLine(l));

            this.Setup(w => w.WriteLineAsync(It.IsAny<string>()))
                .Callback<string>(l => this.stringBuilder.AppendLine(l));
        }

        public override string ToString()
        {
            return this.stringBuilder.ToString();
        }

        public string GetSummary()
        {
            return
                $"{this.fileName}:{Environment.NewLine}" +
                $"{this}";
        }
    }
}