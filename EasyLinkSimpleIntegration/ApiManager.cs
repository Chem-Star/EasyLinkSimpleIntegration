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

        private const string OutboundSuffix = "/services/apexrest/v2/outbound";
        private const string InboundSuffix = "/services/apexrest/v2/inbound";

        private String clientId;
        private String clientSecret;
        private String username;
        private String password;
        private String securityToken;
        private bool isSandbox;

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
        public EasyLinkResponse SubmitEasyLinkOrderRequest(EasyLinkOrder order, EasyLinkOrder.OrderType orderType, Action action)
        {
            return SendEasyLinkOrderRequest(order, orderType, action, "", "");
        }
        public EasyLinkResponse SubmitGetEasyLinkOrderOrderRequest(EasyLinkOrder.OrderType orderType, string Record_Name, string Record_ExternalName)
        {
            return SendEasyLinkOrderRequest(null, orderType, Action.GET, Record_Name, Record_ExternalName);
        }

        private EasyLinkResponse SendEasyLinkOrderRequest(EasyLinkOrder order, EasyLinkOrder.OrderType orderType, Action action, string Record_Name, string Record_ExternalName)
        {
            RestClient client = new RestClient();
            RestRequest request = new RestRequest();
            EasyLinkResponse response = new EasyLinkResponse();
            request.RequestFormat = DataFormat.Json;

            string endpoint = auth_response.instance_url;
            Method method = Method.POST;

            //Set the endpoint based on the OrderType
            switch(orderType)
            {
                case EasyLinkOrder.OrderType.Inbound:
                    endpoint += InboundSuffix;
                    break;
                case EasyLinkOrder.OrderType.Outbound:
                    endpoint += OutboundSuffix;
                    break;
                default:
                    Console.WriteLine("ORDER TYPE ISN'T SUPPORTED CURRENTLY");
                    break;
            }
            
            //Set the method and parameters based on the Action
            switch(action)
            {
                case Action.NEW:
                    method = Method.POST;
                    request.AddBody(order);
                    break;
                case Action.UPDATE:
                    method = Method.PATCH;
                    request.AddQueryParameter("Record_ExternalName", order.Record_ExternalName);
                    request.AddQueryParameter("Record_Name", order.Record_Name);
                    request.AddBody(order);
                    break;
                case Action.GET:
                    method = Method.GET;
                    request.AddQueryParameter("Record_ExternalName", Record_ExternalName);
                    request.AddQueryParameter("Record_Name", Record_Name);
                    break;
                default:
                    Console.WriteLine("METHOD ISN'T SUPPORTED CURRENTLY");
                    break;
            }

            client.BaseUrl = new System.Uri(endpoint);
            request.Method = method;

            response = SendEasyLinkRequest(client, request);
            Console.WriteLine(" == STATUS      : " + response.status);
            Console.WriteLine(" == RECORD NAME : " + response.record_name);
            Console.WriteLine(" == MESSAGE     : " + response.message);
            Console.WriteLine(" == RECORD      : " + JsonConvert.SerializeObject(response.record));

            return response;
        }



        public int retryCount = 0;
        //Handles sending the RestRequest and re-validating if the session id has expired
        public EasyLinkResponse SendEasyLinkRequest(RestClient client, RestRequest request)
        {
            request.AddOrUpdateParameter("Authorization", "Bearer " + auth_response.access_token, ParameterType.HttpHeader);
            request.AddHeader("cache-control", "no-cache");

            IRestResponse response = client.Execute(request);

            if (response.IsSuccessful)
            {
                EasyLinkResponse easyLinkResponse = JsonConvert.DeserializeObject<EasyLinkResponse>(response.Content);

                //if(easyLinkResponse.)

                return easyLinkResponse;
            }else
            {
                if (response.Content.Contains("INVALID_SESSION_ID") && retryCount < 3)
                {
                    retryCount++;
                    ValidateCredentials();
                    return SendEasyLinkRequest(client, request);
                }

                EasyLinkResponse easyLinkResponse = new EasyLinkResponse();
                easyLinkResponse.status = "FAILED_API_REQUEST";
                easyLinkResponse.message = "Something went wrong "+response.Content;
                Console.WriteLine(JsonConvert.SerializeObject(response));
                return easyLinkResponse;
            }


        }
    }
}
