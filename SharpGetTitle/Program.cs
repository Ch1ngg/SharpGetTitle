using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using System.Linq;

namespace SharpGetTitle
{
    class Program
    {
        public static AutoResetEvent myEvent = new AutoResetEvent(false);
        public static Queue<String> IPQueue = new Queue<String>();
        public static List<String> listPorts = new List<string>();
        public static List<String> listIP = new List<string>();
        public static int count = 0;
        public static bool isshow = false;
        public static string filename = "result.txt";
        static void Main(string[] args)
        {
            string ip = "";
            string ports = "80-89,443,7001,7002,8000-9999";
            string threadsnum = "100";
            var arguments = CommandLineArgumentParser.Parse(args);
            //help
            if (arguments.Has("-h") || args.Length <= 0)
            {
                help();
                Environment.Exit(0);
            }

            //输出详细信息到Console
            if (args.Contains("-v"))
            {
                isshow = true;
            }

            //ip处理if
            if (arguments.Has("-i"))
            {
                ip = arguments.Get("-i").Next;
                if (ip.Contains("/"))
                {
                    listIP = ip_cidr.Get_ipAddr(ip);
                }
                else if (ip.Contains("-"))
                {
                    string[] p = ip.Split(new char[] { '-' });
                    listIP = Util.GetLiveIP(p[0], p[1]);
                }
                else
                {
                    listIP.Add(ip);
                }
            }
            else
            {
                Console.WriteLine("-i is null !");
                Environment.Exit(0);
            }

            //端口处理if
            if (arguments.Has("-p"))
            {
                ports = arguments.Get("-p").Next;
                listPorts = Util.parsePort(ports);
            }

            //输出文件处理if
            if (arguments.Has("-o"))
            {
                filename = arguments.Get("-o").Next;
            }

            //线程处理if
            if (arguments.Has("-t"))
            {
                threadsnum = arguments.Get("-t").Next;
            }


            //拼接 ip：port
            for (int i = 0; i < listIP.Count; i++)
            {
                for (int t = 0; t < listPorts.Count; t++)
                {
                    IPQueue.Enqueue(listIP[i] + ":" + listPorts[t]);
                }
            }
            //设置最高和最低线程，数值是 -t
            ThreadPool.SetMinThreads(Convert.ToInt32(threadsnum), Convert.ToInt32(threadsnum));
            ThreadPool.SetMaxThreads(Convert.ToInt32(threadsnum), Convert.ToInt32(threadsnum));
            count = IPQueue.Count;
            
            for (int i = 1; i <= count; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(GetAll), IPQueue.Dequeue());
            }
            Console.WriteLine("[*] {0} Start ！", DateTime.Now.ToString());
            while (true)
            {
                if (count == 0)//监听 count 是否为0，0为结束。
                {
                    myEvent.Set();
                    break;
                }
            }
            myEvent.WaitOne();
            Console.WriteLine("[*] {0} Done ！", DateTime.Now.ToString());
            //Console.ReadKey();

        }
        public static void GetAll(object obj)
        {
            string url = "";
            if (obj.ToString().Contains(":443"))
            {
                url = String.Format("https://{0}", obj.ToString().Replace(":443", ""));
            }
            else
            {
                url = String.Format("http://{0}", obj.ToString());
            }
            String regex = @"<title>.+</title>";
            GC.Collect();
            HttpWebRequest req = null;
            HttpWebResponse res = null;
            try
            {
                
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                ServicePointManager.DefaultConnectionLimit = 500;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
                req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
                req.Method = "GET";
                req.Timeout = 1000;
                req.Proxy = null;
                req.AllowWriteStreamBuffering = false;
                req.ServicePoint.UseNagleAlgorithm = false;
                res = (HttpWebResponse)req.GetResponse();
                Stream myResponseStream = res.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
                string retString = myStreamReader.ReadToEnd();
                String title = Regex.Match(retString, regex).ToString();
                title = Regex.Replace(title, @"<title>", "");
                title = Regex.Replace(title, @"</title>", "");
                if (isshow)
                {
                    Console.WriteLine("{0}    [{1}]    [{2}]    [{3}]", url, Convert.ToInt32(res.StatusCode), res.Server, title);
                }
                Util.writre(filename,String.Format("{0}    [{1}]    [{2}]    [{3}]", url, Convert.ToInt32(res.StatusCode), res.Server, title));
                count -= 1;//用来统计线程数线程是否执行
                res.Close();                
            }
            catch (WebException ex)
            {
                
                if (ex.Response == null)
                {
                    count -= 1;//用来统计线程数线程是否执行
                    return;
                }
                res = (HttpWebResponse) ex.Response;
                StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
                string retString = sr.ReadToEnd();
                String title = Regex.Match(retString, regex).ToString();
                title = Regex.Replace(title, @"<title>", "");
                title = Regex.Replace(title, @"</title>", "");
                if (isshow)
                {
                    Console.WriteLine("{0}    [{1}]    [{2}]    [{3}]", url, Convert.ToInt32(res.StatusCode), res.Server, title);
                }
                Util.writre(filename, String.Format("{0}    [{1}]    [{2}]    [{3}]", url, Convert.ToInt32(res.StatusCode), res.Server, title));
                count -= 1;//用来统计线程数线程是否执行
            }
        }
        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   // 总是接受  
            return true;
        }

        public static void help()
        {
            Console.WriteLine("SharpGetTitle V1.1");
            Console.WriteLine("-h help         Print help");
            Console.WriteLine("-i ip           Input IP. eg:127.0.0.1|127.0.0.1-127.0.0.22|127.0.0.1/24");
            Console.WriteLine("-p ports        Scanner ports. default:80-89,443,7001,7002,8000-9999");
            Console.WriteLine("-t thread       Threads. default:100");
            Console.WriteLine("-o outfile      Output result to file. default:result.txt");
            Console.WriteLine("-v infomations  Show Runlog");
            Console.WriteLine("");
            Console.WriteLine("Run in Cobaltstrike:");
            Console.WriteLine("execute-assembly SharpGetTitle \"-i 127.0.0.1 -p 80\"");
        }
    }
}
