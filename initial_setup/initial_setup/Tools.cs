using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using Newtonsoft.Json.Linq;
using RestSharp;

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

        public static string GetAvailableVersion()
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
                            Data.Logger("Error while parsing the patch details.");
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
                        Data.Logger("Error while converting version number.");
                    }
                }

                return "false";
            }
            catch
            {
                Data.Logger("Fail while trying to obtain agent update available.");
            }

            return "false";
        }

        public static string GetDownloadLink(string availableVersion = "false")
        {

            JObject patch = new JObject();
            try
            {
                #region connects to GitHub and gets the json(jsonRest)
                RestRequest request = new RestRequest("https://api.github.com/repos/toppatch/vFenseAgent-win/releases");

                RestClient client = new RestClient();

                var responst = client.Execute(request);

                var jsonrest = JArray.Parse(responst.Content);
                #endregion

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
                        JProperty download = prop.FirstOrDefault(b => b.Name == "browser_download_url");
                        JProperty name = null;
                        JProperty size = null;
                        JProperty url = null;

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
                                        url = assets.FirstOrDefault(b => b.Name == "url");
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
                                patch.Add("name", "vFenseAgent");
                                patch.Add("vendor_name", "vFense");
                                patch.Add("release_date", publishDate);
                                patch.Add("description", body.Value.ToString());
                                patch.Add("version", release);
                                patch.Add("vendor_id", id.Value.ToString());
                                patch.Add("vendor_severity", "recommended");
                                patch.Add("status", "available");
                                patch.Add("reboot_required", "no");
                                patch.Add("install_date", 0);
                                patch.Add("repo", "");
                                patch.Add("kb", "");
                                patch.Add("support_url", @"https://github.com/toppatch/vFenseAgent-win/issues");
                                patch.Add("download_link", download);

                                try
                                {
                                    JObject getAssets = new JObject();
                                    getAssets["file_hash"] = string.Empty;
                                    getAssets["file_name"] = name.Value.ToString();
                                    getAssets["file_uri"] = url.Value.ToString();
                                    getAssets["file_size"] = int.Parse(size.Value.ToString());

                                    JArray assetslist = new JArray(getAssets);

                                    patch.Add("file_data", assetslist);
                                }
                                catch
                                {
                                    Data.Logger("Error while populating filedata for the agent patch data");
                                }

                                try
                                {
                                    JArray dependArray = new JArray();
                                    patch.Add("dependencies", dependArray);
                                }
                                catch
                                {
                                }

                            }
                            catch
                            {
                                Data.Logger("Error while populating agent patch data.");
                            }

                        }
                    }
                }
                catch (Exception e)
                {
                    Data.Logger(e.Message);
                    Data.Logger("Error while parsing agent udpate data.");
                }

            }
            catch
            {
                Data.Logger("Error in AgentPatchUdateData.");
            }

            JObject agentPatch = new JObject();
            agentPatch.Add("data", patch);


            return patch["download_link"].ToString();
        }
    }
}
