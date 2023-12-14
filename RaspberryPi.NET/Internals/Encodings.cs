using System.Text;

namespace RaspberryPi.Internals
{
    internal static class Encodings
    {
        internal static Encoding UTF8EncodingWithoutBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    }
}
