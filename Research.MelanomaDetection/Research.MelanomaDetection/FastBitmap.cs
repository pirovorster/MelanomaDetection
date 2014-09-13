
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.MelanomaDetection
{
	unsafe internal sealed class FastBitmap : IDisposable
	{
		private struct PixelData
		{
			public byte Blue;
			public byte Green;
			public byte Red;
		}

		private readonly Bitmap _bitmap;
		private BitmapData _bitmapData;
		private readonly int _pixelSize;
		private byte* _current;
		private readonly int _width;

		internal FastBitmap(Bitmap bitmap)
		{
			
			_bitmap = bitmap;
			_pixelSize = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
			_bitmapData = _bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
			_width = _bitmapData.Stride;
			_current = (byte*)(void*)_bitmapData.Scan0;
		}

		internal Color GetPixel(int x, int y)
		{
			PixelData* pixelData = (PixelData*)(_current + y * _width + x * _pixelSize);
			return Color.FromArgb(pixelData->Red, pixelData->Green, pixelData->Blue);
		}

		internal void SetPixel(int x, int y, Color color)
		{
			PixelData* data = (PixelData*)(_current + y * _width + x * _pixelSize);
			data->Red = color.R;
			data->Green = color.G;
			data->Blue = color.B;

		}

		public void Dispose()
		{
			_bitmap.UnlockBits(_bitmapData);
			_bitmapData = null;
		}


	}
}
