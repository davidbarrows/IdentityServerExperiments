using System;
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

        // note - this can still fail if, for example, you have the folder open in Windows Explorer or some other process still has the folder locked.
        // see:  https://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true
        public static void DeleteDirectory(DirectoryInfo dir)
        {
            if (!dir.Exists) return;

            SetAttributesNormal(dir);
            
            try
            {
                dir.Delete(true);
            }
            catch (IOException)
            {
                dir.Delete(true);
            }
            catch (UnauthorizedAccessException)
            {
                dir.Delete(true);
            }
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
