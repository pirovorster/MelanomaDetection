using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Algorithms
{
	internal sealed class GroupSplitMask : IMask
	{
		#region Instance Fields

		private readonly int[] _mask;
		private readonly HashSet<int> _values;

		#endregion

		#region Instance Properties

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

		internal GroupSplitMask(int[] mask, IEnumerable<int> values, int width, int height)
		{
			if (mask == null)
				throw new ArgumentNullException("mask");

			if (values == null)
				throw new ArgumentNullException("values");


			_mask = mask;
			_values = new HashSet<int>(values);
			_width = width;
			_height = height;
		}

		#endregion

		#region Instance Methods

		public bool GetCell(int column, int row)
		{
			int index = row * Width + column;

			return _values.Contains(_mask[index]) ;
		}

		#endregion
	}
}
