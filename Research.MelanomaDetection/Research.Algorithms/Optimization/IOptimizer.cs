using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Algorithms
{
	/// <summary>
	/// Delegate that defines an error function to be optimized
	/// </summary>
	/// <param name="dimensions">input to the error function</param>
	/// <returns></returns>
	public delegate double OptimizationErrorFunction(double[] dimensionValues);

	/// <summary>
	/// The contract for a class that defines optimization algorithm
	/// </summary>
	public interface IOptimizer
	{
		/// <summary>
		/// Searches for a solution 
		/// </summary>
		/// <returns>Returns the answer of n dimensions</returns>
		double[] FindSolution(OptimizationErrorFunction optimizationErrorFunction);
	}
}
