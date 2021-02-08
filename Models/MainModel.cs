
using ClientVideoStream.Handlers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vlc.DotNet.Core;

namespace ClientVideoStream.Models
{
    class MainModel : INotifyPropertyChanged
    {
        #region Memebers
        private bool _isPlaying = false;
        private bool _isConnected = false;

        private string _link = "";

        private ProgramStatus status = ProgramStatus.UNKNOWN;

        private MyLogger logger;
        private VlcMediaPlayer _mediaPlayer;
        private ClientHandler clientHandler; 

        #endregion
        #region Properties

        public ProgramStatus Status
        {
            get => this.status;
            set
            {
                if(value != this.status)
                {
                    this.status = value;
                    OnPropertyChanged(nameof(Status));
                    if (value == ProgramStatus.UNKNOWN)
                    {
                        IsConnected = false;
                    }
                    else
                    {
                        IsConnected = true;
                    }
                }
            }
        }

        public bool IsConnected
        {
            get => this._isConnected;
            set
            {
                if( value != this._isConnected)
                {
                    this._isConnected = value;
                    OnPropertyChanged(nameof(IsConnected));
                }
            }
        }

        public ClientHandler Client
        {
            get => this.clientHandler;
            set
            {
                if (!value.Equals(this.clientHandler))
                {
                    this.clientHandler = value;
                    OnPropertyChanged(nameof(Client));
                    Client.IsStatusChanged = OnStatusChanged;
                }
            }
        }

        public bool IsPlaying
        {
            get => this._isPlaying;
            set
            {
                if (value != _isPlaying)
                {
                    this._isPlaying = value;
                    OnPropertyChanged(nameof(IsPlaying));
                }
            }
        }

        public string Link
        {
            get => this._link;
            set
            {
                if (!value.Equals(this._link))
                {
                    this._link = value;
                    OnPropertyChanged(nameof(Link));
                }
            }
        }

        #endregion
        #region Singleton
        private static MainModel Instance;
        
        private MainModel()
        {
            logger = MyLogger.GetLogger();
            _link = logger.GetLastLink();
        }

        ~MainModel()
        {
            logger.SaveLastLink(Link);
        }

        public static MainModel GetInstance()
        {
            if(Instance == null)
            {
                Instance = new MainModel();
            }

            return Instance;
        }

        #endregion
        #region Methods
        public async Task<bool> EstablisConnection()
        {
            Client.StartConnection();
            int count = 5;
            while (count-- > 0)
            {
                Client.EstablishConnection();
                await Task.Delay(100);
                if (IsConnected)
                {
                    break;
                }
                if(count == 1)
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> StartStream()
        {
            if (IsConnected && Client.IsActive)
            {
                Client.StartStream();
                int count = 0;
                while(count++ < 1000)
                {
                    if (Status == ProgramStatus.STREAMING)
                    {
                        return true;
                    }
                    await Task.Delay(100);
                }
            }
            return false;
        }

        public async Task<bool> StopStream()
        {
            if(IsConnected && Client.IsActive)
            {
                Client.StopStream();
                int count = 0;
                while(count++ < 1000)
                {
                    if(Status == ProgramStatus.CONNECTED)
                    {
                        return true;
                    }
                    await Task.Delay(100);
                }

            }
            return false;
        }
        #endregion

        #region Listeners
        public void OnStatusChanged(object sender, ProgramStatus newValue)
        {
            Console.WriteLine("IsConnectionChanged :" + newValue.ToString());
            Status = newValue;
        }
        #endregion

        #region Implementations
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}
