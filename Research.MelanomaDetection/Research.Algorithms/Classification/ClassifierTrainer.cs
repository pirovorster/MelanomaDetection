
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Algorithms
{
	public sealed class ClassifierTrainer
	{

		public double[] TrainWeights(IOptimizer optimizer, Func<double[], IClassifier> classifierFactory, IEnumerable<ClassifierTrainingItem> trainingItems)
		{
			if (trainingItems == null || trainingItems.Any(o => o == null))
				throw new ArgumentException("trainingItems cannot be null or have null items");
			if (optimizer == null)
				throw new ArgumentNullException("optimizer");
			if (classifierFactory == null)
				throw new ArgumentNullException("classifierFactory");

			OptimizationErrorFunction errorFunction = (weights)=>
				{
					IClassifier neuralNetwork = classifierFactory(weights);
					double error=0;
					int count = 0;
					foreach(ClassifierTrainingItem trainingItem in trainingItems)
					{
						double[] outputs = neuralNetwork.Classify(trainingItem.Features);
						int outputsCount = trainingItem.Outputs.Count();

						for (int outputIndex = 0; outputIndex < outputsCount; outputIndex++)
						{
							double diff = outputs[outputIndex] - trainingItem.Outputs[outputIndex];
							double featureError = diff * diff;
							error += featureError;
							count++;
						}
					}

					return error / count;
				};

			return optimizer.FindSolution(errorFunction);
		}
	}
}
