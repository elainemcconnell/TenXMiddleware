using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Text;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuthService
{
    public class PushService
    {
        /*
         * Role of server
         * communicate with clients
         * able to send properly formatted requests to the GCM connection server
         * able to handle requests and resend them using exponential back-off
         * able to securely store the server key and client registration tokens
         * 
         * GCM Connection Server Protocol
         * enable server to interact with GCM connection servers
         * HTTP is fine in our case as we only want to send downstream messages - synchronous
         * JSON messages & plain text messages sent as HTTP POST
         * Multicast downstream supported in JSON message format
        */

        // deviceToken is the client registration token
        // message is the message to be sent in the notification
        public static string SendNotification(string deviceToken, string message)
        {
            // server key
            string GoogleAppID = "AIzaSyCmjfLbwq8jT4askR9yIKv-To5ZmUDd50g";
            string SENDER_ID = "483555755914";

            var jGcmData = new JObject();
            var jData = new JObject();

            jData.Add("message", message);
            jData.Add("name", SENDER_ID);
            jGcmData.Add("to", deviceToken);
            jGcmData.Add("data", jData);
              
            // set up connection to GCM
            WebRequest tRequest = WebRequest.Create("https://android.googleapis.com/gcm/send");
            tRequest.Method = "post";
            tRequest.ContentType = " application/json";
            tRequest.Headers.Add(string.Format("Authorization: key={0}", GoogleAppID));

            // set up proxy
            if (WebRequest.GetSystemWebProxy().GetProxy(tRequest.RequestUri) != tRequest.RequestUri)
            {
                tRequest.Proxy = WebRequest.GetSystemWebProxy();
                tRequest.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            }

            Byte[] byteArray = Encoding.UTF8.GetBytes(jGcmData.ToString());
            tRequest.ContentLength = byteArray.Length;

            Stream dataStream = tRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse tResponse = tRequest.GetResponse();
            dataStream = tResponse.GetResponseStream();
            StreamReader tReader = new StreamReader(dataStream);
            String sResponseFromServer = tReader.ReadToEnd();
        
            tReader.Close();
            dataStream.Close();
            tResponse.Close();
            return sResponseFromServer;
        }
    }
}