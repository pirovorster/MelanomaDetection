using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Algorithms.ImageFilters
{
	/// <summary>
	/// Contract for all filters that create a new bitmap as output
	/// </summary>
	public interface IImmutableImageToImageFilter
	{
		/// <summary>
		/// Applies a filter on the input image to produce the new output image.
		/// </summary>
		/// <param name="bitmap">Source bitmap image</param>
		/// <returns>The resulting bitmap after applying the filter</returns>
		Bitmap Apply(Bitmap bitmap);
	}
}
