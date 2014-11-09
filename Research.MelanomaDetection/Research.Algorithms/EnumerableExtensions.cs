
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Algorithms
{
	/// <summary>
	/// Extension methods for the <see cref="IEnumerable&lt;T&gt;"/> interface
	/// </summary>
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Shuffles a collection randomly.
		/// </summary>
		/// <typeparam name="T">The type of the items in the collection.</typeparam>
		/// <param name="input">The collection to shuffle.</param>
		/// <returns>The shuffled collection.</returns>
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> input)
		{
			if (input == null)
				throw new ArgumentNullException("input");

			Random random = new Random((int)DateTime.Now.Millisecond);
			List<T> sortedList = input.OrderBy(x => random.Next()).ToList();

			foreach (T item in sortedList)
			{
				yield return item;
			}
		}


		public static IEnumerable<List<T>> GetAllCombos<T>(this IEnumerable<T> initial)
		{
			if (initial == null)
				throw new ArgumentNullException("initial");

			List<T> initialList = initial.ToList();

			var ret = new List<List<T>>();

			// The final number of sets will be 2^N (or 2^N - 1 if skipping empty set)
			long setCount = Convert.ToInt64(Math.Pow(2, initialList.Count()));

			// Start at 1 if you do not want the empty set
			for (long mask = 0; mask < setCount; mask++)
			{
				var nestedList = new List<T>();
				for (int j = 0; j < initialList.Count(); j++)
				{
					// Each position in the initial list maps to a bit here
					var pos = 1 << j;
					if ((mask & pos) == pos) { nestedList.Add(initialList[j]); }
				}
				yield return nestedList; 
			}
		}
	}
}
