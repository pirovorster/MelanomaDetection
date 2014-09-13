using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Algorithms
{
	/// <summary>
	/// Line implementation
	/// </summary>
	public sealed class Line
	{
		private readonly double _angleInRadians;
		private readonly Point _point;

		public Line(double angleInRadians, Point point)
		{
			_angleInRadians =angleInRadians;

			_point = point;
		}

		public double AngleInRadians { get { return _angleInRadians; } }
		public Point Point { get { return _point; } }

	}
}
