using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace initial_setup
{
    class Tools
    {
        public static bool CopyFile(string newFile, string oldFile)
        {
            var newfile = new FileInfo(newFile);
            var oldfile = new FileInfo(oldFile);
            string errorMsg = "";
            var f2 = new FileIOPermission(FileIOPermissionAccess.AllAccess, oldFile);
            f2.AddPathList(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, newFile);

            try
            {
                f2.Demand();
            }
            catch (SecurityException s)
            {
                Console.WriteLine(s.Message);
            }
            

            for (int x = 0; x < 100; x++)
            {
                try
                {
                    File.Delete(oldfile.FullName);
                    newfile.CopyTo(oldfile.FullName, true);
                    return true;
                }
                catch (Exception e)
                {
                    errorMsg = e.Message + " :   " + e.InnerException;
                    Thread.Sleep(200);
                }
            }
            Data.Logger(errorMsg);
            return false;
        }
    }
}
