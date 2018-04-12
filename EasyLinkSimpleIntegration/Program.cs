using EasyLinkSimpleIntegration.DataObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLinkSimpleIntegration
{
    class Program
    {
        static ApiManager apiManager;
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to EasyLinkSimpleIntegration");

            initializeApiManager();
            initializeFolderWatchers();

            while (true)
            {
                string line = Console.ReadLine();
                switch (line.ToLower())
                {
                    case "clear":
                        Console.Clear();
                        break;
                    case "getoutboundbyexternal":
                        Console.WriteLine("Enter the record external name of the outbound order you would like to retrieve.");
                        string Outbound_ExternalName = Console.ReadLine();
                        apiManager.SubmitGetEasyLinkOrderOrderRequest(EasyLinkOrder.OrderType.Outbound, "", Outbound_ExternalName);
                        break;
                    case "getoutbound":
                        Console.WriteLine("Enter the record name of the outbound order you would like to retrieve.");
                        string Outbound_Name = Console.ReadLine();
                        apiManager.SubmitGetEasyLinkOrderOrderRequest(EasyLinkOrder.OrderType.Outbound, Outbound_Name, "");
                        break;
                    case "getinboundbyexternal":
                        Console.WriteLine("Enter the record external name of the inbound order you would like to retrieve.");
                        string Inbound_ExternalName = Console.ReadLine();
                        apiManager.SubmitGetEasyLinkOrderOrderRequest(EasyLinkOrder.OrderType.Inbound, "", Inbound_ExternalName);
                        break;
                    case "getinbound":
                        Console.WriteLine("Enter the record name of the inbound order you would like to retrieve.");
                        string Inbound_Name = Console.ReadLine();
                        apiManager.SubmitGetEasyLinkOrderOrderRequest(EasyLinkOrder.OrderType.Inbound, Inbound_Name, "");
                        break;
                    default:
                        Console.WriteLine("Command '" + line + "' is not valid.");
                        break;
                }
            }
        }

        //Helper method to retrieve settings from config. Automatically typecasts to the specified type
        public static T GetAppSetting<T>(string key)
        {
            var appSetting = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(appSetting)) throw new Exception();

            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)(converter.ConvertFromInvariantString(appSetting));
        }

        //Initialize the API Manager with credentials from App.config
        static void initializeApiManager()
        {
            apiManager = new ApiManager(
                    GetAppSetting<string>("client_id"),
                    GetAppSetting<string>("client_secret"),
                    GetAppSetting<string>("username"),
                    GetAppSetting<string>("password"),
                    GetAppSetting<string>("security_token"),
                    GetAppSetting<bool>("is_sandbox")
                );
        }

        //////////////////////////////////////////////////////////////
        // FILE WATCHERS
        //////////////////////////////////////////////////////////////
        static FileSystemWatcher outboundNewPayloadWatcher;
        static FileSystemWatcher outboundUpdatePayloadWatcher;
        static FileSystemWatcher inboundNewPayloadWatcher;
        static FileSystemWatcher inboundUpdatePayloadWatcher;

        //Initialize a folder watcher for both Outbound and Inbound for NEW and UPDATE payloads
        static void initializeFolderWatchers()
        {
            string rootPath = ConfigurationManager.AppSettings["OrderFolderPath"];

            Program.outboundNewPayloadWatcher       = initializeFolderWatcher(rootPath, EasyLinkOrder.OrderType.Outbound, ApiManager.Action.NEW);
            Program.outboundUpdatePayloadWatcher    = initializeFolderWatcher(rootPath, EasyLinkOrder.OrderType.Outbound, ApiManager.Action.UPDATE);
            Program.inboundNewPayloadWatcher        = initializeFolderWatcher(rootPath, EasyLinkOrder.OrderType.Inbound , ApiManager.Action.NEW);
            Program.inboundUpdatePayloadWatcher     = initializeFolderWatcher(rootPath, EasyLinkOrder.OrderType.Inbound , ApiManager.Action.UPDATE);

            Console.WriteLine("Initialized File Watchers - "+rootPath);
        }

        //Create a folder watcher at the rootpath\ordertype\action if one doesn't exist
        //then any time a json file is modified in the folder, trigger the payloadChanged method
        //DRAG AND DROP OF FILES DOESN"T TRIGGER A CHANGE
        static FileSystemWatcher initializeFolderWatcher(string rootpath, EasyLinkOrder.OrderType orderType, ApiManager.Action action)
        {
            string path = rootpath + "\\" + orderType + "\\" + action;
            FileSystemWatcher fileWatcher = new FileSystemWatcher();

            try
            {
                //make the directory if it doesn't currently exist
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                fileWatcher.Path = path;
                fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
                fileWatcher.Filter = "*.json*";
                fileWatcher.Changed += (s, e) => Program.payloadChanged(s, e, orderType, action);
                fileWatcher.EnableRaisingEvents = true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("There was an issue setting up your folder watcher.");
                Console.WriteLine(ex);
                return null;
            }

            return fileWatcher;
        }

        // When a payload is detected, make sure that it hasn't already been processed by a different handler 
        // (FileSystemWatcher often triggers
        // If it hasn't been handled, then send it to the apiManager to submit to the ChemStar system.
        // If ChemStar successfully receives the payload then move the file to a 'Processed' sub-folder
        // If there is an issue with the submission then move the payload to an 'Errored' sub-folder
        static Dictionary<string, DateTime> lastRead = new Dictionary<string, DateTime>();
        static void payloadChanged(object sender, FileSystemEventArgs file, EasyLinkOrder.OrderType orderType, ApiManager.Action action)
        {
            try
            {
                string filename = file.Name;
                string filepath = file.FullPath;
                DateTime lastWrite = File.GetLastWriteTime(file.FullPath);

                //get the value of last write for the last time we processed the order
                DateTime lastLastWrite;
                lastRead.TryGetValue(filename, out lastLastWrite);

                //don't handle deleted files and don't handle the same file change twice
                if (File.Exists(filepath) && lastWrite != lastLastWrite)
                {
                    Console.WriteLine("ORDER FILE CHANGED: " + file.Name);
                    lastRead[filename] = lastWrite;

                    string json = System.IO.File.ReadAllText(file.FullPath);
                    EasyLinkOrder order = JsonConvert.DeserializeObject<EasyLinkOrder>(json);

                    EasyLinkResponse response = new EasyLinkResponse();
                    response = apiManager.SubmitEasyLinkOrderRequest(order, orderType, action);

                    if (response.status == "SUCCESS") moveFileToSubFolder(file, "Processed");
                    else moveFileToSubFolder(file, "Errored");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception trying to read file: " + file.Name);
                Console.WriteLine(ex.Message);
            }
        }

        // Move file to a designated sub folder
        static bool moveFileToSubFolder(FileSystemEventArgs file, String subFolderPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(file.FullPath);
            string fileDirectory = Path.GetDirectoryName(file.FullPath);
            string fileExtension = Path.GetExtension(file.FullPath);
            string newFileDirectory = fileDirectory + "\\" + subFolderPath;
            string newFullFilePath = newFileDirectory + "\\" + fileName + fileExtension;
            try
            {
                if (!Directory.Exists(newFileDirectory)) Directory.CreateDirectory(newFileDirectory);

                //don't move a file that doesn't exist
                if (!File.Exists(file.FullPath))
                {
                    return false;
                }

                //don't overwrite an existing file
                int i = 0;
                while (File.Exists(newFullFilePath))
                {
                    newFullFilePath = newFileDirectory + "\\" + fileName + "-" + i + fileExtension;
                    i++;
                }

                File.Move(file.FullPath, newFullFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("The process failed: "+ ex.ToString());
                return false;
            }
            return true;
        }

    }
}
