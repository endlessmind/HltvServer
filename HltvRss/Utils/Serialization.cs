using HltvRss.Classes;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace HltvRss.Utils
{
    class Serialization
    {

        public static void SaveConfig(ConfigCollection cfg)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("config.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, cfg);
            stream.Close();
        }

        public static ConfigCollection LoadConfig()
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream("config.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
                ConfigCollection obj = (ConfigCollection)formatter.Deserialize(stream);
                stream.Close();
                if (obj == null)
                {
                    obj = new ConfigCollection();
                }
                return obj;
            }
            catch (Exception e) { return new ConfigCollection(); }
           
        }
 

    }
}
