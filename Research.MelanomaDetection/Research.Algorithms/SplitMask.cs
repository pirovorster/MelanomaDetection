
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Algorithms
{
	internal sealed class SplitMask : IMask
	{
		#region Instance Fields

		private readonly int[] _mask;
		private readonly int _value;

		#endregion

		#region Instance Properties

		internal int Value
		{
			get { return _value; }
		}

		private readonly int _width;
		public int Width
		{
			get { return _width; }
		}

		private readonly int _height;
		public int Height
		{
			get { return _height; }
		}

		#endregion

		#region Constructors

		internal SplitMask(int[] mask, int value, int width, int height)
		{
			if (mask == null)
				throw new ArgumentNullException("mask");

			_mask = mask;
			_value = value;
			_width = width;
			_height = height;
		}

		#endregion

		#region Instance Methods

		public bool GetCell(int column, int row)
		{
			int index = row * Width + column;

			return _mask[index] == _value;
		}

		internal bool CheckIfSameReferenceType(SplitMask splitMask)
		{
			if(splitMask == null)
				throw new ArgumentNullException("splitMask");

			if (ReferenceEquals(_mask, splitMask._mask))
				return true;
			else
				return false;
		}

		internal IMask Combine(IMask mask)
		{
			if (mask == null)
				throw new ArgumentNullException("mask");
			

			SplitMask splitMask = mask as SplitMask;
			if (splitMask != null && ReferenceEquals(_mask, splitMask._mask))
				return new GroupSplitMask(_mask, new[] { _value, splitMask.Value }, Width, Height);
			else
			{
				int arraySize = _height * _width ;
				BitArray maskData = new BitArray(arraySize);

				for (int x= 0; x < _width; x++)
				{
					for (int y = 0; y < _height; y++)
					{
						int index = y * _width + x;
						maskData[index] = _mask[index]==0|| mask.GetCell(x, y);
					}
				}

				return new Mask(maskData,_width,_height);
			}
		}

		#endregion
	}
}
