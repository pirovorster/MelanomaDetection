using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Algorithms
{
	/// <summary>
	/// A class that implements a neural network
	/// </summary>
	public sealed class NeuralNetwork : IClassifier 
	{
		private readonly int _numberOfFeatures;
		private readonly IEnumerable<NeuralNetworkLayer> _hiddenLayers;
		private readonly NeuralNetworkLayer _outputLayer;

		public NeuralNetwork(int numberOfFeatures, IEnumerable<NeuralNetworkLayer> hiddenLayers, NeuralNetworkLayer outputLayer)
		{
			if (numberOfFeatures < 1)
				throw new ArgumentOutOfRangeException("numberOfFeatures");
			if (hiddenLayers == null || hiddenLayers.Any(o=>o==null))
				throw new ArgumentException("hiddenLayers cannot be null or have null items");
			if (outputLayer == null)
				throw new ArgumentNullException("outputLayer");

			int output = numberOfFeatures;
			foreach(NeuralNetworkLayer hiddenLayer in hiddenLayers)
			{
				if (hiddenLayer.InputCount == output)
					output = hiddenLayer.OutputsCount;
				else
					throw new ArgumentException("The output dimensions of one layer should match the input dimensions of the next");
			}

			if (outputLayer.InputCount != output)
				throw new ArgumentException("The output dimensions of one layer should match the input dimensions of the next");

			_numberOfFeatures = numberOfFeatures;
			_hiddenLayers = hiddenLayers.ToList();
			_outputLayer = outputLayer;

		}

		/// <summary>
		/// Calculates multiple classifications based on an array of input features using a neural network
		/// </summary>
		/// <param name="inputs">input features</param> 
		/// <returns>Classifications</returns>
		public double[] Classify(double[] inputs)
		{

			if (inputs == null)
				throw new ArgumentNullException("inputs");
			if (inputs.Length != _numberOfFeatures)
				throw new ArgumentException("input array must have the same dimension as the first dimension of the number of features specified.");

			double[] output = inputs;
			foreach (NeuralNetworkLayer hiddenLayer in _hiddenLayers)
			{
				output=hiddenLayer.Calculate(output);
			}
			output = _outputLayer.Calculate(output);
			return output;

		}
	}
}
