namespace GZipTest
{
    /// <summary>
    /// Declares the strategy how the window of the stream should be moved
    /// </summary>
    internal interface IStreamWindowMovingStrategy
    {
        /// <summary>
        /// Moves the window to the next position
        /// </summary>
        /// <returns>The new window</returns>
        StreamWindow Move();

        /// <summary>
        /// Gets the result whether the window can be moved 
        /// </summary>
        /// <returns>True if window can be moved; otherwise False</returns>
        bool CanBeMoved();
    }
}
