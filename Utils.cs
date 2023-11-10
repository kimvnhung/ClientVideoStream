using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ClientVideoStream
{
    class Utils
    {
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public static string GetHeximal(byte[] rs)
        {
            StringBuilder result = new StringBuilder(rs.Length * 2);
            for (int i = 0; i < rs.Length; i++)
            {
                result.Append("{");
                result.AppendFormat("{0:x2}", rs[i]);
                result.Append("}");
            }

            return result.ToString();
        }

        public static void showMessage(string msg, string title)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                MessageBox.Show(Application.Current.MainWindow, msg, title);
            }));
        }
    }
}
