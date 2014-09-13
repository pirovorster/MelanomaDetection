using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Collections.Generic;
using System.Linq;

namespace Research.Algorithms.ImageFilters
{
	/// <summary>
	/// Common operations for the <see cref="BitmapImage"/>.
	/// </summary>
	public static class BitmapOperations
	{

		/// <summary>
		/// Finds the color that occurs most in an image
		/// </summary>
		/// <param name="image">The image from which we want to determine the color</param>
		/// <returns>The <see cref="Color"/> that occurs most in an image</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="image"/> is <c>null</c>.</exception>
		public static Color FindMeanColor(this Bitmap image)
		{

			if (image == null)
				throw new ArgumentNullException("image");

			Dictionary<Color, int> colorCount = new Dictionary<Color, int>();

			for (int x = 0; x < image.Width; x++)
			{
				for (int y = 0; y < image.Height; y++)
				{
					Color pixel = image.GetPixel(x, y);

					if (!colorCount.ContainsKey(pixel))
						colorCount[pixel] = 0;

					colorCount[image.GetPixel(x, y)]++;
				}
			}

			return colorCount.OrderByDescending(o => o.Value).First().Key;
		}


		/// <summary>
		/// Creates a bitmap with a singular background color
		/// </summary>
		/// <param name="width">The image width</param>
		///  <param name="height">The image height</param>
		///  <param name="backgroundColor">The image background color</param>
		/// <returns>The new bitmap</returns>
		public static Bitmap CreateFilledBitmap(int width, int height, Color backgroundColor)
		{

			Bitmap blank = new Bitmap(width, height);
			using (Graphics graphics = Graphics.FromImage(blank))
			{
				Rectangle rectangle = new Rectangle(new System.Drawing.Point(0, 0), blank.Size);
				SolidBrush brush = new SolidBrush(backgroundColor);
				graphics.FillRectangle(brush, rectangle);

			}

			return blank;
			
		}
	}
}
