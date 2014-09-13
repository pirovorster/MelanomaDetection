
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
	public sealed class FillNonMaskFilter : IImmutableImageToImageFilter,IMutableImageToImageFilter
	{
		private readonly IMask _mask;
		private readonly Color _unmaskedColor;

		/// <summary>
		/// Contructs an apply mask filter for a single mask
		/// </summary>
		/// <param name="mask">The mask to apply</param>
		/// <param name="unmaskedColor">The color to make the unmasked part of the section</param>
		public FillNonMaskFilter(IMask mask, Color unmaskedColor)
		{
			if (mask == null)
				throw new ArgumentNullException("mask");

			_mask = mask ;
			_unmaskedColor = unmaskedColor;
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
						if (!_mask.GetCell(x, y))
							fastBitmap.SetPixel(x, y, _unmaskedColor);
					}
				}
			}

			
		}
	}
}
