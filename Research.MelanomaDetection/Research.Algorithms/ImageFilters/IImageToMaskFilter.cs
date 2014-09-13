using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Algorithms.ImageFilters
{
	/// <summary>
	/// Contract for all filters that create a mask as output from an image
	/// </summary>
	public interface IImageToMaskFilter
	{
		/// <summary>
		/// Applies a filter on the input image to produce the  output mask.
		/// </summary>
		/// <param name="bitmap">Source bitmap image</param>
		/// <returns>The resulting mask after applying the filter</returns>
		IMask Apply(Bitmap bitmap);
	}
}
