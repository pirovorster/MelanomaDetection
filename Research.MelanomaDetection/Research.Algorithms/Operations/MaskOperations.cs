using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Linq;
using Research.Algorithms.SpecializedDataStructures;
using System.Collections;
using MathNet.Numerics;

namespace Research.Algorithms
{
	/// <summary>
	/// Common mask operations
	/// </summary>
	public static class MaskOperations
	{

		/// <summary>
		/// Fills small holes within a mask
		/// </summary>
		/// <param name="blobMask">The mask we want to fill</param>
		/// <param name="minimumHoleSize">The minimum area that a hole is allowed to be before it is considered part of the blob</param>
		/// <returns>The filled mask</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="blobMask"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="minimumHoleSize"/> is less than<c>0</c>.</exception>
		public static void FillBlobMask(this IMask blobMask, int minimumHoleSize)
		{

			if (blobMask == null)
				throw new ArgumentNullException("blobMask");

			Queue<Point> markedQueue = new Queue<Point>();
			Queue<Point> fillQueue = new Queue<Point>();

			HashSet<int> processedPoints = new HashSet<int>();

			Mask mask = blobMask as Mask;
			if (mask == null)
				mask = blobMask.ToMask();

			int height = mask.Height;
			int width = mask.Width;

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					int count = 0;
					bool isNotMarked = !blobMask.GetCell(x, y);
					Point point = new Point(x, y);
					int index = y * width + x;


					if (isNotMarked && !processedPoints.Contains(index))
					{
						markedQueue.Enqueue(point);
						processedPoints.Add(index);

						while (markedQueue.Count != 0)
						{
							Point precessingPoint = markedQueue.Dequeue();
							if (count < minimumHoleSize)
								fillQueue.Enqueue(precessingPoint);
							count++;

							for (int xAdjacent = precessingPoint.X - 1; xAdjacent <= precessingPoint.X + 1; xAdjacent++)
							{
								for (int yAdjacent = precessingPoint.Y - 1; yAdjacent <= precessingPoint.Y + 1; yAdjacent++)
								{
									if (xAdjacent > 0 && yAdjacent > 0 && xAdjacent < width && yAdjacent < height)
									{
										int adjacentIndex = yAdjacent * width + xAdjacent;

										Point adjacentPoint = new Point(xAdjacent, yAdjacent);
										if (!processedPoints.Contains(adjacentIndex) && !mask.GetCell(xAdjacent, yAdjacent))
										{
											markedQueue.Enqueue(adjacentPoint);
											processedPoints.Add(adjacentIndex);
										}
									}
								}
							}

						}

						if (count < minimumHoleSize)
						{

							while (fillQueue.Count != 0)
							{
								Point fillPoint = fillQueue.Dequeue();
								mask.SetCell(fillPoint.X, fillPoint.Y, true);
							}

						}

						fillQueue.Clear();
					}
				}
			}
		}

		/// <summary>
		/// A list of masks for each blob within a significant area
		/// </summary>
		/// <param name="blobMask">The mask from which we want to extract the blobs</param>
		/// <param name="significanceArea">Pixel area within which the blob needs to be</param>
		/// <returns>The a collection of masks each with a blob</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="blobMask"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="significanceArea"/> is less than one.</exception>
		public static IEnumerable<IMask> SplitBlobMask(this IMask blobMask, int significanceArea)
		{
			if (blobMask == null)
				throw new ArgumentNullException("blobMask");

			int width = blobMask.Width;
			int height = blobMask.Height;
			int size = width * height;
			int[] scans = new int[size];

			// first paint values
			Dictionary<int, int> counts = new Dictionary<int, int>();
			SplitMaskMergerList mappings = new SplitMaskMergerList((width * 5) + 1, (width / 2) + 1);

			unsafe
			{
				int indexUpperLimit = scans.Length;
				fixed (int* scansPointer = scans)
				{
					for (int index = 0; index < indexUpperLimit; ++index)
					{
						int x = index % width;
						int y = index / width;
						bool isMask = blobMask.GetCell(x, y);
						if (isMask)
						{
							int* currentPointerLocation = scansPointer + index;
							int leftValue = 0;
							if (x > 0)
							{
								leftValue = *(currentPointerLocation - 1);
							}
							int upValue = 0;
							if (y > 0)
							{
								upValue = *(currentPointerLocation - width);
							}

							if (leftValue > 0)
							{
								*(currentPointerLocation) = leftValue;
								counts[leftValue] = counts[leftValue] + 1;

								// mark for merging if needed
								if (upValue > 0)
								{
									leftValue = mappings.GetValue(leftValue);
									upValue = mappings.GetValue(upValue);
									if (upValue != leftValue)
									{
										if (upValue < leftValue)
										{
											mappings.SetValue(leftValue, upValue);
										}
										else
										{
											mappings.SetValue(upValue, leftValue);
										}
									}
								}
							}
							else if (upValue > 0)
							{
								*(currentPointerLocation) = upValue;
								counts[upValue] = counts[upValue] + 1;
							}
							else
							{
								int nextNumber = mappings.GetNext();
								*(currentPointerLocation) = nextNumber;
								counts.Add(nextNumber, 1);
							}
						}
					}

					Dictionary<int, int> mergedBlobsCount = new Dictionary<int, int>();
					int lastKey = mappings.HighestKey;
					for (int key = 1; key <= lastKey; key += 1)
					{
						int value = mappings.GetValue(key);
						if (key == value)
						{
							mergedBlobsCount.Add(value, counts[key]);
						}
						else
						{
							mergedBlobsCount[value] = mergedBlobsCount[value] + counts[key];
						}
					}

					// Set one value per blog i.e. merge
					int* currentScanPointer = scansPointer;
					for (int index = 0; index < indexUpperLimit; ++index)
					{
						int currentValue = *(currentScanPointer);
						if (currentValue > 0)
						{
							// CG: We can directly access mappings here because each one was through the GetMappingsValue method in the above for.
							*(currentScanPointer) = mappings.GetValueRawAccess(currentValue);
						}
						currentScanPointer = (currentScanPointer + 1);
					}

					List<IMask> masks = new List<IMask>();
					foreach (int key in mergedBlobsCount.Keys)
					{
						if (mergedBlobsCount[key] >= significanceArea)
						{
							masks.Add(new SplitMask(scans, key, width, height));
						}
					}
					return masks;
				}
			}
		}

		private static uint GetMappingsValue(Dictionary<uint, uint> mappings, uint key)
		{
			uint value = mappings[key];
			if (value == key)
			{
				return value;
			}

			List<uint> keysToUpdate = new List<uint>();
			keysToUpdate.Add(key);

			while (key != value)
			{
				key = value;
				value = mappings[key];
				keysToUpdate.Add(key);
			}

			foreach (uint keyToUpdate in keysToUpdate)
			{
				mappings[keyToUpdate] = value;
			}

			return value;
		}


		/// <summary>
		///Creates a new mask from any IMask
		/// </summary>
		/// <param name="mask">Mask to create a new mask from</param>
		/// <returns>Returns a new Mask</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="mask"/> is <c>null</c>.</exception>
		public static Mask ToMask(this IMask mask)
		{
			if (mask == null)
				throw new ArgumentNullException("mask");

			int width = mask.Width;
			int height = mask.Height;

			Mask basicMask = mask as Mask;
			if (basicMask != null)
				return new Mask((BitArray)basicMask.MaskData.Clone(), width, height);


			BitArray bitArray = new BitArray(height * width);

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					int index = y * width + x;
					bitArray[index] = mask.GetCell(x, y);
				}
			}

			return new Mask(bitArray, width, height);
		}


		/// <summary>
		///Inverts a mask
		/// </summary>
		/// <param name="mask">The mask to be inverted</param>
		/// <returns>The inverted <see cref="Mask"/></returns>
		/// <exception cref="ArgumentNullException">If <paramref name="mask"/> is <c>null</c>.</exception>
		public static IMask Invert(this IMask mask)
		{

			if (mask == null)
				throw new ArgumentNullException("mask");

			Mask concreteMask = mask as Mask;

			if (concreteMask == null)
				concreteMask = new Mask((BitArray)mask.ToMask().MaskData.Not(), mask.Width, mask.Height);
			else
				concreteMask = new Mask(((BitArray)concreteMask.MaskData.Clone()).Not(), mask.Width, mask.Height);

			return concreteMask;
		}

		/// <summary>
		///Counts cells in the mask that is set
		/// </summary>
		/// <param name="mask">The mask to count on</param>
		/// <returns>The sum of set cells </returns>
		/// <exception cref="ArgumentNullException">If <paramref name="mask"/> is <c>null</c>.</exception>
		/// http://stackoverflow.com/questions/5063178/counting-bits-set-in-a-net-bitarray-class
		public static int Sum(this IMask mask)
		{
			if (mask == null)
				throw new ArgumentNullException("mask");

			Mask concreteMask = mask as Mask;
			if (concreteMask == null)
				concreteMask = mask.ToMask();

			int[] ints = new int[(concreteMask.MaskData.Count >> 5) + 1];

			concreteMask.MaskData.CopyTo(ints, 0);

			int count = 0;

			// fix for not truncated bits in last integer that may have been set to true with SetAll()
			ints[ints.Length - 1] &= ~(-1 << (concreteMask.MaskData.Count % 32));

			for (Int32 i = 0; i < ints.Length; i++)
			{

				Int32 c = ints[i];

				// magic (http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel)
				unchecked
				{
					c = c - ((c >> 1) & 0x55555555);
					c = (c & 0x33333333) + ((c >> 2) & 0x33333333);
					c = ((c + (c >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
				}

				count += c;

			}

			return count;
		}

		/// <summary>
		///Gets the logical and between 2 masks
		/// </summary>
		/// <param name="mask">First mask</param>
		///  <param name="secondMask">Second mask to be added</param>
		/// <returns>The result of anding 2 masks</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="mask"/> is <c>null</c>.</exception>
		public static IMask And(this IMask mask, IMask secondMask)
		{
			if (mask == null)
				throw new ArgumentNullException("mask");
			if (secondMask == null)
				throw new ArgumentNullException("secondMask");


			Mask concreteMaskOne = mask as Mask;
			Mask concreteMaskTwo = secondMask as Mask;

			if (concreteMaskTwo == null)
				concreteMaskTwo = secondMask.ToMask();

			if (concreteMaskOne == null)
				concreteMaskOne = new Mask((BitArray)mask.ToMask().MaskData.And(concreteMaskTwo.MaskData), mask.Width, mask.Height);
			else
				concreteMaskOne = new Mask(((BitArray)concreteMaskOne.MaskData.Clone()).And(concreteMaskTwo.MaskData), mask.Width, mask.Height);

			return concreteMaskOne;
		}

		/// <summary>
		///Add 2 masks together to produce a new mask
		/// </summary>
		/// <param name="mask">First mask to be added</param>
		/// <param name="adder">Second mask to be added</param>
		/// <returns>The the some of the 2 masks </returns>
		/// <exception cref="ArgumentNullException">If <paramref name="mask"/> is <c>null</c>.</exception>
		///  <exception cref="ArgumentNullException">If <paramref name="adder"/> is <c>null</c>.</exception>
		public static IMask Add(this IMask mask, IMask adder)
		{
			if (adder == null)
				throw new ArgumentNullException("adder");

			if (mask == null)
				throw new ArgumentNullException("mask");

			SplitMask splitMaskMask = mask as SplitMask;
			SplitMask splitMaskAdder = adder as SplitMask;
			Mask normalMaskMask = mask as Mask;
			Mask normalMaskAdder = adder as Mask;
			if (splitMaskMask != null && splitMaskAdder != null)
				return splitMaskMask.Combine(splitMaskAdder);
			else if (normalMaskMask != null && normalMaskAdder != null)
				return normalMaskMask.Combine(normalMaskAdder);
			else if (splitMaskMask != null && normalMaskAdder != null)
				return splitMaskMask.Combine(normalMaskAdder);
			else if (splitMaskAdder != null && normalMaskMask != null)
				return normalMaskMask.Combine(splitMaskAdder);
			else
				throw new NotSupportedException();

		}


		/// <summary>
		/// Finds the most center mask in a group of blobs
		/// </summaobry>
		/// <param name="mask">source mask</param>
		/// <returns>center mask</returns>
		public static IMask MostCenterMask(this IMask mask)
		{
			if (mask == null)
				throw new ArgumentNullException("mask");

			BitArray maskData = new BitArray(mask.Width * mask.Height);

			int width = mask.Width;
			int height = mask.Height;

			int centerX = width / 2;
			int centerY = height / 2;

			HashSet<Point> searchedCenterPoints = new HashSet<Point>();
			Queue<Point> searchCenterPoints = new Queue<Point>();

			Point point = new Point(centerX, centerY);

			searchedCenterPoints.Add(point);
			searchCenterPoints.Enqueue(point);

			bool foundMask = false;
			Point foundPoint = default(Point);
			while (foundMask == false && searchCenterPoints.Count != 0)
			{
				foundPoint = searchCenterPoints.Dequeue();
				if (mask.GetCell(foundPoint.X, foundPoint.Y))
				{
					foundMask = true;
				}
				else
				{
					for (int x = foundPoint.X - 1; x <= foundPoint.X + 1; x++)
					{
						for (int y = foundPoint.Y - 1; y <= foundPoint.Y + 1; y++)
						{
							Point adjacentPoint = new Point(x, y);
							if (!searchedCenterPoints.Contains(adjacentPoint) && y >= 0 && y < height && x >= 0 && x < width)
							{
								searchCenterPoints.Enqueue(adjacentPoint);
								searchedCenterPoints.Add(adjacentPoint);
							}
						}
					}
				}
			}

			HashSet<Point> markedPoints = new HashSet<Point>();
			Queue<Point> maskPoints = new Queue<Point>();
			maskPoints.Enqueue(foundPoint);
			markedPoints.Add(foundPoint);

			while (maskPoints.Count != 0)
			{
				Point currentPoint = maskPoints.Dequeue();

				int index = currentPoint.Y * width + currentPoint.X;
				maskData.Set(index, true);


				for (int y = currentPoint.Y - 1; y <= currentPoint.Y + 1; y++)
				{
					Point adjacentPoint = new Point(currentPoint.X, y);
					if (!markedPoints.Contains(adjacentPoint) && y >= 0 && y < height && mask.GetCell(adjacentPoint.X, adjacentPoint.Y))
					{
						maskPoints.Enqueue(adjacentPoint);
						markedPoints.Add(adjacentPoint);
					}
				}

				for (int x = currentPoint.X - 1; x <= currentPoint.X + 1; x++)
				{
					Point adjacentPoint = new Point(x, currentPoint.Y);
					if (!markedPoints.Contains(adjacentPoint) && x >= 0 && x < width && mask.GetCell(adjacentPoint.X, adjacentPoint.Y))
					{
						maskPoints.Enqueue(adjacentPoint);
						markedPoints.Add(adjacentPoint);
					}
				}
			}


			return new Mask(maskData, width, height);

		}

		/// <summary>
		/// Finds the center of mass of a mask
		/// </summary>
		/// <param name="mask">The mask of which to find the center of mass</param>
		/// <returns>The center of mass of the mask</returns>
		public static Point Centroid(this IMask mask)
		{
			int width = mask.Width;
			int height = mask.Height;

			int points = 0;
			int xSum = 0;
			int ySum = 0;

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (mask.GetCell(x, y))
					{
						xSum += x;
						ySum += y;
						points++;
					}

				}
			}

			return new Point(xSum / points, ySum / points);

		}

		/// <summary>
		/// Finds the moment of a mask
		/// </summary>
		/// <param name="mask">The mask</param>
		/// <param name="centroid">The centroid of the mask</param>
		/// <param name="i">The i component of the centroid</param>
		/// <param name="j">The j component of the centroid</param>
		/// <returns>Returns the moment</returns>
		public static double CaclulateMoment(this IMask mask, Point centroid, int i, int j)
		{
			int width = mask.Width;
			int height = mask.Height;

			double moment = 0;

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (mask.GetCell(x, y))
					{
						moment += Math.Pow((x - centroid.X), i) * Math.Pow((y - centroid.Y), j);
					}

				}
			}

			return moment;


		}

		public static Line GetPrimaryAxisOfSymmetry(this IMask mask)
		{
			Point centroid = mask.Centroid();
			double moment20 = mask.CaclulateMoment(centroid, 2, 0);
			double moment11 = mask.CaclulateMoment(centroid, 1, 1);
			double moment00 = mask.CaclulateMoment(centroid, 0, 0);
			double moment02 = mask.CaclulateMoment(centroid, 0, 2);

			double covariantMoment20 = moment20 / moment00;
			double covariantMoment11 = moment11 / moment00;
			double covariantMoment02 = moment02 / moment00;

			double angle = 0.5 * Math.Tanh((2 * covariantMoment11) / (covariantMoment20 - covariantMoment02));
			return new Line(angle, centroid);

		}

		public static Line GetSecondaryAxisOfSymmetry(this IMask mask)
		{
			Line primary = GetPrimaryAxisOfSymmetry(mask);
			return new Line(primary.AngleInRadians + Math.PI / 2, primary.Point);

		}

		public static int GetNonOverlappingArea(this IMask mask, Line axisOfSymmetry)
		{
			double centroidX = axisOfSymmetry.Point.X;
			double centroidY = axisOfSymmetry.Point.Y;

			double newX = Math.Cos(axisOfSymmetry.AngleInRadians) * 2000 + axisOfSymmetry.Point.X;
			double newY = Math.Sin(axisOfSymmetry.AngleInRadians) * 2000 + axisOfSymmetry.Point.Y;

			int width = mask.Width;
			int height = mask.Height;

			int nonOverlap = 0;

			for (int x = 0; x < width; x++)
			{

				for (int y = 0; y < height; y++)
				{

					if (mask.GetCell(x, y))
					{
						double x1 = newX;
						double y1 = newY;
						double x2 = centroidX;
						double y2 = centroidY;

						double k = ((y2 - y1) * (x - x1) - (x2 - x1) * (y - y1)) / ((y2 - y1) * (y2 - y1) + (x2 - x1) * (x2 - x1));
						double x4 = x - k * (y2 - y1);
						double y4 = y + k * (x2 - x1);

						int mirrorX = (int)Math.Round(x4 - (x - x4), 0);

						int mirrorY = (int)Math.Round(y4 - (y - y4), 0);


						if (mirrorX >= width || mirrorX < 0 || mirrorY >= height || mirrorY < 0 || !mask.GetCell(mirrorX, mirrorY))
						{
							nonOverlap++;
						}

					}
				}
			}

			return nonOverlap;

		}


		/// <summary>
		/// Finds the center of mass of a mask
		/// </summary>
		/// <param name="mask">The mask of which to find the center of mass</param>
		/// <returns>The center of mass of the mask</returns>
		public static int Perimeter(this IMask mask)
		{
			int width = mask.Width;
			int height = mask.Height;

			int perimeter = 0;
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (mask.GetCell(x, y))
					{
						int incremet = 0;
						for (int adjacentX = x - 1; adjacentX <= x + 1; adjacentX++)
						{
							if (adjacentX < 0 || adjacentX >= width)
								incremet = 1;
							else
								if (!mask.GetCell(adjacentX, y))
									incremet = 1;
						}

						for (int adjacentY = y - 1; adjacentY <= y + 1; adjacentY++)
						{
							if (adjacentY < 0 || adjacentY >= height)
								incremet = 1;
							else
								if (!mask.GetCell(x, adjacentY))
									incremet = 1;
						}

						perimeter += incremet;
					}
				}
			}

			return perimeter;

		}

		public static double EdgeAverageLightnessChange(this IMask mask, Bitmap image, double skinMeanLightness)
		{
			int width = mask.Width;
			int height = mask.Height;

			double totalChange = 0;
			int count = 0;
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (mask.GetCell(x, y))
					{
						bool isEdge = false;
						for (int adjacentX = x - 1; adjacentX <= x + 1; adjacentX++)
						{
							if (adjacentX < 0 || adjacentX >= width || !mask.GetCell(adjacentX, y))
								isEdge = true;

						}

						for (int adjacentY = y - 1; adjacentY <= y + 1; adjacentY++)
						{
							if (adjacentY < 0 || adjacentY >= height || !mask.GetCell(x, adjacentY))
								isEdge = true;

						}

						if (isEdge)
						{
							totalChange += Math.Abs(image.GetPixel(x, y).GetLightness() - skinMeanLightness);
							count++;
						}

					}
				}
			}

			return totalChange / count;

		}
		public static double Average(this IMask mask, Bitmap image, Func<Color, double> valueFunction)
		{
			int width = mask.Width;
			int height = mask.Height;

			int count = 0;
			double totalValue = 0;
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (mask.GetCell(x, y))
					{
						count++;
						totalValue += valueFunction(image.GetPixel(x, y));

					}

				}

			}

			return totalValue / count;
		}

		public static double Variance(this IMask mask, Bitmap image, Func<Color, double> valueFunction)
		{
			int width = mask.Width;
			int height = mask.Height;

			int count = 0;
			double totalValue = 0;

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (mask.GetCell(x, y))
					{
						count++;
						totalValue += valueFunction(image.GetPixel(x, y));
					}

				}

			}

			double mean = totalValue / count;

			totalValue = 0;
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (mask.GetCell(x, y))
					{
						double value = valueFunction(image.GetPixel(x, y)) - mean;
						totalValue += value * value;
					}

				}

			}

			return totalValue / count;
		}
		public static IEnumerable<double> PercentageBuckets(this IMask mask, Bitmap image, IEnumerable<Func<Color, bool>> bucketFunctions)
		{
			int count = 0;
			int width = mask.Width;
			int height = mask.Height;

			List<Func<Color, bool>> bucketFunctionsList = bucketFunctions.ToList();

			var functions = Enumerable.Range(0, bucketFunctionsList.Count).Select(o => new { Index = o, Function = bucketFunctionsList[o] });
			List<int> counts = Enumerable.Range(0, bucketFunctionsList.Count).Select(o => 0).ToList();

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (mask.GetCell(x, y))
					{
						count++;
						Color pixel = image.GetPixel(x, y);

						foreach (var function in functions)
						{
							if (function.Function(pixel))
								counts[function.Index]++;
						}
					}

				}

			}

			return counts.Select(o => o / (double)count).ToList().AsReadOnly();
		}


		public static IEnumerable<double> PercentageCloseTo(this IMask mask, Bitmap image, IEnumerable<Color> colors, double distanceThreshold)
		{
			int count = 0;
			int width = mask.Width;
			int height = mask.Height;

			List<Color> colorsList = colors.ToList();

			var indexedColors = Enumerable.Range(0, colorsList.Count).Select(o => new { Index = o, Color = colorsList[o] }).Select(o => new { Index = o.Index, HclPoint = o.Color.GetHclPoint() });
			List<int> counts = Enumerable.Range(0, colorsList.Count).Select(o => 0).ToList();

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (mask.GetCell(x, y))
					{
						count++;
						Color pixel = image.GetPixel(x, y);

						foreach (var indexedColor in indexedColors)
						{
							HclPoint hclPoint = pixel.GetHclPoint();

							double diffHue =indexedColor.HclPoint.Hue- hclPoint.Hue;
							double diffChroma = indexedColor.HclPoint.Chroma - hclPoint.Chroma;
							double diffLightness = indexedColor.HclPoint.Lightness - hclPoint.Lightness;

							double distance = Math.Sqrt(diffHue * diffHue + diffChroma * diffChroma + diffLightness * diffLightness);

							if(distance<=distanceThreshold)
								counts[indexedColor.Index]++;
						}
					}

				}

			}

			return counts.Select(o => o / (double)count).ToList().AsReadOnly();
		}

		public static float GetChroma(this Color color)
		{

			const float rgbMax = 255;

			float r = color.R / rgbMax;
			float g = color.G / rgbMax;
			float b = color.B / rgbMax;

			float max = Math.Max(Math.Max(r, g), b);

			float min = Math.Min(Math.Min(r, g), b);

			return (max - min);



		}

		public static float GetLightness(this Color color)
		{
			const float rgbMax = 255;

			float r = color.R / rgbMax;
			float g = color.G / rgbMax;
			float b = color.B / rgbMax;

			float max = Math.Max(Math.Max(r, g), b);

			float min = Math.Min(Math.Min(r, g), b);

			return 0.5f * (max + min);
		}

		private static HclPoint GetHclPoint(this Color color)
		{

			const double rgbMax = 255;
			const double floatPi = (float)Math.PI;
			const double radians360Degrees = floatPi * 2;
			const double radians90Degrees = (float)Math.PI / 2;
			const double radians270Degrees = radians90Degrees * 3;

			double r = color.R / rgbMax;
			double g = color.G / rgbMax;
			double b = color.B / rgbMax;

			double max = Math.Max(Math.Max(r, g), b);

			double min = Math.Min(Math.Min(r, g), b);

			double chroma = (max - min);

			double lightness = 0.5f * (max + min);

			double hueRadians = 0;

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


			double hypotenuse = (hueRadians <= floatPi && hueRadians > radians90Degrees) || (hueRadians < radians360Degrees && hueRadians >= radians270Degrees) ? -chroma : chroma;

			return new HclPoint(Trig.Sine(hueRadians) * hypotenuse, lightness * 2 - 1, Trig.Cosine(hueRadians) * hypotenuse);

		}

		internal class HclPoint
		{
			private readonly double _hue;
			private readonly double _chroma;
			private readonly double _lightness;
			public HclPoint(double hue, double chroma, double lightness)
			{
				_hue = hue;
				_chroma = chroma;
				_lightness = lightness;
			}

			internal double Hue { get { return _hue; } }
			internal double Chroma { get { return _chroma; } }
			internal double Lightness { get { return _lightness; } }

		}

	}


}
