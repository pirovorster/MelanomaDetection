using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Algorithms.ImageFilters
{
	/// <summary>
	/// Contract for all filters that gets applied to the input bitmap
	/// </summary>
	public interface IMutableImageToImageFilter
	{
		/// <summary>
		/// Applies a filter on the input image 
		/// </summary>
		/// <param name="bitmap">Source bitmap image</param>
		void ApplyOnSource(Bitmap bitmap);
	}
}
