using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ClientVideoStream.DataTypes
{
    class FrameTrackingItem
    {
        #region Members
        private string _position;
        private string _codec_infor;
        private BitmapImage _image; 
        #endregion
        #region Contructors
        public FrameTrackingItem(BitmapImage image,string position,string codecInfor)
        {
            Image = image;
            Position = position;
            CodecInfor = codecInfor;
        }
        #endregion
        #region Properties
        public string Position
        {
            get => this._position;
            set
            {
                if (!value.Equals(this._position))
                {
                    this._position = value;
                }
            }
        }

        public string CodecInfor
        {
            get => this._codec_infor;
            set
            {
                if (!value.Equals(this._codec_infor))
                {
                    this._codec_infor = value;
                }
            }
        }

        public BitmapImage Image
        {
            get => this._image;
            set
            {
                if (!value.Equals(this._image))
                {
                    this._image = value;
                }
            }
        }
        #endregion
        #region Methods
        #endregion
    }
}
