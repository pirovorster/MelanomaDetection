using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Algorithms
{
	

	/// <summary>
	/// The contract for a class that defines a classification algorithm
	/// </summary>
	public interface IClassifier
	{
		/// <summary>
		/// Calculates multiple classifications based on an array of input features
		/// </summary>
		/// <param name="inputs">input features</param> 
		/// <returns>Classifications</returns>
		double[] Classify(double[] inputs);
	}
}
