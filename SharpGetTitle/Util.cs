using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SharpGetTitle
{
    class Util
    {
        public static List<string> GetLiveIP(string startIP, string endIP)
        {
            List<string> result = new List<string>();
            var ipArray = startIP.Split('.');
            int A1 = int.Parse(ipArray[0]);
            int A2 = int.Parse(ipArray[1]);
            int A3 = int.Parse(ipArray[2]);
            int A4 = int.Parse(ipArray[3]);

            int i = 0;
            while (i == 0)
            {

                string item = string.Empty;
                if (A4 != 255)
                {
                    if (A4 == 0)
                    {
                        A4++;
                        continue;
                    }
                    item = A1 + "." + A2 + "." + A3 + "." + A4;
                    A4++;
                    result.Add(item);
                }

                if (A4 == 255)
                {
                    A3++;
                    A4 = 0;
                }
                else if (A3 == 255)
                {
                    A2++;
                    A3 = 0;
                }
                else if (A2 == 255)
                {
                    A1++;
                }

                if (item == endIP)
                {
                    i = 1;
                }
            }

            return result;
        }

        public static List<String> parsePort(String ports)
        {

            List<String> allports = new List<string>();
            if (!ports.Contains(",") && !ports.Contains("-"))
            {
                allports.Add(ports);
                return allports;
            }
            else if(!ports.Contains(",") && ports.Contains("-"))
            {
                string[] p = ports.Split(new char[] { '-' });
                for (int i = Convert.ToInt32(p[0]); i <= Convert.ToInt32(p[1]); i++)
                {
                    allports.Add(Convert.ToString(i));
                }
                return allports;
            }
            else
            {
                string[] port = ports.Split(new char[] { ',' });
                foreach (var item in port)
                {
                    if (item.Contains("-"))
                    {
                        string[] p = item.Split(new char[] { '-' });
                        for (int i = Convert.ToInt32(p[0]); i <= Convert.ToInt32(p[1]); i++)
                        {
                            allports.Add(Convert.ToString(i));
                        }
                        continue;
                    }
                    allports.Add(Convert.ToString(item));
                }
            }
            return allports;
        }
        public static bool regexAll(string string_0)
        {
            Regex regex = new Regex("^(\\d{1,2}|1\\d\\d|2[0-4]\\d|25[0-5])\\.(\\d{1,2}|1\\d\\d|2[0-4]\\d|25[0-5])\\.(\\d{1,2}|1\\d\\d|2[0-4]\\d|25[0-5])\\.(\\d{1,2}|1\\d\\d|2[0-4]\\d|25[0-5])$");
            return regex.IsMatch(string_0);
        }
        public static bool regexAll_1(string string_0)
        {
            Regex regex = new Regex("^(\\d{1,2}|1\\d\\d|2[0-4]\\d|25[0-5])\\.(\\d{1,2}|1\\d\\d|2[0-4]\\d|25[0-5])\\.(\\d{1,2}|1\\d\\d|2[0-4]\\d|25[0-5])$");
            return regex.IsMatch(string_0);
        }

        public static void writre(string filename,string res)
        {
            StreamWriter sw = null;
            if (!File.Exists(filename))
            {
                FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
                sw = new StreamWriter(fs);
                sw.WriteLine(res);
                //记得要关闭！不然里面没有字！
                sw.Close();
                fs.Close();
            }
            else
            {
                sw = File.AppendText(filename);
                sw.WriteLine(res);
                sw.Close();
                //MessageBox.Show("已经有log文件了!");
            }
        }
    }
}
