using System;
using System.CodeDom;
using System.IO;
using System.Net;
using System.Threading;
using Agent.Core.Utils;

namespace Agent.RV.Utils
{
    class NetworkThrottle
    {
        static public void DownloadThrottle(string localPath, string remotePath, string appName, byte bandwidth)
        {
            int bytesProcessed = 0;
            Stream remoteStream = null;
            Stream localStream = null;
            WebResponse response = null;

            try
            {
                WebRequest request = WebRequest.Create(remotePath);

                if (request != null)
                {
                    response = request.GetResponse();
                    if (response != null)
                    {
                        remoteStream = response.GetResponseStream();

                        localStream = File.Create(Path.Combine(localPath, appName));

                        byte[] buffer = new byte[bandwidth];
                        int bytesRead;

                        do
                        {
                            bytesRead = remoteStream.Read(buffer, 0, buffer.Length);

                            localStream.Write(buffer, 0, bytesRead);
                            bytesProcessed += bytesRead;
                            Thread.Sleep(1000);
                        } while (bytesRead > 0);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }
            finally
            {
                if (response != null)
                    response.Close();
                if (remoteStream != null)
                    remoteStream.Close();
                if (localStream != null)
                    localStream.Close();
            }
        }
    }
}
