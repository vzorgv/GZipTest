namespace GZipTest.BlockGenerators
{
    /// <summary>
    /// Supports block generation over a collection
    /// </summary>
    /// <typeparam name="T">The type of block to generate</typeparam>
    internal interface IBlockGenerator<T>
    {
        /// <summary>
        /// Advances the enumerator to the next element of the collection
        /// </summary>
        /// <param name="blockSize">The requested size of block</param>
        /// <param name="current">The current element of collection</param>
        /// <returns>True if the next element exist in collection; otherwise False</returns>
        bool TryGetNext(int blockSize, out T current);
    }
}
