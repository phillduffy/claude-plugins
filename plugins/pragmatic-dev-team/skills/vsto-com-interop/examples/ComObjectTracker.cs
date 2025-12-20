using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace YourNamespace.Interop;

/// <summary>
/// Tracks COM objects for safe lifecycle management.
/// Uses LIFO (stack) release order to ensure proper cleanup of dependent objects.
/// </summary>
/// <example>
/// using var tracker = new ComObjectTracker();
/// var paragraphs = tracker.Track(doc.Paragraphs);
/// var para = tracker.Track(paragraphs[1]);
/// var range = tracker.Track(para.Range);
/// range.Text = "Hello";
/// // All released in reverse order on dispose
/// </example>
public class ComObjectTracker : IDisposable
{
    private readonly Stack<object> _tracked = new();
    private bool _disposed;

    /// <summary>
    /// Tracks a COM object for later release. Returns the same object for fluent usage.
    /// </summary>
    /// <typeparam name="T">The COM object type.</typeparam>
    /// <param name="comObject">The COM object to track.</param>
    /// <returns>The same object for fluent chaining.</returns>
    public T Track<T>(T comObject) where T : class
    {
        if (comObject == null)
            throw new ArgumentNullException(nameof(comObject));

        _tracked.Push(comObject);
        return comObject;
    }

    /// <summary>
    /// Releases all tracked COM objects in LIFO order.
    /// Safe to call multiple times.
    /// </summary>
    public void ReleaseAll()
    {
        while (_tracked.Count > 0)
        {
            var obj = _tracked.Pop();
            try
            {
                if (obj != null)
                {
                    Marshal.ReleaseComObject(obj);
                }
            }
            catch (InvalidComObjectException)
            {
                // Object was already released - safe to ignore
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        ReleaseAll();
        _disposed = true;
    }
}
