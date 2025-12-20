using System;
using System.Runtime.InteropServices;
using Range = Microsoft.Office.Interop.Word.Range;

namespace YourNamespace.Interop;

/// <summary>
/// Disposable wrapper for Word Range COM object.
/// Implements IDisposable for deterministic COM cleanup.
/// </summary>
/// <example>
/// using var range = new WordDocumentRange(doc.Range());
/// range.Range.Text = "Hello World";
/// // Range is released when disposed
/// </example>
public class WordDocumentRange : IDisposable
{
    private bool _disposed;

    public Range Range { get; }

    public WordDocumentRange(Range range)
    {
        Range = range ?? throw new ArgumentNullException(nameof(range));
    }

    public float Start => Range.Start;
    public float End => Range.End;
    public string Text => Range.Text;

    public void Dispose()
    {
        if (_disposed)
            return;

        Marshal.ReleaseComObject(Range);
        _disposed = true;
    }
}

/// <summary>
/// Alternative pattern: Generic COM wrapper for any Office object.
/// </summary>
public class ComWrapper<T> : IDisposable where T : class
{
    private bool _disposed;

    public T Object { get; }

    public ComWrapper(T comObject)
    {
        Object = comObject ?? throw new ArgumentNullException(nameof(comObject));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Marshal.ReleaseComObject(Object);
        _disposed = true;
    }
}

// Usage:
// using var paragraphs = new ComWrapper<Paragraphs>(doc.Paragraphs);
// for (int i = 1; i <= paragraphs.Object.Count; i++) { ... }
