using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Algorithms
{
	public sealed class ClassifierTrainingItem
	{
		private readonly double[] _features;
		private readonly double[] _outputs;
		public ClassifierTrainingItem(double[] features, double[] outputs)
		{
			_features = features;
			_outputs = outputs;
		}

		public double[] Features { get { return _features; } }
		public double[] Outputs { get { return _outputs; } }

	}
}
