using HltvRss.GCM.Class;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace HltvRss.GCM
{
    class GCMService
    {

        public static void SendNewMatchNoticifation(GCMObject data)
        { 
            JavaScriptSerializer serial = new JavaScriptSerializer();
            string postData = serial.Serialize(data.data);
            string json = "{ \"to\":  \"/topics/added\" , \"data\":" + postData + "}";
            Console.WriteLine(json);
            send(json);
        }

        public static void SendToTopic(GCMObject data, String topic)
        {
            JavaScriptSerializer serial = new JavaScriptSerializer();
            string postData = serial.Serialize(data.data);
            string json = "{ \"to\":  \"/topics/" + topic + "\" , \"data\":" + postData + "}";
            Console.WriteLine(json);
            send(json);
        }

        public static void SendNotification(GCMObject data)
        {
            JavaScriptSerializer serial = new JavaScriptSerializer();
            string postData = serial.Serialize(data);
            send(postData);

        } 

        private static void send(String msg)
        {
            var applicationID = "abcdefghijklmnopqrstuvwxyz1234567890";
            var SENDER_ID = "1234567890";

            WebRequest tRequest;
            tRequest = WebRequest.Create("https://gcm-http.googleapis.com/gcm/send");
            tRequest.Method = "post";
            tRequest.ContentType = "application/json";
            tRequest.Headers.Add(string.Format("Authorization: key={0}", applicationID));

            tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));
            Console.WriteLine(msg + "\n");
            Byte[] byteArray = Encoding.UTF8.GetBytes(msg);
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
        }

    }
}
