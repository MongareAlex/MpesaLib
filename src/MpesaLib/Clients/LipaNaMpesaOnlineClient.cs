﻿using MpesaLib.Interfaces;
using MpesaLib.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MpesaLib.Clients
{
    /// <summary>
    /// Instantiate this class to make a Lipa Na Mpesa Online payment request
    /// </summary>
    public class LipaNaMpesaOnlineClient : ILipaNaMpesaOnlineClient
    {
        private readonly HttpClient _httpclient;
        public LipaNaMpesaOnlineClient(HttpClient httpClient)
        {
            _httpclient = httpClient;
        }

        /// <summary>
        /// Defaults to sandbox URL. Override with Mpesa production url when deploying to a production environment.
        /// </summary>
        public string BaseUri { get; set; } = "https://sandbox.safaricom.co.ke/mpesa/stkpush/v1/processrequest";

        /// <summary>
        /// Make a Lipa Na Mpesa Online payment 
        /// </summary>
        /// <param name="mpesaItem">Lipa Na Mpesa Online payment request object</param>
        /// <param name="accesstoken">Access token generated by AuthClient</param>
        /// <returns>Returns a JSON string that you should deserialize</returns>
        public async Task<string> MakePayment(LipaNaMpesaOnline mpesaItem, string accesstoken)
        {
            Uri BaseAddress = new Uri(BaseUri);
            _httpclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accesstoken);           

            var values = new Dictionary<string, string>
            {
                { "BusinessShortCode", mpesaItem.BusinessShortCode },
                { "Password", mpesaItem.Password },
                { "Timestamp", mpesaItem.Timestamp },
                { "TransactionType", mpesaItem.TransactionType },
                { "Amount", mpesaItem.Amount },
                { "PartyA", mpesaItem.PartyA },
                { "PartyB", mpesaItem.PartyB },
                { "PhoneNumber", mpesaItem.PhoneNumber },
                { "CallBackURL", mpesaItem.CallBackURL },
                { "AccountReference", mpesaItem.AccountReference },
                { "TransactionDesc", mpesaItem.TransactionDesc }
            };
           
            var jsonvalues = JsonConvert.SerializeObject(values);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BaseAddress)
            {
                Content = new StringContent(jsonvalues.ToString(), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await _httpclient.SendAsync(request);

            Console.WriteLine("This is the request data: " + jsonvalues.ToString());

            return response.Content.ReadAsStringAsync().Result;

        }
    }
}
