
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Algorithms.ImageFilters
{
	/// <summary>
	/// A filter that can be used to resize a bitmap
	/// </summary>
	public sealed class ResizeFilter : IImmutableImageToImageFilter
	{
		private readonly int _newWidth;
		private readonly int _newHeight;
		private readonly bool _keepAspectRatio;

		/// <summary>
		/// Contructs a resizing filter
		/// </summary>
		/// <param name="newWidth">The desired new width</param>
		/// <param name="newHeight">The desired new height</param>
		/// <param name="keepAspectRatio">Flag indicating whether or not the resizing should maintain the aspect ratio</param>
		public ResizeFilter(int newWidth, int newHeight, bool keepAspectRatio)
		{
			_newWidth = newWidth;
			_newHeight = newHeight;
			_keepAspectRatio = keepAspectRatio;
		}

		/// <summary>
		/// Resize a image
		/// </summary>
		/// <param name="bitmap">The source bitmap</param>
		/// <returns>The new resized bitmap</returns>
		public Bitmap Apply(Bitmap bitmap)
		{

			if (bitmap == null)
				throw new ArgumentNullException("bitmap");

			int newRatioAdjustedWidth = _newWidth;
			int newRatioAdjustedHeight = _newHeight;

			if (_keepAspectRatio)
			{
				var ratioX = (double)_newWidth / bitmap.Width;
				var ratioY = (double)_newHeight / bitmap.Height;
				var ratio = Math.Min(ratioX, ratioY);

				newRatioAdjustedWidth = (int)(bitmap.Width * ratio);
				newRatioAdjustedHeight = (int)(bitmap.Height * ratio);
			}

			Bitmap newImage = new Bitmap(newRatioAdjustedWidth, newRatioAdjustedHeight);
			using (Graphics gr = Graphics.FromImage(newImage))
			{
				gr.SmoothingMode = SmoothingMode.HighQuality;
				gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
				gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
				gr.DrawImage(bitmap, new Rectangle(0, 0, newRatioAdjustedWidth, newRatioAdjustedHeight));
			}

			return newImage;
		}

	}
}
