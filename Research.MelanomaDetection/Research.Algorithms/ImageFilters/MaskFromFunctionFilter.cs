
using MathNet.Numerics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Algorithms.ImageFilters
{
	/// <summary>
	/// A filter that can be used to create a mask based on a function that is applied on an image
	/// </summary>
	public sealed class MaskFromFunctionFilter : IImageToMaskFilter
	{
		private readonly Func<Color, bool> _maskingFunction;

		/// <summary>
		/// Contructs a MaskFromFunctionFilter
		/// </summary>
		/// <param name="maskingFunction">The function to apply to get the mask from the image</param>		
		public MaskFromFunctionFilter(Func<Color, bool> maskingFunction)
		{
			if (maskingFunction == null)
				throw new ArgumentNullException("maskingFunction");
			

			_maskingFunction = maskingFunction;
		}

		/// <summary>
		/// Creates a mask from the image based on a function
		/// </summary>
		/// <param name="bitmap">The source bitmap</param>
		/// <returns>The new mask of the function</returns>
		public IMask Apply(Bitmap bitmap)
		{
			if (bitmap == null)
				throw new ArgumentNullException("bitmap");
		

			BitArray maskData = new BitArray(bitmap.Width * bitmap.Height);

			for (int x = 0; x < bitmap.Width; x++)
			{
				for (int y = 0; y < bitmap.Height; y++)
				{
					int baseIndex = y * bitmap.Width;
					int index = baseIndex + x;
					Color currentColor = bitmap.GetPixel(x, y);

					if (_maskingFunction(currentColor))
						maskData[index] = true;
					else
						maskData[index] = false;
				}
			}

			return new Mask(maskData, bitmap.Width, bitmap.Height);
		}

		

	}
}
