
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
	/// A filter that can be used to apply a mask on a bitmap
	/// </summary>
	public sealed class OverlayMaskFilter : IImmutableImageToImageFilter,IMutableImageToImageFilter
	{
		private readonly IMask _mask;
		private readonly Color _blendColor;

		/// <summary>
		/// Contructs an apply mask filter for a single mask
		/// </summary>
		/// <param name="mask">The mask to apply</param>
		/// <param name="blendColor">The color to make the unmasked part of the section</param>
		public OverlayMaskFilter(IMask mask, Color blendColor)
		{
			if (mask == null)
				throw new ArgumentNullException("mask");

			_mask = mask ;
			_blendColor = blendColor;
		}

		/// <summary>
		/// Applies a mask on an image
		/// </summary>
		/// <param name="bitmap">The source bitmap</param>
		/// <returns>The new masked bitmap</returns>
		public Bitmap Apply(Bitmap bitmap)
		{

			if (bitmap == null)
				throw new ArgumentNullException("bitmap");

			Bitmap newBitmap = new Bitmap(bitmap);
			ApplyOnSource(newBitmap);

			return newBitmap;
			
		}

		/// <summary>
		/// Applies a mask on an image
		/// </summary>
		/// <param name="bitmap">The source bitmap</param>
		public void ApplyOnSource(Bitmap bitmap)
		{

			if (bitmap == null)
				throw new ArgumentNullException("bitmap");

			using (FastBitmap fastBitmap = new FastBitmap(bitmap))
			{
				int width = bitmap.Width;
				int height = bitmap.Height;

				for (int x = 0; x < width; x++)
				{
					for (int y = 0; y < height; y++)
					{
						if (_mask.GetCell(x, y))
							fastBitmap.SetPixel(x, y, Blend(_blendColor,fastBitmap.GetPixel(x,y),0.2));
					}
				}
			}

			
		}


		private static Color Blend( Color color, Color backColor, double amount)
		{
			byte r = (byte)((color.R * amount) + backColor.R * (1 - amount));
			byte g = (byte)((color.G * amount) + backColor.G * (1 - amount));
			byte b = (byte)((color.B * amount) + backColor.B * (1 - amount));
			return Color.FromArgb(r, g, b);
		}
	}
}
