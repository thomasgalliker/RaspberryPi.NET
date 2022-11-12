using System.IO;
using RaspberryPi.Storage;

namespace RaspberryPi.Tests.Utils
{
    public class TestDirectory : IDirectory
    {
        private readonly string rootPath;

        public TestDirectory(string rootPath)
        {
            this.rootPath = rootPath;
        }

        public bool Exists(string path)
        {
            return System.IO.File.Exists(this.Combine(path));
        }

        public void CreateDirectory(string path)
        {
            System.IO.Directory.CreateDirectory(this.Combine(path));
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
