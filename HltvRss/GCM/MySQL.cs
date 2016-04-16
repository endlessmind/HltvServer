using HltvRss.GCM.Class;
using log4net;
using log4net.Config;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace HltvRss.GCM
{
    class MySQL
    {

       static String MyConString = "SERVER=*IP*;DATABASE=*some database name*;UID=*some userID*;PASSWORD=*password*;Pooling=false";
        static MySqlConnection connection;
        static MySqlCommand command;
        static MySqlDataReader Reader;

        ILog log;
        bool connected = false;

        public MySQL()
        {
            log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            XmlConfigurator.Configure();
        }

        public void CloseConnection()
        {
            if (Reader != null && !Reader.IsClosed)
                Reader.Close();

            if (connected)
            {
                connection.CloseAsync();
                connection.Dispose();
            }
            connected = false;
        }

        public void CreateConnection(String s)
        {
            //MyConString = con;
          
            connection = new MySqlConnection(MyConString);
            connection.Open();
            command = connection.CreateCommand();
            connected = true;
        }

        private void OpenIfNeeded()
        {
            if (connection == null || connection.State != System.Data.ConnectionState.Open)
                CreateConnection(MyConString);

            connected = true;
        }

        public int RegisterKey(String key, String email)
        {
            OpenIfNeeded();

            Boolean HasAcc = AccountExist(email);
            int accountID = -1;
            int accResult = -1;
            int devResult = -1;
            if (!HasAcc) //Let's register the "account"
            {
                OpenIfNeeded();
                accResult = InsertAccount(email);
            } else
            {
                accResult = 1;
            }

            if (accResult == 1)
            {
                OpenIfNeeded();
                accountID = GetAccountIDFromEmail(email);
            }
            OpenIfNeeded();
            Boolean hasDevice = DeviceExist(key);
            if (!hasDevice)
            {
                if (accountID > -1)
                {
                    OpenIfNeeded();
                    devResult = InsertDevice(key, accountID);
                }
            } else { devResult = 0;  } //Device alreeady exist.

            CloseConnection();

            switch (devResult)
            {
                case 0:
                    return App.GCM_REGISTER_KEY_ALREADY_EXISTS;
                    break;
                case 1:
                    return App.GCM_REGISTER_SUCCESS;
                    break;
                default:
                    return App.GCM_REGISTER_ERROR;
                    break;
            }

        }

        public int UpdateDeviceKey(String oldKey, String newKey)
        {
            OpenIfNeeded();

            Boolean hasDevice = DeviceExist(oldKey);
            DBDevice dev;
            if (hasDevice)
            {
                OpenIfNeeded();

                dev = GetDeviceFromKey(oldKey)[0];
                int result = UpdateDevice(newKey, dev.ID);
                CloseConnection();
                return result == 1 ? App.GCM_UPDATE_SUCCESS : App.GCM_UPDATE_ERROR;
            } else
            {
                //Something is very wrong. Just ignore for now.
                CloseConnection();
                return App.GCM_UPDATE_ERROR; // negative will always represent and error
            }
           

        }

        private int InsertAccount(String email)
        {
            OpenIfNeeded();

            String query = "INSERT INTO account(email)VALUES('" + email + "')";
            command = new MySqlCommand(query, connection);
            int result = command.ExecuteNonQuery();
            CloseConnection();
            return result;
        }

        private int InsertDevice(String key, int account)
        {
            OpenIfNeeded();

            String query = "INSERT INTO devices(device_key, account_id)VALUES('"+ key + "', '" + account + "')";
            command = new MySqlCommand(query, connection);
            int result = command.ExecuteNonQuery();
            CloseConnection();
            return result;
        }

        public int InsertMatch(String key, String match)
        {
            OpenIfNeeded();
            command.CommandText = "SELECT * FROM devices WHERE device_key = '" + key + "'";
            Reader = command.ExecuteReader();

            if (Reader.HasRows)
            {
                Reader.Read();
                int accountID = Reader.GetInt32(2);
                Reader.Close();
                String query = "INSERT INTO matches(match_id, account_id)VALUES('" + match + "', '" + accountID + "')";
                command = new MySqlCommand(query, connection);
                int result = command.ExecuteNonQuery();
                CloseConnection();
                return result;
            } else
            {
                CloseConnection();
                return 0;
            }


        }

        public int RemoveDevice(String key)
        {
            OpenIfNeeded();
            if (DeviceExist(key))
            {
                OpenIfNeeded();

                String query = "DELETE FROM devices WHERE device_key = '" + key + "'";
                command = new MySqlCommand(query, connection);
                int result = command.ExecuteNonQuery();
                CloseConnection();
                switch (result)
                {
                    case 0:
                        return App.GCM_UNREG_NOT_FOUND;
                        break;
                    case 1:
                        return App.GCM_UNREG_SUCCESS;
                        break;
                    default:
                        return App.GCM_UNREG_ERROR;
                        break;
                }


            } else
            {
                CloseConnection();
                return App.GCM_UNREG_NOT_FOUND;
            }
        }

        public int RemoveMatch(String key, String match)
        {
            OpenIfNeeded();
            command.CommandText = "SELECT * FROM devices WHERE device_key = '" + key + "'";
            Reader = command.ExecuteReader();

            if (Reader.HasRows)
            {
                Reader.Read();
                int accountID = Reader.GetInt32(2);
                Reader.Close();
                String query = "DELETE FROM matches WHERE match_id = '" + match + "' AND account_id = '" + accountID + "'";
                command = new MySqlCommand(query, connection);
                int result = command.ExecuteNonQuery();
                CloseConnection();
                return result;
            }
            else
            {
                CloseConnection();
                return 0;
            }

        }

        public int RemoveMatchesWithID(String matchID)
        {
            OpenIfNeeded();

            String query = "DELETE FROM matches WHERE match_id = '" + matchID + "'";
            command = new MySqlCommand(query, connection);
            int result = command.ExecuteNonQuery();
            CloseConnection();
            return result;
        }

        private int UpdateDevice(String key, int id)
        {
            OpenIfNeeded();

            String query = "UPDATE devices SET device_key='" + key + "' WHERE id='" + id + "'";

            command = new MySqlCommand(query, connection);
            int result = command.ExecuteNonQuery();
            CloseConnection();
            return result;

        }

        private Boolean AccountExist(String email)
        {
            OpenIfNeeded();

            command.CommandText = "SELECT * FROM account WHERE email = '" + email + "'";
            Reader = command.ExecuteReader();
            Boolean result = Reader.HasRows;
            CloseConnection();
            return result;
        }

        private Boolean DeviceExist(String key)
        {
            OpenIfNeeded();

            command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM devices WHERE device_key = '" + key + "'";
            Reader = command.ExecuteReader();
            Boolean result = Reader.HasRows;
            CloseConnection();
            return result;
        }

        public int GetAccountIDFromEmail(String email)
        {
            OpenIfNeeded();

            int id = -1;
            try { 
            command.CommandText = "SELECT * FROM account WHERE email = '" + email + "'";
            Reader = command.ExecuteReader();
                if (Reader.HasRows)
                {
                    Reader.Read();
                    id = Reader.GetInt32(0);
                }
                else
                {
                    id = -2;
                }
                
            }
            catch (Exception e) {
                log.Error(e.Message + "\n" + e.StackTrace);
                id = -3;
            }
            CloseConnection();
            return id;
        }

        /**
        * Get all users that want to get notified when a match finishes
        */
        public List<DBMatch> GetMatchesFromID(String matchID)
        {
            OpenIfNeeded();

            List<DBMatch> data = new List<DBMatch>();
            command.CommandText = "SELECT * FROM matches WHERE match_id = '" + matchID + "'";
            Reader = command.ExecuteReader();

            while (Reader.Read())
            {
                DBMatch d = new DBMatch();
                d.ID = Reader.GetInt32(0);
                d.MatchID = Reader.GetInt32(1);
                d.AccountID = Reader.GetInt32(2);
                data.Add(d);
            }
            CloseConnection();

            return data;
        }

        /**
        * Get account that wants notification about new matches
        *
        *
        */

        public List<DBAccount> GetAccountNotifyNew()
        {
            OpenIfNeeded();

            List<DBAccount> data = new List<DBAccount>();
            command.CommandText = "SELECT * FROM account WHERE new_match = '1'";
            Reader = command.ExecuteReader();

            while (Reader.Read())
            {
                DBAccount d = new DBAccount();
                d.ID = Reader.GetInt32(0);
                d.Email = Reader.GetString(1);
                d.WantNewMatchNotification = Reader.GetInt16(3);
                data.Add(d);
            }
            CloseConnection();

            return data;
        }

        public List<DBAccount> GetAccountFromID(int id)
        {
            OpenIfNeeded();

            List<DBAccount> data = new List<DBAccount>();
            command.CommandText = "SELECT * FROM account WHERE id = '" + id + "'";
            Reader = command.ExecuteReader();

            while (Reader.Read())
            {
                DBAccount d = new DBAccount();
                d.ID = Reader.GetInt32(0);
                d.Email = Reader.GetString(1);
                d.WantNewMatchNotification = Reader.GetInt16(3);
                data.Add(d);
            }
            CloseConnection();
            return data;
        }

        /**
        *Get device from registation key that device will provide upon registation to GCM.
        */
        public List<DBDevice> GetDeviceFromKey(String key) 
        {
            OpenIfNeeded();

            List<DBDevice> data = new List<DBDevice>();
            command.CommandText = "SELECT * FROM devices WHERE device_key = '" + key + "'";
            Reader = command.ExecuteReader();

            while (Reader.Read())
            {
                DBDevice d = new DBDevice();
                d.ID = Reader.GetInt32(0);
                d.Key = Reader.GetString(1);
                d.AccountID = Reader.GetInt32(2);
                data.Add(d);
            }
            CloseConnection();

            return data;
        }

        public bool isMatchWatched(String deviceKey, String matchID)
        {
            OpenIfNeeded();

            command.CommandText = "SELECT * FROM devices WHERE device_key = '" + deviceKey + "'";
            Reader = command.ExecuteReader();
            if (Reader.HasRows)
            {
                Reader.Read();
                int accoutID = Reader.GetInt32(2);
                Reader.Close();
                command = new MySqlCommand("SELECT * FROM matches WHERE match_id='" + matchID + "' AND account_id='" + accoutID + "'", connection);
                Reader = command.ExecuteReader();
                bool has = Reader.HasRows;
                CloseConnection();
                return has;
            } else
            {
                CloseConnection();
                return false;
            }
        }

        public List<DBDevice> GetDevicesFromAccountID(int id)
        {
            OpenIfNeeded();

            List<DBDevice> data = new List<DBDevice>();
            command.CommandText = "SELECT * FROM devices WHERE account_id = '" + id + "'";
            Reader = command.ExecuteReader();

            while (Reader.Read())
            {
                DBDevice d = new DBDevice();
                d.ID = Reader.GetInt32(0);
                d.Key = Reader.GetString(1);
                d.AccountID = Reader.GetInt32(2);
                data.Add(d);
            }
            CloseConnection();
            return data;
        }
 
    }
}
