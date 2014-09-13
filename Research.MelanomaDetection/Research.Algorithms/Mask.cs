using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections;

namespace Research.Algorithms
{
	/// <summary>
	/// A matrix mask
	/// </summary>
	public sealed class Mask : IMask
	{

		#region Instance Properties

		private readonly BitArray _maskData;
		internal BitArray MaskData { get { return _maskData; } }

		private readonly int _width;
		/// <summary>
		/// The width of the mask.
		/// </summary>
		public int Width { get { return _width; } }

		private readonly int _height;
		/// <summary>
		/// The height of the mask.
		/// </summary>
		public int Height { get { return _height; } }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for Mask 
		/// </summary>
		/// <param name="maskData">The mask data</param>
		/// <param name="width">The mask width</param>
		/// <param name="height">The mask height</param>
		public Mask(BitArray maskData, int width, int height)
		{
			if (maskData == null)
				throw new ArgumentNullException("maskData");

			_width = width;
			_height = height;

			_maskData = maskData;
		}

		#endregion

		/// <summary>
		/// Obtains a specific mask value from the mask based on column and row index
		/// </summary>
		/// <param name="column">The column index for the desired mask value</param>
		/// <param name="row">The row index for the desired  mask value</param>
		/// <returns></returns>
		public bool GetCell(int column, int row)
		{
			int baseIndex = row * Width;
			int index = baseIndex + column;
			return _maskData[index];
		}

		/// <summary>
		/// Sets a value of the mask
		/// </summary>
		/// <param name="column">The column index for the desired mask value</param>
		/// <param name="row">The row index for the desired  mask value</param>
		/// <param name="value">The value to set the cell</param>
		public void SetCell(int column, int row, bool value)
		{
			int baseIndex = row * Width;
			int index = baseIndex + column;
			_maskData[index] = value;
		}


		internal IMask Combine(IMask mask)
		{
			if (mask == null)
				throw new ArgumentNullException("mask");

			Mask normalMask = mask as Mask;
			if (mask != null )
				return new Mask(((BitArray)_maskData.Clone()).Or(normalMask.MaskData), _width, _height);
			else
			{
				int arraySize = _height * _width;
				BitArray maskData = new BitArray(arraySize);

				for (int x = 0; x < _width; x++)
				{
					for (int y = 0; y < _height; y++)
					{
						int index = y * _width + x;
						maskData[index] = _maskData[index] || mask.GetCell(x, y);
					}
				}

				return new Mask(maskData, _width, _height);
			}
		}

	}
}
