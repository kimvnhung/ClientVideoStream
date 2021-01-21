
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
        private MyLogger logger;
        private string _link = "";
        private bool _isPlaying = false;
        private VlcMediaPlayer _mediaPlayer;
        #endregion
        #region Properties

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
