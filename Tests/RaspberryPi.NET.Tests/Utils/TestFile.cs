using System.IO;
using RaspberryPi.Storage;

namespace RaspberryPi.Tests.Utils
{
    public class TestFile : IFile
    {
        private readonly string rootPath;

        public TestFile(string rootPath)
        {
            this.rootPath = rootPath;
        }

        public void Delete(string path)
        {
            System.IO.File.Delete(this.Combine(path));
        }

        public bool Exists(string path)
        {
            return System.IO.File.Exists(this.Combine(path));
        }

        public string[] ReadAllLines(string path)
        {
            return System.IO.File.ReadAllLines(this.Combine(path));
        }

        public void WriteAllText(string path, string contents)
        {
            System.IO.File.WriteAllText(this.Combine(path), contents);
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
