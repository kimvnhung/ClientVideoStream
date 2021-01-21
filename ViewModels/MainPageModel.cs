using ClientVideoStream.Models;
using ClientVideoStream.Pages;
using ClientVideoStream.ViewModels.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClientVideoStream.ViewModels
{
    class MainPageModel : ViewModelBase
    {
        #region Members
        private MainModel _model;
        #endregion
        #region Properties
        public MainModel MainModel
        {
            get => this._model;
        }

        public ICommand StartCommand
        {
            get => new RelayCommand(async para => await Start());
        }
        #endregion
        #region Contructor
        public MainPageModel()
        {
            _model = MainModel.GetInstance();
        }

        #endregion
        #region Methods
        private async Task Start()
        {
            Session.Navigator.Navigate(new MediaPlayerPage());
        }
        #endregion
    }
}
