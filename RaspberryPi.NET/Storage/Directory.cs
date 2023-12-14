namespace RaspberryPi.Storage
{
    public class Directory : IDirectory
    {
        public bool Exists(string path)
        {
            return System.IO.Directory.Exists(path);
        }

        public void CreateDirectory(string path)
        {
            System.IO.Directory.CreateDirectory(path);
        }
    }
}