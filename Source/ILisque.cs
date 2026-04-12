namespace Dastardly.Data;

/// <summary>
/// An IList that also acts as a stack and a queue.
/// </summary>
/// <typeparam name="T">The type of items.</typeparam>
public interface ILisque<T> : IList<T>
{
    /// <summary>
    /// Prepends the given item to the start of the ILisque.
    /// </summary>
    /// <param name="item">The item to prepend.</param>
    void PushFirst(T item);
    /// <summary>
    /// Appends the given item to the end of the ILisque.
    /// </summary>
    /// <param name="item">The item to append.</param>
    void PushLast(T item);

    /// <summary>
    /// Removes and returns the first item in the ILisque.
    /// </summary>
    /// <returns>The first item, which will be removed.</returns>
    /// <exception cref="InvalidOperationException">When the ILisque is empty.</exception>
    T PopFirst();
    
    /// <summary>
    /// Removes and returns the last item in the ILisque.
    /// </summary>
    /// <returns>The last item, which will be removed.</returns>
    /// <exception cref="InvalidOperationException">When the ILisque is empty.</exception>
    T PopLast();
    
    /// <summary>
    /// Removes and returns the last item in the ILisque.
    /// </summary>
    /// <param name="index">At least 0, and less than <see cref="ILisque{T}.Count"/>.</param>
    /// <returns>The item at the given index, which will be removed.</returns>
    /// <exception cref="InvalidOperationException">When the ILisque is empty.</exception>
    T PopAt(int index);
    
    /// <summary>
    /// The first item in the ILisque. Getting or setting this when the ILisque is empty will throw an Exception.
    /// </summary>
    /// <exception cref="InvalidOperationException">When the ILisque is empty.</exception>
    T First { get; set; }
    /// <summary>
    /// The last item in the ILisque. Getting or setting this when the ILisque is empty will throw an Exception.
    /// </summary>
    /// <exception cref="InvalidOperationException">When the ILisque is empty.</exception>
    T Last { get; set; }
}
