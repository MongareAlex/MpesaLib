# MpesaLib 
[![mpesalib MyGet Build Status](https://www.myget.org/BuildSource/Badge/mpesalib?identifier=cf0f8e5c-2a40-41cf-8065-9f27db7e2678)](https://www.myget.org/) [![Build Status](https://geospartan.visualstudio.com/MpesaLib/_apis/build/status/ayiemba.MpesaLib)](https://geospartan.visualstudio.com/MpesaLib/_build/latest?definitionId=2)
 
MPESA API LIBRARY For C# Developers

* This documentation is meant to help you get started on how to use this library and does not explain MPESA APIs and there internal workings or exemplifications of when and where you might want to use any of them. If you need some indepth explanation on how these Mpesa APIs work you can checkout this link ---> https://peternjeru.co.ke/safdaraja. Otherwise Safaricoms developer portal should get you sufficient detail.[Safaricom's Developer Portal](https://developer.safaricom.co.ke/apis-explorer)


## 1. HOW TO USE In an ASP.NET Core Web Application

* Run `Install-Package MpesaLib -Version 1.X.X` in Package Manager Console or go to Manage Nuget Packages, Search and install MpesaLib
* Add usings - these are the only namespaces you'll ever need from MpesaLib

```c# 
using MpesaLib.Clients; //gives you the clients
using MpesaLib.Interfaces; //gives you the interfaces for use in DI
using MpesaLib.Models; //gives the DTOs for each client

```
### Option 1 on how to add the services using Dependency Injection: 
* Add Mpesa API Clients in DI Container; For asp.net core core this can be done in Startup.cs.
There are about 10 mpesa api clients. Only register and use the specific ones you need in your code. If you need more than 3 clients follow Option 2 to add them in startup.cs in a cleaner way.

```c#
    //Add AuthClient - gets you accesstokens (This is manadatory)
    services.AddHttpClient<IAuthClient, AuthClient>();
    
    //Add LipaNaMpesaOnlineClient - makes STK Push payment requests
    services.AddHttpClient<ILipaNaMpesaOnlineClient, LipaNaMpesaOnlineClient>();
    
    //Add C2BRegisterUrlClient - register your callback URLS, goes hand-in-hand with the C2BClient
    services.AddHttpClient<IC2BRegisterUrlClient, C2BRegisterUrlClient>();
    
    //Add C2BClient - makes customer to business payment requests 
    services.AddHttpClient<IC2BClient, C2BClient>();
    
    //Add B2BClient - makes business to business payment requests
    services.AddHttpClient<IB2BClient, B2BClient>();
    
    //Add B2CClient - makes business to customer payment requests
    services.AddHttpClient<IB2CClient, B2CClient>();
    
    //Add LipaNaMpesaQueryClient - Query status of a LipaNaMpesaOnline Payment request
    services.AddHttpClient<ILipaNaMpesaQueryClient, LipaNaMpesaQueryClient>();
    
    //Add TransactionReversalClient - Reverses Mpesa transactions
    services.AddHttpClient<ITransactionReversalClient, TransactionReversalClient>();
    
    //Add TransactionStatusClient - Query status of transaction requests
    services.AddHttpClient<ITransactionStatusClient, TransactionStatusClient>();  
    
     //Add AccountBalanceQueryCient - Query Mpesa balance
    services.AddHttpClient<IAccountBalanceQueryCient, AccountBalanceQueryCient>(); 
    

```
### Option 2 on how to add the services using dependency Injection:
* Option 1 above is not very clean since your startup class might get littered with too many services. To solve this, i use extention methods. The idea is to abstract these services behind one method so that we just do ```services.AddMpesaSupport()```. This makes the startup class much cleaner. To achieve this use the IserviceCollection interface available in ASP.NET Core. Here is a sample...

```c#
using Microsoft.Extensions.DependencyInjection;
using MpesaLib.Clients;
using MpesaLib.Interfaces;

namespace YourWebApp.Extensions
{
    public static class MpesaExtentions
    {
        public static void AddMpesaSupport(this IServiceCollection services)
        {
            //Add Mpesa Clients
            services.AddHttpClient<IAuthClient, AuthClient>();
            services.AddHttpClient<ILipaNaMpesaOnlineClient, LipaNaMpesaOnlineClient>();
            services.AddHttpClient<IAccountBalanceQueryClient, AccountBalanceQueryClient>();
            services.AddHttpClient<IC2BClient, C2BClient>();
            services.AddHttpClient<IB2BClient, B2BClient>();
            services.AddHttpClient<IB2CClient, B2CClient>();
            services.AddHttpClient<IC2BRegisterUrlClient, C2BRegisterUrlClient>();
            services.AddHttpClient<ILipaNaMpesaQueryClient, LipaNaMpesaQueryClient>();
            services.AddHttpClient<ITransactionStatusClient, TransactionStatusClient>();
            services.AddHttpClient<ITransactionReversalClient, TransactionReversalClient>();
        }
    }
}

```

Then in Startup.cs just add ```using YourWebApp.Extensions``` followed by ```services.AddMpesaSupport();```

* Inject the clients in the constructor of your controller or any class that makes the api calls... (in this case i only need AuthClient and LipaNaMpesaOnlineClient. I store my API Keys and secrets in a configuration file and inject them into the necessary class using ```IConfiguration``` interface.


```c#
    public class PaymentsController : Controller
    {
        private readonly IAuthClient _auth;
        private ILipaNaMpesaOnlineClient _lipaNaMpesa;
        private readonly IConfiguration _config;

        public PaymentsController(IAuthClient auth, ILipaNaMpesaOnlineClient lipaNampesa, IConfiguration configuration)
        {
            _auth = auth;
            _lipaNaMpesa = lipaNampesa;
            _config = configuration;
        }
        ...
        //Code omitted for brevity
```


* You can store your ConsumerKey and ConsumerSecret in appsettings.json as follows

```json
     "MpesaConfiguration": {
         "ConsumerKey": "[Your Mpesa ConsumerKey from daraja]",
         "ConsumerSecret": "[Your Mpesa ConsumerSecret from daraja]"
       }
```

* First generate an `accesstoken` using the AuthClient as follows

```c#
        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            var consumerKey = _config["MpesaConfiguration:ConsumerKey"];

            var consumerSecret = _config["MpesaConfiguration:ConsumerSecret"];
            
            
            //You can override AuthClients sandbox url when moving to production as follows
            _auth.BaseUri = "Mpesa api production Url provided by safaricom after completing GO live process"; //Only do this when moving to production, otherwise comment oout this line when in development or add an environment checker and execute the correct code
            var accesstoken = await _auth.GetToken(consumerKey,consumerSecret);

            ...
        //code omitted for brevity
```
## Note
* *Access tokens generated by the [Authclient](https://github.com/ayiemba/MpesaLib/blob/master/src/MpesaLib/Clients/AuthClient.cs) expire after an hour (this is documented in Daraja).* 

![Accesstoken Expirition period](screenshots/accesstoken.png)

* *Users of MpesaLib should ensure they handle token expriration in their code. A quick solution would be to put the semaphore in a try/catch/finally block as documented in [this question](https://stackoverflow.com/questions/49304326/refresh-token-using-static-httpclient) from stackoverflow.*

* To Send payment Request using LipaNaMpesaOnline, initialize the LipaNaMpesaOnline data transfer object as follows. The DTOs are in namespace ```using MpesaLib.Models;```

```c#
      LipaNaMpesaOnline lipaonline = new LipaNaMpesaOnline
      {
          AccountReference = "test",
          Amount = "1",
          PartyA = "254708374149",
          PartyB = "174379",
          BusinessShortCode = "174379",
          CallBackURL = "[your callback url, i wish i could help but you'll have to write your own]",
          Password = "daraga explains on how to get password",
          PhoneNumber = "254708374149",
          Timestamp = "20180716124916",//DateTime.Now.ToString("yyyyMMddHHmmss"),
          TransactionDesc = "test"
          TransactionType = "CustomerPayBillOnline" //I am using this by default, you might wanna check the other option
      };
```

* You can then make a payment request in your controller as follows...

```c#
//When Moving to production you need to overrride sandbox urls that are used by default before making api call you could probly wrap your call in an if statement that checks your environment before making api call
//To overrride sandbox url set the BaseUri property of the Mpesa Api client you are calling
_lipaNaMpesa.BaseUri = "https://api.safaricom.co.ke/mpesa/stkpush/v1/processrequest"; //only do this in production and ensure to get correct urls from safaricom after completing the GO Live process in daraja

//Otherwise proceed and make call to api
var paymentrequest = await _lipaNaMpesa.MakePayment(lipaonline, accesstoken);
```

* (Not Recommended) - If you dont want to use Dependency Injection you can just New-Up the clients and use them like this..
```c#


   var httpClient = new HttpClient(); //required, comes from System.Net.Http or Microsoft.Extensions.Http

   LipaNaMpesaOnlineClient LipaNaMpesa = new LipaNaMpesaOnlineClient(httpClient); //you have to pass in an instance of httpClient

   ...
   ...
   var paymentrequest = await LipaNaMpesa.MakePayment(lipaonline, accesstoken);
```



* Do whatever you want with the results of the request...


## 2. A quick and dirty Way to test Using Console App:


```c#
using MpesaLib.Clients;
using MpesaLib.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        

        static void  Main(string[] args)
        {
            Console.WriteLine("Mpesa API Test ..."); 
            
            MakePaymentAsync().GetAwaiter().GetResult(); 

            Console.WriteLine("Press Any Key to Exit..");

            Console.ReadKey();

        }

        static async Task<string> MakePaymentAsync()
        {
            string ConsumerSecret = "your consumer secret from daraja";
            string ConsumerKey = "your consumer key from daraja";

            var httpClient = new HttpClient(); //this is needed by each client

           
            AuthClient Auth = new AuthClient(httpClient);   //Your have to pass in httpClient to all the MpesaLib clients.        

            string accesstoken = await Auth.GetToken(ConsumerKey, ConsumerSecret); //this will get you a token

            var LipaNaMpesaOnline = new LipaNaMpesaOnline
            {
                AccountReference = "test",
                Amount = "1",
                PartyA = "2547xxxxxxxx", //replace with your number
                PartyB = "174379",
                BusinessShortCode = "174379",

                CallBackURL = "https://use-your-own-callback-url/api/callback", //you should implement your own callback url, can be an api controller with a post method taking in a JToken

                Password = "use your own password",
                PhoneNumber = "254xxxxxxx", //same as PartyA
                Timestamp = "20180716124916", // replace with timestamp used to generate password
                TransactionDesc = "test"

            };

            LipaNaMpesaOnlineClient lipaonline = new LipaNaMpesaOnlineClient(httpClient);   //initialize the LipaNaMpesaOnlineClient()                

            var paymentdata = lipaonline.MakePayment(LipaNaMpesaOnline, accesstoken);// this will make the STK Push and if you use your personal number you should see that on your phone. If you complete the payment it will be reversed.           

            return paymentdata.ToString(); // you can return or log to console, in a real app there is plenty that you still need to do 
        }

    }
}

```
You should see the following from your phone if you configured everything propoerly...

![STK Push Screen](screenshots/stkpush.png)
