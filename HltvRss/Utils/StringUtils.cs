using HltvRss.Classes;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace HltvRss.Utils
{
    class StringUtils
    {

        public static String getCountryCodeFromFlagUrl(String url)
        {
            String country = url.Substring(url.LastIndexOf("/"));
            try
            {
                country = country.Substring(1, country.Length - 5).ToUpper();
            } catch (Exception e) { country = "WORLD"; }
            return country;
        }

        public static String getURLFromCSS(String data)
        {
            int len = data.Length - 5;
            return data.Substring(4, len);
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static String FormateFileSize(long i)
        {
            try
            {
                string sign = (i < 0 ? "-" : "");
                double readable = (i < 0 ? -i : i);
                string suffix;
                if (i >= 0x1000000000000000) // Exabyte
                {
                    suffix = "EB";
                    readable = (double)(i >> 50);
                }
                else if (i >= 0x4000000000000) // Petabyte
                {
                    suffix = "PB";
                    readable = (double)(i >> 40);
                }
                else if (i >= 0x10000000000) // Terabyte
                {
                    suffix = "TB";
                    readable = (double)(i >> 30);
                }
                else if (i >= 0x40000000) // Gigabyte
                {
                    suffix = "GB";
                    readable = (double)(i >> 20);
                }
                else if (i >= 0x100000) // Megabyte
                {
                    suffix = "MB";
                    readable = (double)(i >> 10);
                }
                else if (i >= 0x400) // Kilobyte
                {
                    suffix = "KB";
                    readable = (double)i;
                }
                else
                {
                    return i.ToString(sign + "0 B"); // Byte
                }
                readable = readable / 1024;

                return sign + readable.ToString("0.### ") + suffix;

            } catch (Exception e)
            {
                return i + "B";
            }
        }

    }
}
