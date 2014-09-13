using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Research.MaskSelection
{
	public sealed class MaskLookupItem : ViewModel
	{
		private  BitmapImage _image;
		private  string _filePath;

		internal MaskLookupItem(BitmapImage image, string filePath)
		{
			Image = image;
			FilePath = filePath;
		}

		public BitmapImage Image { 
			get 
			{ 
				return _image; 
			}
			set
			{
				_image = value;
				this.OnPropertyChanged("Image");
			}
		}
		public string FilePath 
		{ 
			get 
			{ 
				return _filePath; 
			}
			set
			{
				_filePath = value;
				this.OnPropertyChanged("FilePath");
			}
		}
	}
}
