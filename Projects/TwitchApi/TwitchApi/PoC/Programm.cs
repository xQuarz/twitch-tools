using ApiTest;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Diagnostics;
using System.Net;
using TwitchApi;


var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

string oAuthClientId = config["oauth:client-id"];
string oAuthClientSecret = config["oauth:client-secret"];
string oAuthTokenUrl = "https://id.twitch.tv/oauth2/token";
string oAuthAccessTokenUrl = "https://id.twitch.tv/oauth2/authorize";
string redirectUri = config["uri:redirect"];

string? streamerLogin = null;

// Use just one example! In example client credentials the api call for clips won't work (line 111-124).

// --------------------------------------
// 
// Start: Example with client credentials
//
// --------------------------------------

// var client = new RestClient(oAuthTokenUrl);
// var request = new RestRequest("", Method.Post);
// request.AddHeader("content-type", "application/x-www-form-urlencoded");
// request.AddParameter("grant_type", "client_credentials").AddParameter("client_id", oAuthClientId).AddParameter("client_secret", oAuthClientSecret);
// RestResponse response = client.Execute(request);

// --------------------------------------
// 
// End: Example with client credentials
//
// --------------------------------------



// --------------------------------------
//
// Start: Example with user credentials
// 
// --------------------------------------
var client0 = new RestClient(oAuthAccessTokenUrl);
var request0 = new RestRequest();
request0.AddParameter("response_type", "code").AddParameter("client_id", oAuthClientId).AddParameter("redirect_uri", redirectUri).AddParameter("scope", "user:read:email");
RestResponse response0 = await client0.ExecuteAsync(request0);

if (response0.ResponseUri != null)
{
    Process.Start(startInfo: new ProcessStartInfo { FileName = response0.ResponseUri.OriginalString, UseShellExecute = true });
} else
{
    throw new Exception("Redirect to twitch failed!");
}


HttpListener listener = new HttpListener();
listener.Prefixes.Add(redirectUri + "/");
listener.Start();
var authorizationCode = "";

var resultForCode = listener.BeginGetContext(new AsyncCallback((result) =>
{
    if (listener.IsListening)
    {
        var context = listener.EndGetContext(result);
        var request = context.Request;

        if (request.Url != null)
        {
            string[] queryParts = request.Url.Query.Split('=', '&');
            authorizationCode = queryParts[1];
        } else
        {
            throw new Exception("No url with authorization code!");
        }
        
    }
}), listener);

resultForCode.AsyncWaitHandle.WaitOne();
listener.Close();

Console.WriteLine("Code: " + authorizationCode);

var client = new RestClient(oAuthTokenUrl);
var request = new RestRequest("", Method.Post);
request.AddHeader("content-type", "application/x-www-form-urlencoded");
request.AddParameter("grant_type", "authorization_code").AddParameter("client_id", oAuthClientId).AddParameter("client_secret", oAuthClientSecret).AddParameter("code", authorizationCode).AddParameter("redirect_uri", redirectUri);
RestResponse response = await client.ExecuteAsync(request);

// --------------------------------------
//
// End: Example with user credentials
// 
// --------------------------------------


if (response.Content != null)
{
    dynamic resp = JObject.Parse(response.Content);
    string accessToken = resp.access_token;
    Console.WriteLine("Token: " + accessToken);

    var handler = new AuthTokenHttpMessageHandler((req, _) =>
    {
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
    });
    var client1 = new HttpClient(handler);
    var apiInstance = new Client(client1);

    try
    {
        var result = await apiInstance.UsersGETAsync(null, streamerLogin, oAuthClientId);
        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        var enumerator = result.Data.GetEnumerator();
        enumerator.MoveNext();
        string broadcasterId = enumerator.Current.Id;

        var clipsResult = await apiInstance.ClipsGETAsync(broadcasterId, null, null, null, null, null, null, null, null, oAuthClientId);

        if (clipsResult.Data.Count <= 0)
        {
            Process.Start(startInfo: new ProcessStartInfo { FileName = "https://www.youtube.com/embed/NuAKnbIr6TE?autoplay=1&rel=0", UseShellExecute = true });
        }
        else
        {
            Random rnd = new Random();
            int index = rnd.Next(0, clipsResult.Data.Count);
            Console.WriteLine(clipsResult.Data.ToArray()[index].Url);
            Process.Start(startInfo: new ProcessStartInfo { FileName = clipsResult.Data.ToArray()[index].Url, UseShellExecute = true });
        }
        
    } catch (ApiException ex)
    {
        Console.WriteLine("You've taken the wrong direction: " + ex.Message);
        Console.WriteLine("Status Code: " + ex.StatusCode);
        Console.WriteLine(ex.StackTrace);
    }
}