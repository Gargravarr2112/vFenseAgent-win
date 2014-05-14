using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Agent.Core;
using Agent.Core.Data.Model;
using Agent.Core.Utils;
using Agent.RV.Data;
using Agent.RV.Utils;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Agent.RV.AgentUpdater
{
    public static class AgentUpdateManager
    {
        //Backup Directory = C:\ProgramData\TopPatchBackup\
        public static readonly string AgentUpdateDirectory = Path.Combine(Settings.TempDirectory, "RVAgentUpdate");

        public static RVsofResult AddAppDetailsToResults(RVsofResult results)
        {
            results.Data.Name           = String.Empty;
            results.Data.Description    = String.Empty;
            results.Data.Kb             = String.Empty;
            results.Data.ReleaseDate    = 0.0;
            results.Data.VendorSeverity = String.Empty;
            results.Data.VendorName     = String.Empty;
            results.Data.VendorId       = String.Empty;
            results.Data.Version        = String.Empty;
            results.Data.SupportUrl     = String.Empty;
            return results;
        }

        public static Operations.SavedOpData DownloadUpdate(Operations.SavedOpData update)
        {
            var uris = new List<DownloadUri>();

            foreach (var uriData in update.filedata_app_uris)
            {
                var tempDownloadUri = new DownloadUri();
                tempDownloadUri.FileName = uriData.file_name;
                tempDownloadUri.FileSize = uriData.file_size;
                tempDownloadUri.Hash = uriData.file_hash;

                foreach (var uri in uriData.file_uris)
                        tempDownloadUri.Uris.Add(uri);

                uris.Add(tempDownloadUri);
            }

            try
            {
                if (Directory.Exists(AgentUpdateDirectory))
                    Directory.Delete(AgentUpdateDirectory, true);
                Directory.CreateDirectory(AgentUpdateDirectory);
            }catch{}


            foreach (var file in uris)
            {
                // Just in case the web server is using a self-signed cert. 
                // Webclient won't validate the SSL/TLS cerficate if it's not trusted.
                var tempCallback = ServicePointManager.ServerCertificateValidationCallback;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var filepath = Path.Combine(AgentUpdateDirectory, file.FileName);

                try
                {
                    using (var client = new WebClient())
                    {
                        if (Settings.Proxy != null)
                            client.Proxy = Settings.Proxy;

                        var downloaded = false;
                        foreach (var uriSingle in file.Uris)
                        {
                            try
                            {
                                var splitted = uriSingle.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);
                                var splitted2 = splitted[1].Split(new[] { '/' });
                                var relayserver = splitted2[0];

                                if (downloaded) break;

                                Logger.Log("Attempting to download {1} from {0} with file size of {2}.", LogLevel.Info, relayserver, file.FileName, file.FileSize);

                                byte NetThrottle = byte.Parse(update.net_throttle);
                                if (NetThrottle > 0 || NetThrottle == null)
                                {
                                    Agent.RV.Utils.NetworkThrottle.DownloadThrottle(filepath, uriSingle, file.FileName, NetThrottle);
                                }
                                else
                                {
                                    client.DownloadFile(uriSingle, filepath);
                                }

                                if (File.Exists(filepath))
                                {
                                    var localFileHash = RvUtils.Md5HashFile(filepath).ToLower();
                                    Logger.Log("Download Complete,  {0}", LogLevel.Info, file.FileName);
                                    Logger.Log("Checking MD5 Hash...");
                                    Logger.Log("Incoming Hash: {0}", LogLevel.Info, file.Hash);
                                    Logger.Log("Local Hash: {0}", LogLevel.Info, localFileHash);
                                    downloaded = true;

                                    if (localFileHash != file.Hash.ToLower())
                                    {
                                        Logger.Log("Local file {0} Hash did not match remote's. Retrying with a different server.", LogLevel.Info, file.FileName);
                                        update.error = "Local file Hash did not match remote. Bad file integrity. ";
                                        update.success = false.ToString().ToLower();
                                        downloaded = false;
                                    }
                                }
                                else
                                {
                                    Logger.Log("File {0} did not download. Retrying with a different server.", LogLevel.Info, file.FileName);
                                    update.error = "File did not download successfully, it was not found on disk. Please check download server.";
                                    update.success = false.ToString().ToLower();
                                    downloaded = false;
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Log("File {0} failed to download correctly... Possible connection issue, Retrying with a different server.", LogLevel.Info, file.FileName);
                                update.error = "File did not download correctly, Exception message: " + e.Message + ". Please check download server connectivity.";
                                update.success = false.ToString().ToLower();
                                downloaded = false;
                            }
                        }

                        //Check if the file was successfully downloaded and return.
                        if (downloaded)
                        {
                            update.error   = String.Empty;
                            update.success = true.ToString().ToLower();
                        }
                    }
                }
                catch (Exception e)
                {
                    //Critical exception occurred.
                    update.error   = "Agent update did not download successfully, Exception occured, refer to log for details.";
                    update.success = false.ToString().ToLower();
                    Logger.Log("One or more Agent update Files were not downloaded successfully; {0}.",
                    LogLevel.Error, file.FileName);
                    Logger.LogException(e);
                    return update;
                }

                ServicePointManager.ServerCertificateValidationCallback = tempCallback;
            }

            return update;
        }

        /// <summary>
        /// Checks if there is an available update from the github repo.
        /// Using RestRquest connection to obtain the json array.
        /// https://api.github.com/repos/toppatch/vFenseAgent-win/releases
        /// </summary>
        /// <returns>Returns a string with either the new version available or false.</returns>
        public static string IsAgentUpdateAvailable()
        {
            try
            {
                #region connects to GitHub and gets the json(jsonrest)
                RestRequest request = new RestRequest("https://api.github.com/repos/toppatch/vFenseAgent-win/releases");

                RestClient client = new RestClient();

                var responst = client.Execute(request);

                var jsonrest = JArray.Parse(responst.Content);
                #endregion

                int major = 0;
                int minor = 0;
                int patch = 0;
                int currentmajor = 0;
                int currentminor = 0;
                int currentpatch = 0;

                #region get the installed version and set the values to the appropriate variables 

                try
                {
                    string currentversion = CurrentInstalledVersion();
                    string[] cv = currentversion.Split('.');
                    currentmajor = int.Parse(cv[0]);
                    currentminor = int.Parse(cv[1]);
                    currentpatch = int.Parse(cv[2]);
                }
                catch
                {
                    Logger.Log("Error while parsing current installed version.");
                }

                #endregion

                foreach (JToken x in jsonrest.Children())
                {
                    var prop = x.Children<JProperty>();
                    JProperty tag_name = prop.FirstOrDefault(b => b.Name == "tag_name");

                    try
                    {
                        string versionstring = tag_name.Value.ToString();
                        string version = versionstring.Replace("v", string.Empty);
                        string[] split = version.Split('.');
                        try
                        {
                            major = int.Parse(split[0]);
                            minor = int.Parse(split[1]);
                            patch = int.Parse(split[2]);
                        }
                        catch
                        {
                            Logger.Log("Error while parsing the patch details.", LogLevel.Warning);
                        }

                        if (currentmajor < major)
                            return version;
                        if (currentminor < minor && currentmajor <= major)
                            return version;
                        if (currentpatch < patch && currentminor <= minor && currentmajor <= major)
                            return version;

                    }
                    catch
                    {
                        Logger.Log("Error while converting version number.", LogLevel.Warning);
                    }
                }

                return "false";
            }
            catch
            {
                Logger.Log("Fail while trying to obtain agent update available.", LogLevel.Error);
            }

            return "false";
        }

        /// <summary>
        /// Generates server list with the new agent update date.
        /// It requires the new agent update version #.
        /// </summary>
        /// <param name="availableVersion">New agent version number #, #.#.#, default to false.</param>
        /// <returns>Returns list<application> with the agent update data.</application></returns>
        public static List<Application> AgentPatchUdateData(string availableVersion = "false")
        {
            var patchData = new List<Application>();

            try
            {
                #region connects to GitHub and gets the json(jsonRest)
                RestRequest request = new RestRequest("https://api.github.com/repos/toppatch/vFenseAgent-win/releases");

                RestClient client = new RestClient();

                var responst = client.Execute(request);

                var jsonrest = JArray.Parse(responst.Content);
                #endregion

                var patch = new Application();

                try
                {
                    foreach (JToken x in jsonrest.Children())
                    {
                        var prop = x.Children<JProperty>();
                        JProperty tag_name = prop.FirstOrDefault(b => b.Name == "tag_name");
                        JProperty id = prop.FirstOrDefault(b => b.Name == "id");
                        JProperty published = prop.FirstOrDefault(b => b.Name == "published_at");
                        JProperty zip_url = prop.FirstOrDefault(b => b.Name == "zipball_url");
                        JProperty jpassets = prop.FirstOrDefault(b => b.Name == "assets");
                        JProperty body = prop.FirstOrDefault(b => b.Name == "body");
                        JProperty name = null;
                        JProperty size = null;

                        string tagname = tag_name.Value.ToString();
                        string release = tagname.Replace("v", string.Empty);

                        if (release == availableVersion)
                        {
                            try
                            {
                                foreach (JToken y in jpassets.Children())
                                {
                                    foreach (JToken z in y.Children())
                                    {
                                        var assets = z.Children<JProperty>();
                                        name = assets.FirstOrDefault(b => b.Name == "name");
                                        size = assets.FirstOrDefault(b => b.Name == "size");
                                    }
                                }
                            }
                            catch
                            {
                            }

                            string publishstring = published.Value.ToString();
                            string[] publishStrings = publishstring.Split(' ');
                            double publishDate = 0;
                            try
                            {
                                publishDate = (Agent.Core.Utils.Time.DateToEpoch(publishStrings[0]));
                            }
                            catch
                            {
                            }

                            try
                            {
                                patch.Name = "vFenseAgent";
                                patch.VendorName = "vFense";
                                patch.ReleaseDate = publishDate;
                                patch.Description = body.Value.ToString();
                                patch.Version = release;
                                patch.VendorId = id.Value.ToString();
                                patch.VendorSeverity = "Important";
                                patch.Status = "available";
                                patch.SupportUrl = @"https://github.com/toppatch/vFenseAgent-win/issues";

                                try
                                {
                                    var assetslist = new DownloadUri();
                                    assetslist.FileName = name.Value.ToString();
                                    assetslist.Uri = zip_url.Value.ToString();
                                    assetslist.FileSize = int.Parse(size.Value.ToString());

                                    patch.FileData.Add(assetslist);
                                }
                                catch
                                {
                                    Logger.Log("Error while populating filedata for the agent patch data", LogLevel.Warning);
                                }
                                }
                            catch
                            {
                                Logger.Log("Error while populating agent patch data.", LogLevel.Warning);
                            }

                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Log(e.Message);
                    Logger.Log("Error while parsing agent udpate data.", LogLevel.Error);
                }

                patchData.Add(patch);
            }
            catch
            {
                Logger.Log("Error in AgentPatchUdateData.", LogLevel.Error);
            }

            return patchData;
        }

        /// <summary>
        /// Checks the registry key file to retrive the current version installed on the system.
        /// </summary>
        /// <returns>Returns a string with the version installed, or empty string if it fails.</returns>
        public static string CurrentInstalledVersion()
        {
            {
                string topPatchRegistry;
                const string key = "Version";


                //64bit or 32bit Machine?
                if (Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") == "x86"
                    && Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432") == null)
                    //32bit
                    topPatchRegistry = @"SOFTWARE\TopPatch Inc.\TopPatch Agent";
                else
                    //64bit
                    topPatchRegistry = @"SOFTWARE\Wow6432Node\TopPatch Inc.\TopPatch Agent";


                //Retrieve the Version number from the TopPatch Agent Registry Key
                try
                {
                    using (var rKey = Registry.LocalMachine.OpenSubKey(topPatchRegistry))
                    {
                        var installedVersion = ((rKey == null) || (rKey.GetValue(key) == null)) ? String.Empty : rKey.GetValue(key).ToString();
                        return installedVersion;
                    }
                }
                catch (Exception) { return string.Empty; }
            }
        }
        
        
    }
}
