using ClientVideoStream.Handlers;
using ClientVideoStream.Models;
using ClientVideoStream.Pages;
using ClientVideoStream.ViewModels.Command;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Vlc.DotNet.Core;
using Vlc.DotNet.Wpf;
using WebEye.Controls.Wpf.StreamPlayerControl;

namespace ClientVideoStream.ViewModels
{
    class MediaPlayerPageModel : ViewModelBase
    {
        #region Members
        private MainModel _model;
        private StreamPlayerControl _controller;

        private string _command_contente = "";
        private ClientHandler _rtsp_client;
        private RtspConsumer rtspConsumer;
        private BitmapImage _currentFrame;

        private Thread _consumer_thread;

        #endregion
        #region Properties

        public MainModel MainModel
        {
            get => this._model;
        }

        public BitmapImage CurrentFrame
        {
            get => this._currentFrame;
            set
            {
                this._currentFrame = value;
                OnPropertyChanged(nameof(CurrentFrame));
            }
        }

        public string CommandContent
        {
            get => this._command_contente;
            set
            {
                if (!value.Equals(this._command_contente))
                {
                    this._command_contente = value;
                    OnPropertyChanged(nameof(CommandContent));
                }
            }
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

        ~MediaPlayerPageModel()
        {
            this._consumer_thread = null;
        }

        #endregion
        #region Methods
        public void SetController(StreamPlayerControl controller)
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
                //_rtsp_client = new ClientHandler(_model.Link);
                //_rtsp_client.Port = 2020;
                //_rtsp_client.StartConnection();
                //var currentAssembly = Assembly.GetEntryAssembly();
                //var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
                //// Default installation path of VideoLAN.LibVLC.Windows
                //var libDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
                //if (_controller.SourceProvider.MediaPlayer == null)
                //{
                //    _controller.SourceProvider.CreatePlayer(libDirectory);
                //}
                //var rs = await _model.StartStream();
                //if (rs)
                //{
                //    Console.WriteLine("Starting streaming");
                //    //to start open stream on Link
                //    Uri uri = new Uri($"rtsp://{MainModel.Link}:2020/test");
                //    Console.WriteLine(uri);
                //    _controller.SourceProvider.MediaPlayer.Play(uri);


                //}

                this._consumer_thread = new Thread(new ThreadStart(delegate ()
                {
                    Console.WriteLine("Start Consumer");
                    while (true)
                    {
                        try
                        {
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                                Bitmap bmp = _controller.GetCurrentFrame();
                                
                                Console.WriteLine(bmp.Width);
                            }));
                            
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }));
                var rs = await _model.StartStream();

                if (rs)
                {
                    Console.WriteLine("Start Reading Stream");
                    //    //to start open stream on Link
                    Uri uri = new Uri($"rtsp://{MainModel.Link}:2020/test");
                    Console.WriteLine(uri);
                    _controller.StartPlay(uri);
                }
            }

        }

        private async Task ListenRTSP()
        {
             
            
        }

        public BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public void OnEnter()
        {
            string cmd = CommandContent;
            CommandContent = "";
            if(_rtsp_client != null)
            {
                _rtsp_client.SendMessage(cmd);
                Console.WriteLine("Sending... : " + cmd);
            }
            else
            {
                Console.WriteLine("RTSP Socket is not active!");
            }
        }

        private void OnMouseClick(System.Windows.Point position)
        {
            Console.WriteLine(position);
        }
       

        private async Task Stop()
        {
            if(_model.Status == ProgramStatus.STREAMING)
            {
                var rs = await _model.StopStream();
                if (rs)
                {
                    
                    Console.WriteLine("Stopped");
                    Session.Navigator.Navigate(new MainPage());
                }
            }
            this._consumer_thread = null;
        }
        #endregion
    }
}
