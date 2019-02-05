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

            InitializeApiManager();
            InitializeFolders();

            while (true)
            {
                string line = Console.ReadLine();
                switch (line.ToLower())
                {
                    case "clear":
                        Console.Clear();
                        break;
                    case "getbulkoutbound":
                        Console.WriteLine("Enter the oldest date for orders you would like to retreive.");
                        string Outbound_Date_String = Console.ReadLine();
                        apiManager.SendGetBulkOrderRequest(EasyLinkOrder.OrderType.Outbound, Outbound_Date_String);
                        break;
                    case "getbulkinbound":
                        Console.WriteLine("Enter the oldest date for orders you would like to retreive.");
                        string Inbound_Date_String = Console.ReadLine();
                        apiManager.SendGetBulkOrderRequest(EasyLinkOrder.OrderType.Inbound, Inbound_Date_String);
                        break;
                    case "getoutboundbyexternal":
                        Console.WriteLine("Enter the record external name of the outbound order you would like to retrieve.");
                        string Outbound_ExternalName = Console.ReadLine();
                        apiManager.SendGetOrderRequest(EasyLinkOrder.OrderType.Outbound, "", Outbound_ExternalName);
                        break;
                    case "getoutbound":
                        Console.WriteLine("Enter the record name of the outbound order you would like to retrieve.");
                        string Outbound_Name = Console.ReadLine();
                        apiManager.SendGetOrderRequest(EasyLinkOrder.OrderType.Outbound, Outbound_Name, "");
                        break;
                    case "getinboundbyexternal":
                        Console.WriteLine("Enter the record external name of the inbound order you would like to retrieve.");
                        string Inbound_ExternalName = Console.ReadLine();
                        apiManager.SendGetOrderRequest(EasyLinkOrder.OrderType.Inbound, "", Inbound_ExternalName);
                        break;
                    case "getinbound":
                        Console.WriteLine("Enter the record name of the inbound order you would like to retrieve.");
                        string Inbound_Name = Console.ReadLine();
                        apiManager.SendGetOrderRequest(EasyLinkOrder.OrderType.Inbound, Inbound_Name, "");
                        break;
                    case "scan":
                        Console.WriteLine("Scanning folders for new payloads.");
                        Program.ScanFolders();
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
        static void InitializeApiManager()
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

        //Initialize a folder watcher for both Outbound and Inbound for NEW and UPDATE payloads
        static void InitializeFolders()
        {
            string rootPath = ConfigurationManager.AppSettings["OrderFolderPath"];

            InitializeFolder(rootPath, EasyLinkOrder.OrderType.Outbound, ApiManager.Action.NEW);
            InitializeFolder(rootPath, EasyLinkOrder.OrderType.Outbound, ApiManager.Action.UPDATE);
            InitializeFolder(rootPath, EasyLinkOrder.OrderType.Inbound , ApiManager.Action.NEW);
            InitializeFolder(rootPath, EasyLinkOrder.OrderType.Inbound , ApiManager.Action.UPDATE);

            Console.WriteLine("Initialized Files - "+rootPath);
        }
        //Create a folder at the rootpath\ordertype\action if one doesn't exist
        static void InitializeFolder(string rootpath, EasyLinkOrder.OrderType orderType, ApiManager.Action action)
        {
            string path = rootpath + "\\" + orderType + "\\" + action;
            try
            {
                //make the directory if it doesn't currently exist
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an issue setting up your folder.");
                Console.WriteLine(ex);
            }
        }


        //Scan each folder for new payloads
        static void ScanFolders()
        {
            string rootPath = ConfigurationManager.AppSettings["OrderFolderPath"];

            ScanFolder(rootPath, EasyLinkOrder.OrderType.Outbound, ApiManager.Action.NEW);
            ScanFolder(rootPath, EasyLinkOrder.OrderType.Outbound, ApiManager.Action.UPDATE);
            ScanFolder(rootPath, EasyLinkOrder.OrderType.Inbound, ApiManager.Action.NEW);
            ScanFolder(rootPath, EasyLinkOrder.OrderType.Inbound, ApiManager.Action.UPDATE);

            Console.WriteLine("Done Scanning Directories");
        }
        //Scan the specified folder for new payloads, process any that are found
        static void ScanFolder(string rootpath, EasyLinkOrder.OrderType orderType, ApiManager.Action action)
        {
            string path = rootpath + "\\" + orderType + "\\" + action;

            Console.WriteLine("Scanning directory " + path);
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] files = null;
            try
            {
                files = di.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                foreach(FileInfo file in files)
                {
                    ProcessPayload(file, orderType, action);
                }

            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("There was an error scanning the directory.");
            }
        }


        // Try to process the specified payload
        // If ChemStar successfully receives the payload then move the file to a 'Processed' sub-folder
        // If there is an issue with the submission then move the payload to an 'Errored' sub-folder
        static void ProcessPayload(FileInfo file, EasyLinkOrder.OrderType orderType, ApiManager.Action action)
        {
            try
            {
                Console.WriteLine("FOUND ORDER FILE: " + file.Name);

                string filename = file.Name;
                string filepath = file.FullName;

                //don't handle deleted files
                if (File.Exists(filepath))
                {
                    string json = System.IO.File.ReadAllText(filepath);
                    EasyLinkOrder order = JsonConvert.DeserializeObject<EasyLinkOrder>(json);

                    EasyLinkResponse response = new EasyLinkResponse();
                    if (action == ApiManager.Action.NEW) response = apiManager.SendNewOrderRequest(orderType, order);
                    if (action == ApiManager.Action.UPDATE) response = apiManager.SendUpdateOrderRequest(orderType, order);

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
        static bool moveFileToSubFolder(FileInfo file, String subFolderPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(file.FullName);
            string fileDirectory = Path.GetDirectoryName(file.FullName);
            string fileExtension = Path.GetExtension(file.FullName);
            string newFileDirectory = fileDirectory + "\\" + subFolderPath;
            string newFullFilePath = newFileDirectory + "\\" + fileName + fileExtension;
            try
            {
                if (!Directory.Exists(newFileDirectory)) Directory.CreateDirectory(newFileDirectory);

                //don't move a file that doesn't exist
                if (!File.Exists(file.FullName))
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

                File.Move(file.FullName, newFullFilePath);
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
