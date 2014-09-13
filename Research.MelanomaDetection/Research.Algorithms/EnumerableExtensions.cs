
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
	}
}
