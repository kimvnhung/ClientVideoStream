using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientVideoStream.Handlers
{
    public interface IOposition : INotifyPropertyChanged
    {
        Socket Socket { get; set; }
        bool IsActive { get; set; }
        IPAddress IPAddress { get; set; }
        ushort Port { get; }
        IPEndPoint IPEndPoint { get; }
        string SourceUsername { get; set; }

        string LastMessage { get; set; }

        EventHandler<bool> IsActiveChanged { get; set; }

        void StartConnection();
        void StopConnection();
        void OnIsActiveChanged(bool newValue);


    }
}
