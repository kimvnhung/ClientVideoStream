﻿using ClientVideoStream.Handlers;
using ClientVideoStream.Models;
using ClientVideoStream.Pages;
using ClientVideoStream.ViewModels.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Vlc.DotNet.Wpf;

namespace ClientVideoStream.ViewModels
{
    class MediaPlayerPageModel : ViewModelBase
    {
        #region Members
        private MainModel _model;
        private VlcControl _controller;

        #endregion
        #region Properties

        public MainModel MainModel
        {
            get => this._model;
        }

        public ICommand StopCommand
        {
            get => new RelayCommand(async para => await Stop());
        }

        public ICommand StartCommand
        {
            get => new RelayCommand(async para => await Start());
        }
        #endregion
        #region Contructor
        public MediaPlayerPageModel()
        {
            _model = MainModel.GetInstance();
        }

        #endregion
        #region Methods
        public void SetController(VlcControl controller)
        {
            this._controller = controller;

            this._controller.MouseDown += delegate (object sender, MouseButtonEventArgs arg)
            {
                OnMouseClick(arg.GetPosition(controller));
            };
        }

        private async Task Start()
        {
            if(_model.Status != ProgramStatus.STREAMING)
            {
                var currentAssembly = Assembly.GetEntryAssembly();
                var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
                // Default installation path of VideoLAN.LibVLC.Windows
                var libDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
                if(_controller.SourceProvider.MediaPlayer == null)
                {
                    _controller.SourceProvider.CreatePlayer(libDirectory);
                }
                var rs = await _model.StartStream();
                if (rs)
                {
                    Console.WriteLine("Starting streaming");
                    //to start open stream on Link
                    Uri uri = new Uri($"rtsp://{MainModel.Link}:2020/test");
                    Console.WriteLine(uri);
                    _controller.SourceProvider.MediaPlayer.Play(uri);
                }

            }

        }

        private void OnMouseClick(Point position)
        {
            Console.WriteLine(position);
        }
       

        private async Task Stop()
        {
            if(_controller != null && _model.Status == ProgramStatus.STREAMING)
            {
                var rs = await _model.StopStream();
                if (rs)
                {
                    
                    Console.WriteLine("Stopped");
                    Session.Navigator.Navigate(new MainPage());
                }
            }
        }
        #endregion
    }
}
