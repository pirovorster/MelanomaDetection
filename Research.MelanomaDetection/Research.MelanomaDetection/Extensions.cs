using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.MelanomaDetection
{
	public static class Extensions
	{

		public static Bitmap ThresholdSegmentation(this Bitmap bitmap, int threshold)
		{
			const double oneThird = 0.33333333;
			Grayscale grayFilter = new Grayscale(oneThird, oneThird, oneThird);

			Bitmap newBitmap = (Bitmap)bitmap.Clone();

			ContrastStretch contrastStretch = new ContrastStretch();


			GaussianBlur gaussianBlurFilter = new GaussianBlur();
			bitmap.Save(@"C:\temp1.bmp");
					

			using (Bitmap smoothedImage = gaussianBlurFilter.Apply(bitmap))
			{
				smoothedImage.Save(@"C:\temp2.bmp");
				using (Bitmap grayImage = grayFilter.Apply(smoothedImage))
				{
					contrastStretch.ApplyInPlace(grayImage);

					grayImage.Save(@"C:\temp3.bmp");
					for (int x = 0; x < bitmap.Width; x++)
					{
						for (int y = 0; y < bitmap.Height; y++)
						{
							Color color = grayImage.GetPixel(x, y);

							if (color.R == 0)
								newBitmap.SetPixel(x, y, Color.White);
							else if (color.R < threshold)
								newBitmap.SetPixel(x, y, Color.Black);
							else
								newBitmap.SetPixel(x, y, Color.White);

						}

					}


				}


			}

			return newBitmap;
		}

	}
}
