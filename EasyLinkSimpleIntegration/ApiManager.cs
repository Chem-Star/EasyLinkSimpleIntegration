using EasyLinkSimpleIntegration.DataObjects;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EasyLinkSimpleIntegration
{
    class ApiManager
    {
        public enum Action { NEW, UPDATE, CANCEL, GET }

        //Used to keep track of the API credentials validation
        private class SalesforceAuthenticationResponse
        {
            public string access_token      { get; set; }
            public string instance_url      { get; set; }
            public string id                { get; set; }
            public string token_type        { get; set; }
            public string issued_at         { get; set; }
            public string signature         { get; set; }
            public string error             { get; set; }
            public string error_description { get; set; }
        }

        //Constants to help set up our requests
        private const string OutboundSuffix = "/services/apexrest/v2/outbound";
        private const string InboundSuffix = "/services/apexrest/v2/inbound";


        //The credentials that will be used to connect to the API
        private String clientId;
        private String clientSecret;
        private String username;
        private String password;
        private String securityToken;
        private bool isSandbox;

        //The response back from the credentials validation
        private SalesforceAuthenticationResponse auth_response;


        //  CONSTRUCTOR - sets user credentials and initializes security settings
        public ApiManager(String clientId, String clientSecret, String username, String password, String securityToken, bool isSandbox)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.username = username;
            this.password = password;
            this.securityToken = securityToken;
            this.isSandbox = isSandbox;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            ValidateCredentials();
        }

        // Connects to the Salesforce authentication token API and stores the result in auth_response
        private bool ValidateCredentials()
        {
            string endpoint = "";
            if (isSandbox) endpoint = "https://test.salesforce.com/services/oauth2/token";
            else endpoint = "https://login.salesforce.com/services/oauth2/token";

            var client = new RestClient(endpoint);
            var request = new RestRequest(Method.POST);
            request.AddQueryParameter("grant_type", "password");
            request.AddQueryParameter("client_id", clientId);
            request.AddQueryParameter("client_secret", clientSecret);
            request.AddQueryParameter("username", username);
            request.AddQueryParameter("password", password + securityToken);
            request.AddHeader("cache-control", "no-cache");
            IRestResponse response = client.Execute(request);

            if (response.IsSuccessful)
            {
                auth_response = JsonConvert.DeserializeObject<SalesforceAuthenticationResponse>(response.Content);
                Console.WriteLine("Successful authentication");
                return true;
            }
            else
            {
                Console.WriteLine("Failed Authentication");
                Console.WriteLine(response.Content);
                return false;
            }
        }


        ////////////////////////////////////////////////////////////////////
        //  EASY LINK ORDER METHODS
        ////////////////////////////////////////////////////////////////////

        //New
        public EasyLinkResponse SendNewOrderRequest(EasyLinkOrder.OrderType orderType, EasyLinkOrder order)
        {
            //Initialize our RestRequest and set the method and parameters
            RestRequest request = new RestRequest();
            request.Method = Method.POST;
            //request format must be set before we add the body
            request.RequestFormat = DataFormat.Json;
            request.AddBody(order);

            return SendRequest(orderType, request);
        }
        //Update
        public EasyLinkResponse SendUpdateOrderRequest(EasyLinkOrder.OrderType orderType, EasyLinkOrder order)
        {
            //Initialize our RestRequest and set the method and parameters
            RestRequest request = new RestRequest();
            request.Method = Method.PATCH;
            request.AddQueryParameter("Record_ExternalName", order.Record_ExternalName);
            request.AddQueryParameter("Record_Name", order.Record_Name);
            //request format must be set before we add the body
            request.RequestFormat = DataFormat.Json;
            request.AddBody(order);

            return SendRequest(orderType, request);
        }
        //Get
        public EasyLinkResponse SendGetOrderRequest(EasyLinkOrder.OrderType orderType, string Record_Name, string Record_ExternalName)
        {
            //Initialize our RestRequest and set the method and parameters
            RestRequest request = new RestRequest();
            request.Method = Method.GET;
            request.AddQueryParameter("Record_ExternalName", Record_ExternalName);
            request.AddQueryParameter("Record_Name", Record_Name);

            return SendRequest(orderType, request);
        }
        //Get Bulk
        public EasyLinkResponse SendGetBulkOrderRequest(EasyLinkOrder.OrderType orderType, string StartDateString, string EndDateString, string StatusesString, string HeaderFieldsString, string LineItemFieldsString)
        {
            //Initialize our RestRequest and set the method and parameters
            RestRequest request = new RestRequest();
            request.Method = Method.GET;
            //If you are an external user, this will be overwritten by the company code associated with your profile
            request.AddQueryParameter("Company"         , "PRE"                         ); 
            request.AddQueryParameter("Bulk"            , "true"                        );
            request.AddQueryParameter("StartDate"       , "\"" + StartDateString + "\"" );
            request.AddQueryParameter("EndDate"         , "\"" + EndDateString + "\""   );
            request.AddQueryParameter("Statuses"        , StatusesString                );
            request.AddQueryParameter("HeaderFields"    , HeaderFieldsString            ); // "Record_Status,Record_Name,WMS_Record_Name,OwnerCode"
            request.AddQueryParameter("LineItemFields"  , LineItemFieldsString          ); // "WMS_RecordLine_Number"

            return SendRequest(orderType, request);
        }

        //Handles sending the RestRequest and re-validating if the session id has expired
        public int retryCount = 0;
        public EasyLinkResponse SendRequest(EasyLinkOrder.OrderType orderType, RestRequest request)
        {
            //initialize our rest client
            RestClient client = new RestClient();
            string endpoint = auth_response.instance_url;
            //Set the endpoint based on the OrderType
            if (orderType == EasyLinkOrder.OrderType.Inbound) endpoint += InboundSuffix;
            if (orderType == EasyLinkOrder.OrderType.Outbound) endpoint += OutboundSuffix;
            client.BaseUrl = new System.Uri(endpoint);

            //Update our request with standard parameters
            request.AddOrUpdateParameter("Authorization", "Bearer " + auth_response.access_token, ParameterType.HttpHeader);
            request.AddHeader("cache-control", "no-cache");


            //Execute our request
            Console.WriteLine("Sending Request");
            IRestResponse response = client.Execute(request);

            //Handle the response
            if (response.IsSuccessful)
            {
                retryCount = 0;
                //Deserialize the successful response and return it
                JsonSerializerSettings jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                //Console.WriteLine(response.Content);

                EasyLinkResponse easyLinkResponse = JsonConvert.DeserializeObject<EasyLinkResponse>(response.Content, jsonSettings);

                Console.WriteLine("STATUS     : " + easyLinkResponse.status           );
                Console.WriteLine("MESSAGE    : " + easyLinkResponse.message          );

                return easyLinkResponse;
            }
            else
            {
                //Try to send again if the credentials weren't valid anymore
                if (response.Content.Contains("INVALID_SESSION_ID") && retryCount < 3)
                {
                    retryCount++;
                    ValidateCredentials();
                    return SendRequest(orderType, request);
                }

                //Build a failed request response to return
                EasyLinkResponse easyLinkResponse = new EasyLinkResponse();
                easyLinkResponse.status = "FAILED_API_REQUEST";
                easyLinkResponse.message = "Something went wrong " + response.Content;
                Console.WriteLine(JsonConvert.SerializeObject(response));
                return easyLinkResponse;
            }


        }
    }
}
