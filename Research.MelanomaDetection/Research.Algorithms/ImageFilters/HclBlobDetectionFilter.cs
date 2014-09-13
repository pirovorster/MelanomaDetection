
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
	/// A filter that can be used to create a mask of all the segments on an image
	/// </summary>
	public sealed class HclBlobDetectionFilter : IImageToMaskFilter
	{
		private readonly float _threshold;
		private readonly float _chromaWeight;
		private readonly float _lightnessWeight;

		/// <summary>
		/// Contructs a segmentationFilter
		/// </summary>
		/// <param name="threshold">The threshold to use to determine wether the point on the HCL color map should be part of the segment</param>
		/// <param name="chromaWeight">How much weight should we give to the chroma dimention in the color map</param>
		/// <param name="lightnessWeight">How much weight should we give lightness in the color map</param>
		public HclBlobDetectionFilter(float threshold, float chromaWeight = 1, float lightnessWeight = 1)
		{
			_threshold = threshold;
			_chromaWeight = chromaWeight;
			_lightnessWeight = lightnessWeight;
		}

		/// <summary>
		/// Creates the segment mask of the image
		/// </summary>
		/// <param name="bitmap">The source bitmap</param>
		/// <returns>The new mask of the segments</returns>
		public IMask Apply(Bitmap bitmap)
		{
			if (bitmap == null)
				throw new ArgumentNullException("bitmap");
		


			int width = bitmap.Width;
			int height = bitmap.Height;
			int size = width * height;
			BitArray maskData = new BitArray(size);
			const int squareMaskSideLength = 3;

			// If too small return empty mask
			if (height < squareMaskSideLength || width < squareMaskSideLength)
			{
				return new Mask(new BitArray(size), width, height);
			}

			using (FastBitmap fastBitmap = new FastBitmap(bitmap))
			{

				unsafe
				{
					const int valuesCount = 3;
					int cacheLineSize = width * valuesCount;	// for each pixel store Hue, Saturation and Brightness
					int cacheSize = cacheLineSize * squareMaskSideLength;
					float* cache = stackalloc float[cacheSize];
					for (int y = 0; y < squareMaskSideLength - 1; ++y)
					{
						for (int x = 0; x < width; ++x)
						{
							int cacheStartIndex = (cacheLineSize * y) + (x * valuesCount);
							float* cacheStartPointer = (cache + cacheStartIndex);
							Color pixel = fastBitmap.GetPixel(x, y);
							SetPointInCache(pixel, cacheStartPointer);


						}
					}

					for (int y = squareMaskSideLength - 1; y < height; ++y)
					{
						int cacheYPosition = y % squareMaskSideLength;
						for (int x = 0; x < squareMaskSideLength - 1; ++x)
						{
							int cacheStartIndex = (cacheLineSize * cacheYPosition) + (x * valuesCount);
							float* cacheStartPointer = (cache + cacheStartIndex);
							Color pixel = fastBitmap.GetPixel(x, y);
							SetPointInCache(pixel, cacheStartPointer);

						}

						int bitMaskY = y - 1;
						for (int x = squareMaskSideLength - 1; x < width; ++x)
						{
							int cacheStartIndex = (cacheLineSize * cacheYPosition) + (x * valuesCount);
							float* cacheStartPointer = (cache + cacheStartIndex);
							Color pixel = fastBitmap.GetPixel(x, y);
							SetPointInCache(pixel, cacheStartPointer);

							float xCurrent = *(cacheStartPointer);
							float yCurrent = *(cacheStartPointer + 1);
							float zCurrent = *(cacheStartPointer + 2);

							bool exceedsThreshold = false;
							for (int evalucationX = x - (squareMaskSideLength - 1); evalucationX <= x; ++evalucationX)
							{
								if (!exceedsThreshold)
								{
									int evalucationPointCacheIndex = (cacheLineSize * (y % squareMaskSideLength)) + (evalucationX * valuesCount);
									float* evaluationPointPointer = (cache + evalucationPointCacheIndex);

									float xNeighbour = *(evaluationPointPointer);
									float yNeighbour = *(evaluationPointPointer + 1);
									float zNeighbour = *(evaluationPointPointer + 2);

									float xDiff = xCurrent - xNeighbour;
									float yDiff = yCurrent - yNeighbour;
									float zDiff = zCurrent - zNeighbour;

									if (Math.Sqrt(xDiff * xDiff + yDiff * yDiff + zDiff * zDiff) > _threshold)
										exceedsThreshold = true;
								}
							}


							for (int evalucationY = y - (squareMaskSideLength - 1); evalucationY <= y; ++evalucationY)
							{
								if (!exceedsThreshold)
								{
									int evalucationPointCacheIndex = (cacheLineSize * (evalucationY % squareMaskSideLength)) + (x * valuesCount);
									float* evaluationPointPointer = (cache + evalucationPointCacheIndex);

									float xNeighbour = *(evaluationPointPointer);
									float yNeighbour = *(evaluationPointPointer + 1);
									float zNeighbour = *(evaluationPointPointer + 2);

									float xDiff = xCurrent - xNeighbour;
									float yDiff = yCurrent - yNeighbour;
									float zDiff = zCurrent - zNeighbour;

									if (Math.Sqrt(xDiff * xDiff + yDiff * yDiff + zDiff * zDiff) > _threshold)
										exceedsThreshold = true;
								}
							}

							if (!exceedsThreshold)
							{
								int bitMaskX = x - 1;
								int bitMaskIndex = (bitMaskY * width) + bitMaskX;
								maskData[bitMaskIndex] = true;
							}
						}
					}
				}


			}

			return new Mask(maskData, bitmap.Width, bitmap.Height);
		}

		
		private unsafe  void SetPointInCache(Color color, float* cachePointer)
		{

			const float rgbMax = 255;
			const float floatPi = (float)Math.PI;
			const float radians360Degrees = floatPi * 2;
			const float radians90Degrees = (float)Math.PI / 2;
			const float radians270Degrees = radians90Degrees * 3;

			float r = color.R / rgbMax;
			float g = color.G / rgbMax;
			float b = color.B / rgbMax;

			float max = Math.Max(Math.Max(r, g), b);

			float min = Math.Min(Math.Min(r, g), b);

			float chroma = (max - min);

			float lightness = 0.5f * (max + min);

			float hueRadians = 0;


			if (chroma == 0)
			{
				hueRadians = 0;
			}
			else if (r == max)
			{
				hueRadians = floatPi * (((g - b) / chroma) % 6) / 3;
			}
			else if (g == max)
			{
				hueRadians = floatPi * (((b - r) / chroma) + 2) / 3;
			}
			else if (b == max)
			{
				hueRadians = floatPi * (((r - g) / chroma) + 4) / 3;
			}

			float hypotenuse = (hueRadians <= floatPi && hueRadians > radians90Degrees) || (hueRadians < radians360Degrees && hueRadians >= radians270Degrees) ? -chroma * _chromaWeight : chroma * _chromaWeight;

			*(cachePointer) = (float)Trig.Sine(hueRadians) * hypotenuse;
			*(cachePointer + 1) = (lightness * 2 - 1) * _lightnessWeight;
			*(cachePointer + 2) = (float)Trig.Cosine(hueRadians) * hypotenuse;

		}

	}
}
