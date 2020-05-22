using System.IO;

namespace Quickstart.Tests.Helpers
{
    public class DirManager
    {
        public static void DeleteAndRecreate(string path)
        {
            if (Directory.Exists(path))
            {
                DeleteDirectory(new DirectoryInfo(path));
            }
            CreateDirIfNotExists(path);
        }

        public static void CreateDirIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void DeleteDirectory(DirectoryInfo dir)
        {
            if (!dir.Exists) return;

            SetAttributesNormal(dir);
            dir.Delete(true);
        }

        // see:  https://stackoverflow.com/questions/1701457/directory-delete-doesnt-work-access-denied-error-but-under-windows-explorer-it
        private static void SetAttributesNormal(DirectoryInfo dir)
        {
            foreach (var subDir in dir.GetDirectories())
            {
                SetAttributesNormal(subDir);
                subDir.Attributes = FileAttributes.Normal;
            }
            foreach (var file in dir.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
            }
        }
    }
}
