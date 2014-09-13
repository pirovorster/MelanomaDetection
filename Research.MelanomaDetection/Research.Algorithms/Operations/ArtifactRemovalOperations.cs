using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Linq;
using Research.Algorithms.SpecializedDataStructures;
using System.Collections;

namespace Research.Algorithms
{
	/// <summary>
	/// Common mask operations
	/// </summary>
	public static class ArtifactRemovalOperations
	{

		/// <summary>
		///Creates a new mask from any IMask
		/// </summary>
		/// <param name="mask">Mask to create a new mask from</param>
		/// <returns>Returns a new Mask</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="mask"/> is <c>null</c>.</exception>
		public static IMask RemoveHairs(this IMask mask)
		{

			int width = mask.Width;
			int height = mask.Height;
			int threshold = 20;
			BitArray bitArray = new BitArray(height * width);

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					int horizontalCount=0;
					int xLeft = x-1;
					while (xLeft >= 0 && mask.GetCell(xLeft, y) && horizontalCount < threshold)
					{
						horizontalCount++;
						xLeft--;
					}

					int xRight = y + 1;
					while (xLeft < width && mask.GetCell(xRight, y) && horizontalCount < threshold)
					{
						horizontalCount++;
						xRight++;
					}

					int verticalCount = 0;
					int yUp = y - 1;
					while (yUp >= 0 && mask.GetCell(x, yUp) && verticalCount < threshold)
					{
						verticalCount++;
						yUp--;
					}

					int yDown = y + 1;
					while (yDown < height && mask.GetCell(x, yDown) && verticalCount < threshold)
					{
						verticalCount++;
						yDown++;
					}

					if (verticalCount >= threshold && horizontalCount >= threshold)
					{

						int index = y * width + x;
						bitArray[index] = true;
					}
				}
			}

			return new Mask(bitArray, width, height);
		}



		

	}
}
