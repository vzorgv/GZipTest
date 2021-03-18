namespace GZipTest
{
    /// <summary>
    /// Supports a atomic iteration over a generic collection
    /// </summary>
    /// <typeparam name="T">The type of objects to enumerate</typeparam>
    internal interface IAtomicEnumerator<T>
    {
        /// <summary>
        /// Advances the enumerator to the next element of the collection
        /// </summary>
        /// <param name="current">The current element of collection</param>
        /// <returns>True if the next element exist in collection; otherwise False</returns>
        bool TryMoveNext(out T current);
    }
}
