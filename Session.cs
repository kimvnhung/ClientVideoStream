using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace ClientVideoStream
{
    class Session
    {
        internal static NavigationService Navigator { get; set; }
        internal static Window CurrentWindow { get; set; }
    }
}
