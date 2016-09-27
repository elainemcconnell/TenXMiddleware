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

namespace AuthService
{
    public class PushService
    {

        static void Main(string[] args)
        {
            // receive registration token as JSON object

            string deviceToken = "f5radYxgXU4:APA91bGnw6g3ESFoBJAz3AwiCzHeX-oRkOf820Kp1wvV2Zb83QQDie8YdRYHzcijZ1ZcVb06xEY60cgDV-ZOvEs_GA4K1wQqT4mRLMnfkitjiVNE-kAct0lqdLh4xAMXv5-DGGSg5yMQ";
            string message = "Middle Tier Notifications test";
            //sendNotification(deviceToken, message);
        }

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
            var SENDER_ID = "483555755914";
            var value = message;


            // set up connection to GCM
            WebRequest tRequest = WebRequest.Create("https://android.googleapis.com/gcm/send");
            tRequest.Method = "post";
            tRequest.ContentType = " application/json";
            tRequest.Headers.Add(string.Format("Authorization: key={0}", GoogleAppID));
            tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));

            // set up proxy
            //WebProxy myproxy = new WebProxy("ilproxy1.europa.internal", 8080);
            //tRequest.Proxy = myproxy;           
        
            string postData = "collapse_key=score_update&time_to_live=108&delay_while_idle=1&data.message=" + value + "&data.time=" + System.DateTime.Now.ToString() + "®istration_id=" + deviceToken + "";
            Console.WriteLine(postData);
            Byte[] byteArray = Encoding.UTF8.GetBytes(postData);


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