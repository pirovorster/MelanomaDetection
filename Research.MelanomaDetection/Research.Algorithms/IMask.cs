using System;
namespace Research.Algorithms
{
    /// <summary>
    /// A matrix mask interface
    /// </summary>
	public interface IMask
    {
        /// <summary>
        /// The width of the mask.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The height of the mask.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the value of the cell for the current mask at the provided by the column and row indexes
        /// </summary>
        /// <param name="column">The column index</param>
        /// <param name="row">The row index.</param>
        /// <returns></returns>
        bool GetCell(int column, int row);
    }
}
