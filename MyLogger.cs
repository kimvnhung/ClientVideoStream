using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientVideoStream
{
    class MyLogger
    {
        private static MyLogger Instance;
        private const string AppName = "ClientVideoStream";
        public static string RootPath = Path.Combine(Path.GetTempPath(), AppName);
        private static readonly string path = RootPath + "\\"+AppName+"_logger.txt";
        private static readonly string lastLinkPath = RootPath + "\\last_link.txt";
        private StreamWriter sw;
        private StreamReader sr;
        private MyLogger()
        {
            try
            {
                //create root
                if (!Directory.Exists(RootPath))
                {
                    Console.WriteLine("Dont exist");
                    //delete cache
                    File.Delete(RootPath);
                    Directory.CreateDirectory(RootPath);
                }

                //log file
                if (!File.Exists(path))
                {
                    sw = File.CreateText(path);
                    sw.Close();
                }

                //process id file
                if (!File.Exists(lastLinkPath))
                {
                    sw = File.CreateText(lastLinkPath);
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static MyLogger GetLogger()
        {
            if (Instance == null)
            {
                Instance = new MyLogger();
            }
            return Instance;
        }

        public string GetLastLink()
        {
            try
            {
                sr = new StreamReader(lastLinkPath);
                string storage = sr.ReadToEnd().Replace("\n", string.Empty).Replace("\r", string.Empty);
                sr.Close();

                return storage;
            }
            catch (Exception e)
            {
                WriteLine(GetType().Name, e.Message);
            }
            return "";
        }

        

        public void SaveLastLink(string last)
        {
            try
            {
                Console.WriteLine(last);
                sw = new StreamWriter(lastLinkPath);
                sw.Write(last);
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(GetType().Name + "-" + e.Message);
            }
        }
        
        public void WriteLine(string _class, string content)
        {
            try
            {
                content = _class + " - " + content;
                Console.WriteLine(content);
                sr = new StreamReader(path);
                string storage = sr.ReadToEnd();
                sr.Close();
                sw = new StreamWriter(path);
                sw.Write(storage);
                sw.WriteLine(DateTime.Now.ToString() + ": " + content);
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(GetType().Name + "-" + e.Message);
            }
        }
    }
}
