using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SharpGetTitle
{
    class ip_cidr
    {
        public static List<string> Get_ipAddr(String iip)
        {
            string[] sip = iip.Split(new char[] { '/' });
            IPAddress ip = IPAddress.Parse(sip[0]);
            int bits = Convert.ToInt32(sip[1]);
            uint mask = ~(uint.MaxValue >> bits);
            // Convert the IP address to bytes.
            byte[] ipBytes = ip.GetAddressBytes();

            // BitConverter gives bytes in opposite order to GetAddressBytes().
            byte[] maskBytes = BitConverter.GetBytes(mask).Reverse().ToArray();

            byte[] startIPBytes = new byte[ipBytes.Length];
            byte[] endIPBytes = new byte[ipBytes.Length];

            // Calculate the bytes of the start and end IP addresses.
            for (int i = 0; i < ipBytes.Length; i++)
            {
                startIPBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
                endIPBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
            }

            // Convert the bytes to IP addresses.
            IPAddress startIP = new IPAddress(startIPBytes);
            IPAddress endIP = new IPAddress(endIPBytes);
            string startip = Convert.ToString(startIP);
            string lip = Convert.ToString(endIP);
            List<string> list = Get_CIDR(startip, lip);
            /*for (int i = 0; i < list.Count; i++)
            {
                Console.WriteLine("Ip: {0} ", list[i]);
            }*/
            return list;
        }
       
        public static List<string> Get_CIDR(string startIP, string lastIp)
        {
            string sip = Convert.ToString(startIP);
            string lip = Convert.ToString(lastIp);
            uint iStartip = ipTint(sip);
            uint iEndIp = ipTint(lip);
            //StringBuilder ip_result = new StringBuilder();
            List<string> ip_result = new List<string>();
            if (iEndIp >= iStartip)
            {
                for (uint ip = iStartip; ip <= iEndIp; ip++)
                {
                    ip_result.Add(intTip(ip));
                }

            }
            else
            {
                Console.WriteLine("error");
            }
            return ip_result;
        }
        public static uint ipTint(string ipStr)
        {
            string[] ip = ipStr.Split('.');
            uint ipcode = 0xFFFFFF00 | byte.Parse(ip[3]);
            ipcode = ipcode & 0xFFFF00FF | (uint.Parse(ip[2]) << 0x8);
            ipcode = ipcode & 0xFF00FFFF | (uint.Parse(ip[1]) << 0xF);
            ipcode = ipcode & 0x00FFFFFF | (uint.Parse(ip[0]) << 0x18);
            return ipcode;
        }
        public static string intTip(uint ipcode)
        {
            byte a = (byte)((ipcode & 0xFF000000) >> 0x18);
            byte b = (byte)((ipcode & 0x00FF0000) >> 0xF);
            byte c = (byte)((ipcode & 0x0000FF00) >> 0x8);
            byte d = (byte)(ipcode & 0x000000FF);
            string ipStr = string.Format("{0}.{1}.{2}.{3}", a, b, c, d);
            return ipStr;
        }
    }
}