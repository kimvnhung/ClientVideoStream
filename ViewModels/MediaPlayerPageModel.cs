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
using ClientVideoStream.DataTypes;

namespace ClientVideoStream.ViewModels
{
    class MediaPlayerPageModel : ViewModelBase
    {
        #region Members
        private MainModel _model;
        private VlcControl _controller;

        private ClientHandler _rtsp_client;
        private BitmapImage _currentFrame;
        private FrameTrackingItem[] _itemsSource;

        private static int VideoWidth = 1072;
        private static int VideoHeight = 1906;

        #endregion
        #region Properties

        public MainModel MainModel
        {
            get => this._model;
        }

        public FrameTrackingItem[] ItemsSource
        {
            get => this._itemsSource;
            set
            {
                if (!value.Equals(this._itemsSource))
                {
                    this._itemsSource = value;
                    OnPropertyChanged(nameof(ItemsSource));
                }
            }
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
                    _controller.SourceProvider.MediaPlayer.SetVideoCallbacks(LockVideo, UnlockVideo, DisplayVideo, IntPtr.Zero);
                    _controller.SourceProvider.MediaPlayer.Playing += delegate (object sender, VlcMediaPlayerPlayingEventArgs arg)
                    {
                        if(_controller.SourceProvider.MediaPlayer.Time <= 100)
                        {
                            foreach (MediaTrack track in _controller.SourceProvider.MediaPlayer.GetMedia().Tracks)
                            {
                                Console.WriteLine(FourCCConverter.FromFourCC(track.CodecFourcc));
                            }
                           // VideoWidth = (_controller.SourceProvider.VideoSource as BitmapSource).PixelWidth;
                            //VideoHeight = (_controller.SourceProvider.VideoSource as BitmapSource).PixelHeight;
                        }
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
        /// Called by libvlc when it wants to acquire a buffer where to write
        /// </summary>
        /// <param name="userdata">The pointer to the buffer (the out parameter of the <see cref="VideoFormat"/> callback)</param>
        /// <param name="planes">The pointer to the planes array. Since only one plane has been allocated, the array has only one value to be allocated.</param>
        /// <returns>The pointer that is passed to the other callbacks as a picture identifier, this is not used</returns>
        private IntPtr LockVideo(IntPtr userdata, IntPtr planes)
        {
            byte[] destination = new byte[10240];
            
            //Console.WriteLine(Utils.GetHeximal(destination));
            Marshal.WriteIntPtr(planes, userdata);
            //var format = _controller.SourceProvider.IsAlphaChannelEnabled ? PixelFormats.Bgra32 : PixelFormats.Bgr32;
            //long totalLength = VideoWidth * VideoHeight * format.BitsPerPixel / 8;//length in bytes
            //Console.WriteLine("TotalLength : " + totalLength);
            //byte[] data = new byte[totalLength];
            //Marshal.Copy(planes, destination, 0, destination.Length);
            //Bitmap bmp = GetDataPicture(VideoWidth, VideoHeight, destination, _controller.SourceProvider.IsAlphaChannelEnabled);
            //Console.WriteLine("bmp : " + bmp.Size);
            //Application.Current.Dispatcher.Invoke(() =>
            //{
            //    if (ItemsSource == null)
            //    {
            //        ItemsSource = new FrameTrackingItem[1];
            //        ItemsSource[0] = new FrameTrackingItem(BitmapToImageSource(bmp), "demo position", "demo codec");
            //    }
            //});
            Marshal.Copy(planes, destination, 0, destination.Length);
            Console.WriteLine(Utils.GetHeximal(destination));
            
            return userdata;
        }

        private void UnlockVideo(IntPtr userData,IntPtr picture,IntPtr[] planes)
        {
            //ImageSource imageSource = _controller.SourceProvider.VideoSource;
            //int width = (imageSource as BitmapSource).PixelWidth;
            //int height = (imageSource as BitmapSource).PixelHeight;
            
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

        public Bitmap GetDataPicture(int w, int h, byte[] data,bool isArgb)
        {
            Bitmap pic = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int arrayIndex = y * w + x;
                    System.Drawing.Color c = System.Drawing.Color.FromArgb(
                       data[arrayIndex],
                       data[arrayIndex + 1],
                       data[arrayIndex + 2],
                       data[arrayIndex + 3]
                    );
                    pic.SetPixel(x, y, c);
                }
            }

            return pic;
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
        }
        #endregion
    }
}
