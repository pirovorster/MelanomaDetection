using AForge.Imaging.Filters;
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
	/// A filter that can be used to create a grayscale of a bitmap
	/// </summary>
	public sealed class AverageGrayscaleFilter : IImmutableImageToImageFilter
	{
		
		/// <summary>
		/// Contructs an average grayscale filter
		/// </summary>
		public AverageGrayscaleFilter()
		{
			
		}

		/// <summary>
		/// Applies an average grayscale filter to a bitmap
		/// </summary>
		/// <param name="bitmap">The source bitmap</param>
		/// <returns>The new masked bitmap</returns>
		public Bitmap Apply(Bitmap bitmap)
		{
			if (bitmap == null)
				throw new ArgumentNullException("bitmap");
		

			const double oneThird = 0.33333333;
			Grayscale averageGrayscaleFilter = new Grayscale(oneThird, oneThird, oneThird);

			Bitmap newImage = averageGrayscaleFilter.Apply(bitmap);

			return newImage;
			
		}
	}
}
