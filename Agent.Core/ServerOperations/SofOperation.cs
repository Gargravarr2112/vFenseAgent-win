using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Agent.Core.Utils;

namespace Agent.Core.ServerOperations
{
    public static class HttpMethods
    {
        public const string Post = "POST";
        public const string Put = "PUT";
        public const string Get = "GET";
    }

    public static class ApiCalls
    {

        private static JToken responseUri = null;


        private const string Delimeter = "|||";

        //LOGIN/LOGOUT
        public const string Login = "/rvl/login" + Delimeter + HttpMethods.Post; //POST

        public static string Logout()
        {
            try
            {

                string respUri = null;
                string respMethod = null;

                var prop = responseUri.Children<JProperty>();
                JProperty Data = prop.FirstOrDefault(b => b.Name == "logout");

                JProperty uri = null;
                JProperty method = null;

                foreach (JToken x in Data.Children())
                {
                    var xdata = x.Children<JProperty>();
                    uri = xdata.FirstOrDefault(b => b.Name == "response_uri");
                    method = xdata.FirstOrDefault(b => b.Name == "request_method");
                }
                respUri = uri.Value.ToString();
                respMethod = method.Value.ToString();

                if (!string.IsNullOrEmpty(respUri) && !string.IsNullOrEmpty(respMethod))
                    return (respUri + Delimeter + respMethod);

            }
            catch
            {
                return "/rvl/logout" + Delimeter + HttpMethods.Post;
            }

            return "/rvl/logout" + Delimeter + HttpMethods.Post;
        }

        //CORE API CALLS
        public static string CoreNewAgent()
        {
            try
            {

                string respUri = null;
                string respMethod = null;

                var prop = responseUri.Children<JProperty>();
                JProperty Data = prop.FirstOrDefault(b => b.Name == "new_agent");

                JProperty uri = null;
                JProperty method = null;

                foreach (JToken x in Data.Children())
                {
                    var xdata = x.Children<JProperty>();
                    uri = xdata.FirstOrDefault(b => b.Name == "response_uri");
                    method = xdata.FirstOrDefault(b => b.Name == "request_method");
                }
                respUri = uri.Value.ToString();
                respMethod = method.Value.ToString();

                if (!string.IsNullOrEmpty(respUri) && !string.IsNullOrEmpty(respMethod))
                    return (respUri + Delimeter + respMethod);

            }
            catch
            {
                return "/rvl/v2/core/newagent" + Delimeter + HttpMethods.Post;
            }

            return "/rvl/v2/core/newagent" + Delimeter + HttpMethods.Post;
        }


        public static string CoreRebootResults()
        {
            return "/rvl/v2/" + Settings.AgentId + "/core/results/reboot" + Delimeter + HttpMethods.Put;
        }

        public static string CoreCheckIn()
        {
            try
            {

                string respUri = null;
                string respMethod = null;

                var prop = responseUri.Children<JProperty>();
                JProperty Data = prop.FirstOrDefault(b => b.Name == "check_in");

                JProperty uri = null;
                JProperty method = null;

                foreach (JToken x in Data.Children())
                {
                    var xdata = x.Children<JProperty>();
                    uri = xdata.FirstOrDefault(b => b.Name == "response_uri");
                    method = xdata.FirstOrDefault(b => b.Name == "request_method");
                }
                respUri = uri.Value.ToString();
                respMethod = method.Value.ToString();

                if (!string.IsNullOrEmpty(respUri) && !string.IsNullOrEmpty(respMethod))
                    return (respUri + Delimeter + respMethod);

            }
            catch
            {
                return "/rvl/v2/" + Settings.AgentId + "/core/checkin" + Delimeter + HttpMethods.Get;
            }

            return "/rvl/v2/" + Settings.AgentId + "/core/checkin" + Delimeter + HttpMethods.Get;
        }

        public static string CoreStartUp()
        {
            try
            {

                string respUri = null;
                string respMethod = null;

                var prop = responseUri.Children<JProperty>();
                JProperty Data = prop.FirstOrDefault(b => b.Name == "startup");

                JProperty uri = null;
                JProperty method = null;

                foreach (JToken x in Data.Children())
                {
                    var xdata = x.Children<JProperty>();
                    uri = xdata.FirstOrDefault(b => b.Name == "response_uri");
                    method = xdata.FirstOrDefault(b => b.Name == "request_method");
                }
                respUri = uri.Value.ToString();
                respMethod = method.Value.ToString();

                if (!string.IsNullOrEmpty(respUri) && !string.IsNullOrEmpty(respMethod))
                    return (respUri + Delimeter + respMethod);

            }
            catch
            {
                return "/rvl/v2/" + Settings.AgentId + "/core/startup" + Delimeter + HttpMethods.Put;
            }

            return "/rvl/v2/" + Settings.AgentId + "/core/startup" + Delimeter + HttpMethods.Put;
        }


        //RV
        public static string RvInstallWinUpdateResults()
        {
            try
            {

                string respUri = null;
                string respMethod = null;

                var prop = responseUri.Children<JProperty>();
                JProperty Data = prop.FirstOrDefault(b => b.Name == "install_os_apps");

                JProperty uri = null;
                JProperty method = null;

                foreach (JToken x in Data.Children())
                {
                    var xdata = x.Children<JProperty>();
                    uri = xdata.FirstOrDefault(b => b.Name == "response_uri");
                    method = xdata.FirstOrDefault(b => b.Name == "request_method");
                }
                respUri = uri.Value.ToString();
                respMethod = method.Value.ToString();

                if (!string.IsNullOrEmpty(respUri) && !string.IsNullOrEmpty(respMethod))
                    return (respUri + Delimeter + respMethod);

            }
            catch
            {
                return "/rvl/v2/" + Settings.AgentId + "/rv/results/install/apps/os" + Delimeter + HttpMethods.Put;
            }

            return "/rvl/v2/" + Settings.AgentId + "/rv/results/install/apps/os" + Delimeter + HttpMethods.Put;
        }

        public static string RvInstallCustomAppsResults()
        {
            try
            {

                string respUri = null;
                string respMethod = null;

                var prop = responseUri.Children<JProperty>();
                JProperty Data = prop.FirstOrDefault(b => b.Name == "install_custom_apps");

                JProperty uri = null;
                JProperty method = null;

                foreach (JToken x in Data.Children())
                {
                    var xdata = x.Children<JProperty>();
                    uri = xdata.FirstOrDefault(b => b.Name == "response_uri");
                    method = xdata.FirstOrDefault(b => b.Name == "request_method");
                }
                respUri = uri.Value.ToString();
                respMethod = method.Value.ToString();

                if (!string.IsNullOrEmpty(respUri) && !string.IsNullOrEmpty(respMethod))
                    return (respUri + Delimeter + respMethod);

            }
            catch
            {
                return "/rvl/v2/" + Settings.AgentId + "/rv/results/install/apps/custom" + Delimeter + HttpMethods.Put;
            }

            return "/rvl/v2/" + Settings.AgentId + "/rv/results/install/apps/custom" + Delimeter + HttpMethods.Put;
        }

        public static string RvInstallSupportedAppsResults()
        {
            try
            {

                string respUri = null;
                string respMethod = null;

                var prop = responseUri.Children<JProperty>();
                JProperty Data = prop.FirstOrDefault(b => b.Name == "install_supported_apps");

                JProperty uri = null;
                JProperty method = null;

                foreach (JToken x in Data.Children())
                {
                    var xdata = x.Children<JProperty>();
                    uri = xdata.FirstOrDefault(b => b.Name == "response_uri");
                    method = xdata.FirstOrDefault(b => b.Name == "request_method");
                }
                respUri = uri.Value.ToString();
                respMethod = method.Value.ToString();

                if (!string.IsNullOrEmpty(respUri) && !string.IsNullOrEmpty(respMethod))
                    return (respUri + Delimeter + respMethod);

            }
            catch
            {
                return "/rvl/v2/" + Settings.AgentId + "/rv/results/install/apps/supported" + Delimeter +
                       HttpMethods.Put;
            }
            return "/rvl/v2/" + Settings.AgentId + "/rv/results/install/apps/supported" + Delimeter +
                       HttpMethods.Put;
        } 

        public static string RvInstallAgentUpdateResults()
        {
            try
            {

                string respUri = null;
                string respMethod = null;

                var prop = responseUri.Children<JProperty>();
                JProperty Data = prop.FirstOrDefault(b => b.Name == "install_agent_update");

                JProperty uri = null;
                JProperty method = null;

                foreach (JToken x in Data.Children())
                {
                    var xdata = x.Children<JProperty>();
                    uri = xdata.FirstOrDefault(b => b.Name == "response_uri");
                    method = xdata.FirstOrDefault(b => b.Name == "request_method");
                }
                respUri = uri.Value.ToString();
                respMethod = method.Value.ToString();

                if (!string.IsNullOrEmpty(respUri) && !string.IsNullOrEmpty(respMethod))
                    return (respUri + Delimeter + respMethod);

            }
            catch
            {
                return "/rvl/v2/" + Settings.AgentId + "/rv/results/install/apps/agent" + Delimeter + HttpMethods.Put;
            }

            return "/rvl/v2/" + Settings.AgentId + "/rv/results/install/apps/agent" + Delimeter + HttpMethods.Put;
        } 

        public static string RvRebootResults()
        {
            try
            {

                string respUri = null;
                string respMethod = null;

                var prop = responseUri.Children<JProperty>();
                JProperty Data = prop.FirstOrDefault(b => b.Name == "reboot");

                JProperty uri = null;
                JProperty method = null;

                foreach (JToken x in Data.Children())
                {
                    var xdata = x.Children<JProperty>();
                    uri = xdata.FirstOrDefault(b => b.Name == "response_uri");
                    method = xdata.FirstOrDefault(b => b.Name == "request_method");
                }
                respUri = uri.Value.ToString();
                respMethod = method.Value.ToString();

                if (!string.IsNullOrEmpty(respUri) && !string.IsNullOrEmpty(respMethod))
                    return (respUri + Delimeter + respMethod);

            }
            catch
            {
                return "/rvl/v2/" + Settings.AgentId + "/core/results/reboot" + Delimeter + HttpMethods.Put;
            }

            return "/rvl/v2/" + Settings.AgentId + "/core/results/reboot" + Delimeter + HttpMethods.Put;
        }

        public static string RvUpdatesApplications()
        {
            try
            {

                string respUri = null;
                string respMethod = null;

                var prop = responseUri.Children<JProperty>();
                JProperty Data = prop.FirstOrDefault(b => b.Name == "updatesapplications");

                JProperty uri = null;
                JProperty method = null;

                foreach (JToken x in Data.Children())
                {
                    var xdata = x.Children<JProperty>();
                    uri = xdata.FirstOrDefault(b => b.Name == "response_uri");
                    method = xdata.FirstOrDefault(b => b.Name == "request_method");
                }
                respUri = uri.Value.ToString();
                respMethod = method.Value.ToString();

                if (!string.IsNullOrEmpty(respUri) && !string.IsNullOrEmpty(respMethod))
                    return (respUri + Delimeter + respMethod);

            }
            catch
            {
                return "/rvl/v2/" + Settings.AgentId + "/rv/updatesapplications" + Delimeter + HttpMethods.Put;
            }

            return "/rvl/v2/" + Settings.AgentId + "/rv/updatesapplications" + Delimeter + HttpMethods.Put;
        } 

        public static string RvUninstallOperation()
        {
            try
            {

                string respUri = null;
                string respMethod = null;

                var prop = responseUri.Children<JProperty>();
                JProperty Data = prop.FirstOrDefault(b => b.Name == "uninstall");

                JProperty uri = null;
                JProperty method = null;

                foreach (JToken x in Data.Children())
                {
                    var xdata = x.Children<JProperty>();
                    uri = xdata.FirstOrDefault(b => b.Name == "response_uri");
                    method = xdata.FirstOrDefault(b => b.Name == "request_method");
                }
                respUri = uri.Value.ToString();
                respMethod = method.Value.ToString();

                if (!string.IsNullOrEmpty(respUri) && !string.IsNullOrEmpty(respMethod))
                    return (respUri + Delimeter + respMethod);

            }
            catch
            {
                return "/rvl/v2/" + Settings.AgentId + "/rv/results/uninstall" + Delimeter + HttpMethods.Put;
            }

            return "/rvl/v2/" + Settings.AgentId + "/rv/results/uninstall" + Delimeter + HttpMethods.Put;
        }

        public static string AvailableAgentUpdate()
        {
            try
            {
                int a = 0;
                string respUri = null;
                string respMethod = null;
                JProperty Data = null;

                do
                {
                    
                    try
                    {
                        var prop = responseUri.Children<JProperty>();
                        Data = prop.FirstOrDefault(b => b.Name == "available_agent_update");
                    }
                    catch
                    {
                        a++;
                        Thread.Sleep(5000);
                    }
                        

                    JProperty uri = null;
                    JProperty method = null;

                    try
                    {
                        foreach (JToken x in Data.Children())
                        {
                            var xdata = x.Children<JProperty>();
                            uri = xdata.FirstOrDefault(b => b.Name == "response_uri");
                            method = xdata.FirstOrDefault(b => b.Name == "request_method");
                        }

                        respUri = uri.Value.ToString();
                        respMethod = method.Value.ToString();
                    }
                    catch
                    { }

                } while (a <= 10 && Data == null); 


                if (!string.IsNullOrEmpty(respUri) && !string.IsNullOrEmpty(respMethod))
                    return (respUri + Delimeter + respMethod);

            }
            catch
            {
                return "/rvl/v2/" + Settings.AgentId + "/rv/available_agent_update" + Delimeter + HttpMethods.Put;
            }

            return "/rvl/v2/" + Settings.AgentId + "/rv/available_agent_update" + Delimeter + HttpMethods.Put;
        }

        public static string UninstallAgent()
        {
            try
            {

                string respUri = null;
                string respMethod = null;

                var prop = responseUri.Children<JProperty>();
                JProperty Data = prop.FirstOrDefault(b => b.Name == "uninstall_agent");

                JProperty uri = null;
                JProperty method = null;

                foreach (JToken x in Data.Children())
                {
                    var xdata = x.Children<JProperty>();
                    uri = xdata.FirstOrDefault(b => b.Name == "response_uri");
                    method = xdata.FirstOrDefault(b => b.Name == "request_method");
                }
                respUri = uri.Value.ToString();
                respMethod = method.Value.ToString();

                if (!string.IsNullOrEmpty(respUri) && !string.IsNullOrEmpty(respMethod))
                    return (respUri + Delimeter + respMethod);

            }
            catch
            {
                return "/rvl/v2/" + Settings.AgentId + "/rv/results/uninstall" + Delimeter + HttpMethods.Put;
            }

            return "/rvl/v2/" + Settings.AgentId + "/rv/results/uninstall" + Delimeter + HttpMethods.Put;
        }

        public static string ra()
        {
            try
            {

                string respUri = null;
                string respMethod = null;

                var prop = responseUri.Children<JProperty>();
                JProperty Data = prop.FirstOrDefault(b => b.Name == "ra");

                JProperty uri = null;
                JProperty method = null;

                foreach (JToken x in Data.Children())
                {
                    var xdata = x.Children<JProperty>();
                    uri = xdata.FirstOrDefault(b => b.Name == "response_uri");
                    method = xdata.FirstOrDefault(b => b.Name == "request_method");
                }
                respUri = uri.Value.ToString();
                respMethod = method.Value.ToString();

                if (!string.IsNullOrEmpty(respUri) && !string.IsNullOrEmpty(respMethod))
                    return (respUri + Delimeter + respMethod);

            }
            catch
            {
                return "/rvl/v2/" + Settings.AgentId + "/ra/rd/results" + Delimeter + HttpMethods.Put;
            }

            return "/rvl/v2/" + Settings.AgentId + "ra/rd/results" + Delimeter + HttpMethods.Put;
        }

        public static string Shutdown()
        {
            try
            {

                string respUri = null;
                string respMethod = null;

                var prop = responseUri.Children<JProperty>();
                JProperty Data = prop.FirstOrDefault(b => b.Name == "uninstall_agent");

                JProperty uri = null;
                JProperty method = null;

                foreach (JToken x in Data.Children())
                {
                    var xdata = x.Children<JProperty>();
                    uri = xdata.FirstOrDefault(b => b.Name == "response_uri");
                    method = xdata.FirstOrDefault(b => b.Name == "request_method");
                }
                respUri = uri.Value.ToString();
                respMethod = method.Value.ToString();

                if (!string.IsNullOrEmpty(respUri) && !string.IsNullOrEmpty(respMethod))
                    return (respUri + Delimeter + respMethod);

            }
            catch
            {
                return "/rvl/v2/" + Settings.AgentId + "/core/results/shutdown" + Delimeter + HttpMethods.Put;
            }

            return "/rvl/v2/" + Settings.AgentId + "/core/results/shutdown" + Delimeter + HttpMethods.Put;
        }

        //MONITOR
        public static string MonData()
        {
            try
            {
               
                string respUri = null;
                string respMethod = null;
               
                    var prop = responseUri.Children<JProperty>();
                    JProperty Data = prop.FirstOrDefault(b => b.Name == "monitor_data");

                    JProperty uri = null;
                    JProperty method = null;

                    foreach (JToken x in Data.Children())
                    {
                        var xdata = x.Children<JProperty>();
                        uri = xdata.FirstOrDefault(b => b.Name == "response_uri");
                        method = xdata.FirstOrDefault(b => b.Name == "request_method");
                    }
                    respUri = uri.Value.ToString();
                    respMethod = method.Value.ToString();

                    if (!string.IsNullOrEmpty(respUri) && !string.IsNullOrEmpty(respMethod))
                        return (respUri + Delimeter + respMethod);
                
            }
            catch
            {
                return "/rvl/v2/" + Settings.AgentId + "/monitoring/monitordata" + Delimeter + HttpMethods.Post;
            }

            return "/rvl/v2/" + Settings.AgentId + "/monitoring/monitordata" + Delimeter + HttpMethods.Post;
        } 
        

        /// <summary>
        /// Converts the json message from the server for response uri into a jtoken.
        /// </summary>
        /// <param name="jmsg">Json messega from the sever containing response uris.</param>
        public static void RefreshUris(ISofOperation jmsg)
        {
            try
            {
                string json = jmsg.RawOperation;
                JObject parjson = JObject.Parse(json);
                responseUri = parjson["data"];
            }
            catch (Exception e)
            {
                Logger.Log("Error while processing \"refresh_uri\".", LogLevel.Error);
                Logger.Log(e.Message);
            }
        }


    }
    

    public class OperationValue
    {
        public const string NewAgent            = "new_agent";
        public const string NewAgentId          = "new_agent_id";
        public const string Startup             = "startup";
        public const string Reboot              = "reboot";
        public const string Shutdown            = "shutdown";
        public const string Received            = "received";
        public const string CheckIn             = "check_in";
        public const string InvalidAgentId      = "invalid_agent_id";
        public const string SystemInfo          = "system_info";
        public const string ReverseTunnel       = "reverse_tunnel";
        public const string TargetIp            = "target_ip";
        public const string TargetPort          = "target_port";
        public const string LocalPort           = "local_port";
        public const string ResumeOp            = "resume_operations";
        public const string InstallWindowsUpdate = "install_os_apps";
        public const string InstallSupportedApp  = "install_supported_apps";
        public const string InstallCustomApp     = "install_custom_apps";
        public const string InstallAgentUpdate   = "install_agent_update";
        public const string Uninstall            = "uninstall";
        public const string AgentUninstall       = "uninstall_agent";
        public const string RefreshUris          = "refresh_response_uris";
    }

    public class OperationKey
    {
        public const string Operation           = "operation";
        public const string OperationId         = "operation_id";
        public const string SystemInfo          = "system_info";
        public const string HardwareInfo        = "hardware";
        public const string Plugin              = "plugin";
        public const string Data                = "data";
        public const string AgentId             = "agent_id";
        public const string CpuThrottle         = "cpu_throttle";
        public const string Success             = "success";
        public const string Error               = "error";
        public const string Customer            = "customer_name";
        public const string Id                  = "id";
        public const string Rebooted            = "rebooted";
        public const string PluginData          = "plugins";
    }

    public class SofOperation : ISofOperation
    {
        public string Plugin { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public string Api { get; set; }
        public string ResponseUri { get; set; }
        public string RequestMethod { get; set; }
        public List<string> Data { get; set; }


        public Dictionary<string, ISofOperation> PluginData { get; set; }

        /// <summary>
        /// Represents the JSON string sent by the server.
        /// </summary>
        public string RawResult { get; set; }
        
        /// <summary>
        /// Represents the JSON string to be sent to the server. (Results, data, etc)
        /// </summary>
        public string RawOperation { get; set; }

        public JObject JsonMessage { get; set; }

        public SofOperation()
        {
            // Self assigned operation id.
            Id = Guid.NewGuid() + "-agent";
            
            Plugin = Settings.EmptyValue;
            Type = Settings.EmptyValue;

            RawOperation = Settings.EmptyValue;
            JsonMessage = null;

            PluginData = new Dictionary<string, ISofOperation>();
            Data = new List<string>();
        }

        public SofOperation(string serverMessage) : this()
        {
            RawOperation = serverMessage;
            JsonMessage = JObject.Parse(serverMessage);

            Plugin = (JsonMessage[OperationKey.Plugin] == null) ?
                Settings.EmptyValue : JsonMessage[OperationKey.Plugin].ToString();
            
            // If no operation Id is provided, self assign one.
            Id = (JsonMessage[OperationKey.OperationId] == null) ?
                Id : JsonMessage[OperationKey.OperationId].ToString();

            Type = JsonMessage[OperationKey.Operation].ToString();
        }        

        /// <summary>
        /// Add the results for a SofOperation. This way the operation can determine what to do with it.
        /// </summary>
        /// <param name="results">The results.</param>
        public void AddResult(SofResult results)
        {
            var root = new JObject();
            root[OperationKey.Id] = results.OperationId;
            root[OperationKey.Operation] = results.Operation;
            root[OperationKey.Success] = results.Success;
            root[OperationKey.Error] = results.Error;
            
            Data.Add(root.ToString());
        }

        /// <summary>
        /// Returns a JSON formatted string of the operations properties.
        /// </summary>
        /// <returns></returns>
        public virtual string ToJson()
        {
            var json = new JObject();

            json[OperationKey.Operation] = Type;
            json[OperationKey.OperationId] = Id;
            json[OperationKey.AgentId] = Settings.AgentId;

            return json.ToString();
        }
    }

    public class SofResult
    {
        public string OperationId { get; set; }
        public string Operation { get; set; }
        public string Success { get; set; }
        public string Error { get; set; }
        public string AppId { get; set; }
    }
}
