using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ClientVideoStream.Handlers
{
    class RtspConsumer : IOposition
    {
        public static string myIp = "192.168.0.101";
        public static int myPort = 2021;
        public RtspConsumer(string server)
        {
            try
            {
                SourceUsername = "Client" + new Random().Next(1, 100).ToString();
                IPAddress = IPAddress.Parse(server);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                IPAddress = IPAddress.Parse("127.0.0.1");
            }
            Port = (ushort)myPort;
        }

        private bool isActive = false;
        private string _lastMessage = "";

        public Dispatcher Dispatcher { get; set; }

        public Socket Socket { get; set; }

        public bool IsActive
        {
            get
            {
                return this.isActive;
            }
            set
            {
                if (value != this.isActive)
                {
                    Console.WriteLine("IsActive changed");
                    this.isActive = value;
                    OnIsActiveChanged(value);
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        public IPAddress IPAddress { get; set; }
        public ushort Port { get; set; }
        public IPEndPoint IPEndPoint { get { return new IPEndPoint(this.IPAddress, this.Port); } }
        public int ClientIdCounter { get; set; }
        public string SourceUsername { get; set; }

        public string LastMessage
        {
            get => this._lastMessage;
            set
            {
                if (!value.Equals(this._lastMessage))
                {
                    this._lastMessage = value;
                    OnPropertyChanged(nameof(LastMessage));
                }
            }
        }

        public EventHandler<bool> IsActiveChanged { get; set; }
        public EventHandler<ProgramStatus> IsStatusChanged { get; set; }



        public void OnIsActiveChanged(bool newValue)
        {
            this.IsActiveChanged?.Invoke(this, newValue);
        }

        public void OnIsStatusChanged(ProgramStatus newStatus)
        {
            this.IsStatusChanged?.Invoke(this, newStatus);
        }

        public void StartConnection()
        {
            if (this.IsActive)
            {
                return;
            }

            try
            {
                this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.Socket.Connect(this.IPEndPoint);

                Task.Run(() => ReceiveMessages());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public void StopConnection()
        {
            if (!this.IsActive)
            {
                return;
            }

            if (this.Socket != null)
            {
                //this.thread.Abort(); MainThread = null;
                this.Socket.Shutdown(SocketShutdown.Both);
                this.Socket.Dispose();
                this.Socket = null;
            }
            OnIsStatusChanged(ProgramStatus.UNKNOWN);
            this.IsActive = false;
        }

        public async Task ReceiveMessages()
        {
            await Task.Delay(100);//wait for establish connection
            while (true)
            {
                byte[] inf = new byte[1024];

                try
                {
                    if (!IsSocketConnected(this.Socket))
                    {
                        this.StopConnection();
                        return;
                    }
                    IsActive = true;

                    int x = this.Socket.Receive(inf);

                    if (x > 0)
                    {
                        //to show msg
                        byte[] data = new byte[x];
                        for (int i = 0; i < x; i++)
                        {
                            data[i] = inf[i];
                        }
                        Console.WriteLine("OnReceive Data");
                        Console.WriteLine(Utils.GetHeximal(data));
                        //replication

                    }
                }
                catch (SocketException ex)
                {
                    this.StopConnection();

                    // Concurrently closing a listener that is accepting at the time causes exception 10004.

                    Console.WriteLine(ex.Message);
                    if (ex.ErrorCode != 10004)
                    {

                        Utils.showMessage(ex.Message, "Lỗi");
                    }

                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        

        

        

        

        

        public void SendMessage(string msg)
        {
            if (IsActive)
            {
                try
                {
                    this.Socket.Send(Encoding.ASCII.GetBytes(msg));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        
        public static bool IsSocketConnected(Socket socket)
        {
            if (!socket.Connected)
            {
                return false;
            }

            if (socket.Available == 0)
            {
                if (socket.Poll(1000, SelectMode.SelectRead))
                {
                    return false;
                }
            }
            return true;
        }

        #region Implementations
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
