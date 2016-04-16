using HltvRss.Classes;
using HltvRss.GCM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace HltvRss.Server
{
    class SocketCommands
    {

        public static int RequestedMatches(Client c, String msg, List<Match> hot, List<Match> recent)
        {
            int size = 0;


            MatchCollectionItem obj = new MatchCollectionItem();
            obj.HotMatches = hot;
            obj.LatestResults = recent;


            JavaScriptSerializer serial = new JavaScriptSerializer();
            String json = serial.Serialize(obj);
            size = json.Length;
            
            c.Send(json + "\n");
            return size;
        }

        public static int RequestedMatchesNew(Client c, String appVersion, List<Match> hot, List<Match> recent)
        {
            int size = 0;

            ResultItem ri = new ResultItem();
            MatchCollectionItem obj = new MatchCollectionItem();

            if (appVersion.Equals("2.4") || appVersion.Equals("2.4.5"))
            {
                for (int i = 0; i < hot.Count(); i++)
                {
                    hot[i].hasScorebot = false; //Stänger av denna för version < 2.5 (inte fullt implementerad i appen för ens v.2.5)
                }
            }

            obj.HotMatches = hot;
            obj.LatestResults = recent;
            ri.Request = App.CMD_REQUEST_MATCHES;
            ri.Data = obj;

            JavaScriptSerializer serial = new JavaScriptSerializer();
            String json = serial.Serialize(ri);
            size = json.Length;
            c.Send(json + "\n");
            return size;
        }

        public static int DataStatus(Client c, bool needData)
        {
            int size = 0;
            ResultItem ri = new ResultItem();
            ResultStatusItem rsi = new ResultStatusItem();

            rsi.Status = (needData ? "true" : "false");
            ri.Request = App.CMD_REQUEST_DATA_STATUS;
            ri.Data = rsi;


            JavaScriptSerializer serial = new JavaScriptSerializer();
            String json = serial.Serialize(ri);
            size = json.Length;

            c.Send(json + "\n");
            return size;
        }

        public static int DataReceived(Client c, string request)
        {
            int size = 0;
            ResultItem ri = new ResultItem();
            ResultStatusItem rsi = new ResultStatusItem();

            rsi.Status = "true";
            ri.Request = request;
            ri.Data = rsi;


            JavaScriptSerializer serial = new JavaScriptSerializer();
            String json = serial.Serialize(ri);
            size = json.Length;

            c.Send(json + "\n");
            return size;
        }

        public static int WatchStatusNew(MySQL sql, Client c, string matchID, string token)
        {
            int size = 0;
            ResultItem ri = new ResultItem();
            ResultStatusItem rsi = new ResultStatusItem();
            bool exist = sql.isMatchWatched(token, matchID);

            rsi.Status = (exist ? "true" : "false");
            ri.Request = App.CMD_REQUEST_WATCH_STATUS;
            ri.Data = rsi;


            JavaScriptSerializer serial = new JavaScriptSerializer();
            String json = serial.Serialize(ri);
            size = json.Length;

            c.Send(json + "\n");
            return size;
        }


        public static int WatchStatus(MySQL sql, Client c, string matchID, string token)
        {
            int size = 0;
            bool exist = sql.isMatchWatched(token, matchID);
            String send = App.CMD_REQUEST_WATCH_STATUS + (exist ? "true\n" : "false\n");
            size = send.Length;
            c.Send(send);
            return size;
        }


        public static int RegWatch(MySQL sql, Client c, string matchID, string token)
        {
            int size = 0;

            int result = sql.InsertMatch(token, matchID);
            switch (result)
            {
                case App.GCM_REGISTER_SUCCESS:
                    size = (App.CMD_REQUEST_REGISTER_WATCH + "OK\n").Length;
                    c.Send(App.CMD_REQUEST_REGISTER_WATCH + "OK\n");
                    break;
                case App.GCM_REGISTER_KEY_ALREADY_EXISTS:
                    size = (App.CMD_REQUEST_REGISTER_WATCH + "EXISTS\n").Length;
                    c.Send(App.CMD_REQUEST_REGISTER_WATCH + "EXISTS\n");
                    break;
                case App.GCM_REGISTER_ERROR:
                    size = (App.CMD_REQUEST_REGISTER_WATCH + "ERROR\n").Length;
                    c.Send(App.CMD_REQUEST_REGISTER_WATCH + "ERROR\n");
                    break;
            }

            return size;
        }


        public static int RegWatchNew(MySQL sql, Client c, string matchID, string token)
        {

            ResultItem ri = new ResultItem();
            ResultStatusItem rsi = new ResultStatusItem();

            ri.Request = App.CMD_REQUEST_REGISTER_WATCH;

            int result = sql.InsertMatch(token, matchID);
            switch (result)
            {
                case App.GCM_REGISTER_SUCCESS:
                    rsi.Status = "OK";
                    break;
                case App.GCM_REGISTER_KEY_ALREADY_EXISTS:
                    rsi.Status = "EXISTS";
                    break;
                case App.GCM_REGISTER_ERROR:
                    rsi.Status = "ERROR";
                    break;
            }
            ri.Data = rsi;

            JavaScriptSerializer serial = new JavaScriptSerializer();
            String json = serial.Serialize(ri);
            c.Send(json + "\n");
            return json.Length;
        }


        public static int UnregWatch(MySQL sql, Client c, string matchID, string token)
        {
            int size = 0;

            int result = sql.RemoveMatch(token, matchID);
            switch (result)
            {
                case App.GCM_REGISTER_SUCCESS:
                    size = (App.CMD_REQUEST_UNREGISTER_WATCH + "OK\n").Length;
                    c.Send(App.CMD_REQUEST_UNREGISTER_WATCH + "OK\n");
                    break;
                case App.GCM_REGISTER_KEY_ALREADY_EXISTS:
                    size = (App.CMD_REQUEST_UNREGISTER_WATCH + "EXISTS\n").Length;
                    c.Send(App.CMD_REQUEST_UNREGISTER_WATCH + "EXISTS\n");
                    break;
                case App.GCM_REGISTER_ERROR:
                    size = (App.CMD_REQUEST_UNREGISTER_WATCH + "ERROR\n").Length;
                    c.Send(App.CMD_REQUEST_UNREGISTER_WATCH + "ERROR\n");
                    break;
            }

            return size;
        }


        public static int UnregWatchNew(MySQL sql, Client c, string matchID, string token)
        {
            int result = sql.RemoveMatch(token, matchID);
            ResultItem ri = new ResultItem();
            ResultStatusItem rsi = new ResultStatusItem();

            ri.Request = App.CMD_REQUEST_UNREGISTER_WATCH;
            switch (result)
            {
                case App.GCM_REGISTER_SUCCESS:
                    rsi.Status = "OK";
                    break;
                case App.GCM_REGISTER_KEY_ALREADY_EXISTS:
                    rsi.Status = "EXISTS";
                    break;
                case App.GCM_REGISTER_ERROR:
                    rsi.Status = "ERROR";
                    break;
            }
            ri.Data = rsi;

            JavaScriptSerializer serial = new JavaScriptSerializer();
            String json = serial.Serialize(ri);
            c.Send(json + "\n");
            return json.Length;
        }

        public static int UnregGCM(MySQL sql, Client c, string token)
        {
            int size = 0;

            int result = sql.RemoveDevice(token);
            switch (result)
            {
                case App.GCM_UNREG_SUCCESS:
                    size = (App.CMD_REQUEST_UNREGISTER_GCM + "OK\n").Length;
                    c.Send(App.CMD_REQUEST_UNREGISTER_GCM + "OK\n");
                    break;
                case App.GCM_UNREG_NOT_FOUND:
                    size = (App.CMD_REQUEST_UNREGISTER_GCM + "NOT_FOUND\n").Length;
                    c.Send(App.CMD_REQUEST_UNREGISTER_GCM + "NOT_FOUND\n");
                    break;
                case App.GCM_UNREG_ERROR:
                    size = (App.CMD_REQUEST_UNREGISTER_GCM + "ERROR\n").Length;
                    c.Send(App.CMD_REQUEST_UNREGISTER_GCM + "ERROR\n");
                    break;
            }

            return size;
        }

        public static int UnregGCMNew(MySQL sql, Client c, string token)
        {
            ResultItem ri = new ResultItem();
            ResultStatusItem rsi = new ResultStatusItem();

            ri.Request = App.CMD_REQUEST_UNREGISTER_GCM;

            int result = sql.RemoveDevice(token);
            switch (result)
            {
                case App.GCM_UNREG_SUCCESS:
                    rsi.Status = "OK";
                    break;
                case App.GCM_UNREG_NOT_FOUND:
                    rsi.Status = "NOT_FOUND";
                    break;
                case App.GCM_UNREG_ERROR:
                    rsi.Status = "ERROR";
                    break;
            }

            ri.Data = rsi;

            JavaScriptSerializer serial = new JavaScriptSerializer();
            String json = serial.Serialize(ri);
            c.Send(json + "\n");
            return json.Length;
        }

        public static int RegisterGCM(MySQL sql, Client c, string email, string token)
        {
            int size = 0;
            int result = sql.RegisterKey(token, email);
            switch (result)
            {
                case App.GCM_REGISTER_SUCCESS:
                    //Should we notify that client?
                    size = "OK\n".Length;
                    c.Send("OK\n");
                    break;
                case App.GCM_REGISTER_KEY_ALREADY_EXISTS:
                    //Should we notify that client?
                    size = "EXISTS\n".Length;
                    c.Send("EXISTS\n");
                    break;
                case App.GCM_REGISTER_ERROR:
                    //Should we notify that client?
                    size = "ERROR\n".Length;
                    c.Send("ERROR\n");
                    break;
            }

            return size;

        }

        public static int RegisterGCMNew(MySQL sql, Client c, string email, string token)
        {
            int result = sql.RegisterKey(token, email);
            ResultItem ri = new ResultItem();
            ResultStatusItem rsi = new ResultStatusItem();

            ri.Request = App.CMD_REQUEST_REGISTER_GCM;

            switch (result)
            {
                case App.GCM_REGISTER_SUCCESS:
                    rsi.Status = "OK";
                    break;
                case App.GCM_REGISTER_KEY_ALREADY_EXISTS:
                    rsi.Status = "EXISTS";
                    break;
                case App.GCM_REGISTER_ERROR:
                    rsi.Status = "ERROR";
                    break;
            }
            ri.Data = rsi;

            JavaScriptSerializer serial = new JavaScriptSerializer();
            String json = serial.Serialize(ri);
            c.Send(json + "\n");
            return json.Length;
        }

        public static int UpdateGcmToken(MySQL sql, Client c, String oldToken, string newToken) //Outdated!
        {
            int result = sql.UpdateDeviceKey(oldToken, newToken);
            int size = 0;
            switch (result)
            {
                case App.GCM_UPDATE_SUCCESS:
                    c.Send("OK\n");
                    size = "OK\n".Length;
                    break;
                default:
                    c.Send("ERROR\n");
                    size = "ERROR\n".Length;
                    break;
            }
            return size;
        }


        public static int UpdateGcmTokenNew(MySQL sql, Client c, String oldToken, string newToken)
        {
            int result = sql.UpdateDeviceKey(oldToken, newToken);

            ResultItem ri = new ResultItem();
            ResultStatusItem rsi = new ResultStatusItem();
            ri.Request = App.CMD_REQUEST_UPDATE_GCM_TOKEN;

            switch (result)
            {
                case App.GCM_UPDATE_SUCCESS:
                    rsi.Status = "OK";
                    break;
                default:
                    rsi.Status = "ERROR";
                    break;
            }
            ri.Data = rsi;

            JavaScriptSerializer serial = new JavaScriptSerializer();
            String json = serial.Serialize(ri);
            c.Send(json + "\n");
            return json.Length;
        }

    }
}
