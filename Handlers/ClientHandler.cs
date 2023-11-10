using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Google.Protobuf;
using System.Windows;

namespace ClientVideoStream.Handlers
{
    class ClientHandler : IOposition
    {
        public static string myIp = "192.168.0.101";
        public static int myPort = 2021;
        public ClientHandler(string server)
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

                        //convert to protobuf object
                        try
                        {
                            Reply reply = Reply.Parser.ParseFrom(data);
                            LastMessage = reply.ToString();
                            if (reply.Status == Reply.Types.Status.Fail)
                            {
                                if (reply.Description.Length > 0)
                                {
                                    Utils.showMessage(reply.Description, "Lỗi!");
                                }
                            }
                            switch (reply.Header)
                            {
                                case Reply.Types.Header.EstablishConnection:
                                    OnIsStatusChanged(ProgramStatus.CONNECTED);
                                    MessageBox.Show("Thiết lập kết nối thành công", "Thông báo!");
                                    break;
                                case Reply.Types.Header.StartStream:
                                    OnIsStatusChanged(ProgramStatus.STREAMING);
                                    MessageBox.Show("Đang bắt đầu Stream tại địa chỉ RTSP://" + IPAddress.ToString() + ":" + Port, "Thông báo!");
                                    break;
                                case Reply.Types.Header.StopStream:
                                    OnIsStatusChanged(ProgramStatus.CONNECTED);
                                    MessageBox.Show("Đã ngừng Stream", "Thông báo!");
                                    break;
                                case Reply.Types.Header.StartTracking:
                                    MessageBox.Show("Bắt đầu tracking", "Thông báo!");
                                    break;
                                case Reply.Types.Header.StopTracking:
                                    MessageBox.Show("Đã ngừng tracking", "Thông báo!");
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine(Utils.GetHeximal(data));
                            LastMessage = Encoding.ASCII.GetString(data);
                        }
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

        public void EstablishConnection()
        {
            if (IsActive)
            {
                Command cmd = new Command();
                cmd.Header = Command.Types.Header.EstablishConnection;
                Command.Types.Address addr = new Command.Types.Address();
                addr.Ip = myIp;
                addr.Port = myPort;
                cmd.Address = addr;
                Console.WriteLine(cmd.ToString());
                SendCmd(cmd);
            }
        }

        public void StartStream()
        {
            if (IsActive)
            {
                Command cmd = new Command();
                cmd.Header = Command.Types.Header.StartStream;
                //cmd.Path = @"\\DESKTOP-95PS3EQ\drap videos\test-video_1m12s.mp4";
                cmd.Path = @"D:\hungkv2\StreamVideo\drap videos\test-video_1m12s.mp4";
                SendCmd(cmd);
            }
        }

        public void StopStream()
        {
            if (IsActive)
            {
                Command cmd = new Command();
                cmd.Header = Command.Types.Header.StopStream;
                SendCmd(cmd);
            }
        }

        public void StartTracking()
        {
            if (IsActive)
            {
                Command cmd = new Command();
                cmd.Header = Command.Types.Header.StartTracking;
                SendCmd(cmd);
            }
        }

        public void StopTracking()
        {
            if (IsActive)
            {
                Command cmd = new Command();
                cmd.Header = Command.Types.Header.StopTracking;
                SendCmd(cmd);
            }
        }

        public void SendMessage(string msg)
        {
            if (IsActive)
            {
                try
                {
                    this.Socket.Send(Encoding.ASCII.GetBytes(msg));
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void SendCmd(Command cmd)
        {
            try
            {
                this.Socket.Send(cmd.ToByteArray());
                Console.WriteLine("Sending succes");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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
