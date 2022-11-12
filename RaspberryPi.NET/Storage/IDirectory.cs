namespace RaspberryPi.Storage
{
    public interface IDirectory
    {
        bool Exists(string path);

        void CreateDirectory(string path);
    }
}