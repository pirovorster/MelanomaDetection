
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Algorithms
{
	/// <summary>
	/// A class that implements a Neural network layer
	/// </summary>
	public sealed class NeuralNetworkLayer
	{

		private readonly double[,] _weightsMatrix;
		private readonly double[] _biasWeights;
		private readonly Func<double, double> _activationFunction;
		private readonly int _inputsCount;
		private readonly int _outputsCount;
		/// <summary>
		/// Construcs Neural Network Layer
		/// </summary>
		/// <param name="weightVector">The weights of the current layer</param>
		/// <param name="activationFunction">Function used to normalize the output</param>
		/// <param name="biases">Bias weights</param>
		public NeuralNetworkLayer(double[,] weightsMatrix,double[] biasWeights, Func<double, double> activationFunction)
		{
			
			if (weightsMatrix == null)
				throw new ArgumentNullException("weightsMatrix");
			if (biasWeights == null)
				throw new ArgumentNullException("biasWeights");
			if (activationFunction == null)
				throw new ArgumentNullException("activationFunction");
			if (biasWeights.Length != weightsMatrix.GetLength(1))
				throw new ArgumentException("Bias must correlate with the output matrix dimension");
			
			_weightsMatrix = weightsMatrix;
			_biasWeights = biasWeights;
			_activationFunction = activationFunction;
			_inputsCount = weightsMatrix.GetLength(0);
			_outputsCount = weightsMatrix.GetLength(1);

		}


		/// <summary>
		/// Calculates the output vector from the input vector
		/// </summary>
		/// <param name="inputs">input vector</param> 
		/// <returns>output vector</returns>
		public double[] Calculate(double[] inputs)
		{

			if (inputs == null)
				throw new ArgumentNullException("inputs");
			if (inputs.Length != _inputsCount)
				throw new ArgumentException("input array must have the same dimension as the first dimension of the weight matrix.");

			double[] outputs = new double[_outputsCount];
			for (int indexOutput = 0; indexOutput < _outputsCount; indexOutput++)
			{
				for (int indexInput = 0; indexInput < _inputsCount; indexInput++)
				{
					outputs[indexOutput] += inputs[indexInput] * _weightsMatrix[indexInput, indexOutput];
				}
				outputs[indexOutput] = _activationFunction(_biasWeights[indexOutput]+outputs[indexOutput]);
			}
			//[]+
			return outputs;

		}

		/// <summary>
		/// The amount of inputs expected
		/// </summary>
		public int InputCount
		{
			get { return _inputsCount; }
		}

		/// <summary>
		/// The amount of outputs to output
		/// </summary>
		public int OutputsCount
		{
			get { return _outputsCount; }
		}

	
	}
}
