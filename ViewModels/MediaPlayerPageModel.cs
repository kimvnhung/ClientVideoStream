using ClientVideoStream.Handlers;
using ClientVideoStream.Models;
using ClientVideoStream.Pages;
using ClientVideoStream.ViewModels.Command;

using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Vlc.DotNet.Core.Interops;
using Vlc.DotNet.Core.Interops.Signatures;
using Vlc.DotNet.Wpf;

using Vlc.DotNet.Core;
using System.ComponentModel;
using System.Windows.Threading;

namespace ClientVideoStream.ViewModels
{
    class MediaPlayerPageModel : ViewModelBase
    {
        #region Members
        private MainModel _model;
        private VlcControl _controller;

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
                //_rtsp_client = new ClientHandler(_model.Link);
                //_rtsp_client.Port = 2020;
                //_rtsp_client.StartConnection();
                var currentAssembly = Assembly.GetEntryAssembly();
                var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
                // Default installation path of VideoLAN.LibVLC.Windows
                var libDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
                if (_controller.SourceProvider.MediaPlayer == null)
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
                    //_controller.SourceProvider.MediaPlayer.SetVideoFormatCallbacks(VideoFormat, null);
                    _controller.SourceProvider.MediaPlayer.SetVideoCallbacks(LockVideo, null, DisplayVideo, IntPtr.Zero);
                    _controller.SourceProvider.MediaPlayer.Playing += delegate (object sender, VlcMediaPlayerPlayingEventArgs arg)
                    {
                        
                    };
                    
                }
            }

        }

        /// <summary>
        /// Aligns dimension to the next multiple of mod
        /// </summary>
        /// <param name="dimension">The dimension to be aligned</param>
        /// <param name="mod">The modulus</param>
        /// <returns>The aligned dimension</returns>
        private uint GetAlignedDimension(uint dimension, uint mod)
        {
            var modResult = dimension % mod;
            if (modResult == 0)
            {
                return dimension;
            }

            return dimension + mod - (dimension % mod);
        }

        /// <summary>
        /// Called by vlc when the video format is needed. This method allocats the picture buffers for vlc and tells it to set the chroma to RV32
        /// </summary>
        /// <param name="userdata">The user data that will be given to the <see cref="LockVideo"/> callback. It contains the pointer to the buffer</param>
        /// <param name="chroma">The chroma</param>
        /// <param name="width">The visible width</param>
        /// <param name="height">The visible height</param>
        /// <param name="pitches">The buffer width</param>
        /// <param name="lines">The buffer height</param>
        /// <returns>The number of buffers allocated</returns>
        private uint VideoFormat(out IntPtr userdata, IntPtr chroma, ref uint width, ref uint height, ref uint pitches, ref uint lines)
        {
            var pixelFormat = this._controller.IsAlphaChannelEnabled ? PixelFormats.Bgra32 : PixelFormats.Bgr32;
            FourCCConverter.ToFourCC("RV32", chroma);

            //Correct video width and height according to TrackInfo
            var md = this._controller.SourceProvider.MediaPlayer.GetMedia();
            foreach (MediaTrack track in md.Tracks)
            {
                if (track.Type == MediaTrackTypes.Video)
                {
                    var trackInfo = (VideoTrack)track.TrackInfo;
                    if (trackInfo.Width > 0 && trackInfo.Height > 0)
                    {
                        width = trackInfo.Width;
                        height = trackInfo.Height;
                        if (trackInfo.SarDen != 0)
                        {
                            width = width * trackInfo.SarNum / trackInfo.SarDen;
                        }
                    }

                    break;
                }
            }

            pitches = GetAlignedDimension((uint)(width * pixelFormat.BitsPerPixel) / 8, 32);
            lines = GetAlignedDimension(height, 32);

            var size = pitches * lines;

            IntPtr memoryMappedFile = Win32Interop.CreateFileMapping(new IntPtr(-1), IntPtr.Zero,
                Win32Interop.PageAccess.ReadWrite, 0, (int)size, null);

            IntPtr memoryMappedView = Win32Interop.MapViewOfFile(memoryMappedFile, Win32Interop.FileMapAccess.AllAccess, 0, 0, size);
            var viewHandle = memoryMappedView;

            userdata = viewHandle;
            Console.WriteLine("Usersize: " + size);
            return 1;
        }


        /// <summary>
        /// Called by Vlc when it requires a cleanup
        /// </summary>
        /// <param name="userdata">The parameter is not used</param>
        private void CleanupVideo(ref IntPtr userdata)
        {
            // This callback may be called by Dispose in the Dispatcher thread, in which case it deadlocks if we call RemoveVideo again in the same thread.
            //if (!this._controller.SourceProvider.MediaPlayer)
            //{
            //    this.dispatcher.Invoke((Action)this.RemoveVideo);
            //}
        }

        

        /// <summary>
        /// Called by libvlc when it wants to acquire a buffer where to write
        /// </summary>
        /// <param name="userdata">The pointer to the buffer (the out parameter of the <see cref="VideoFormat"/> callback)</param>
        /// <param name="planes">The pointer to the planes array. Since only one plane has been allocated, the array has only one value to be allocated.</param>
        /// <returns>The pointer that is passed to the other callbacks as a picture identifier, this is not used</returns>
        private IntPtr LockVideo(IntPtr userdata, IntPtr planes)
        {
            byte[] destination = new byte[1024];
            Marshal.Copy(userdata, destination, 0, destination.Length);
            //Console.WriteLine(Utils.GetHeximal(destination));
            Marshal.WriteIntPtr(planes, userdata);
            //Console.WriteLine("onLockVideo");
            
            return userdata;
        }

        /// <summary>
        /// Called by libvlc when the picture has to be displayed.
        /// </summary>
        /// <param name="userdata">The pointer to the buffer (the out parameter of the <see cref="VideoFormat"/> callback)</param>
        /// <param name="picture">The pointer returned by the <see cref="LockVideo"/> callback. This is not used.</param>
        private void DisplayVideo(IntPtr userdata, IntPtr picture)
        {
            // Invalidates the bitmap
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                (this._controller.SourceProvider.VideoSource as InteropBitmap)?.Invalidate();
            }));
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
