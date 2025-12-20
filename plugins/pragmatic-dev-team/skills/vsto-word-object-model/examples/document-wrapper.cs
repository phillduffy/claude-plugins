// Document Range Wrapper Pattern
// Demonstrates: IDisposable for COM, abstraction layer, safe operations

using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Word;
using Range = Microsoft.Office.Interop.Word.Range;

namespace WordAddIn.Interop;

/// <summary>
/// Wraps Word.Range with IDisposable for automatic COM cleanup.
/// Implements domain interface to abstract Word dependency.
/// </summary>
public class WordDocumentRange : IDocumentRange, IDisposable
{
    private Range? _range;
    private bool _disposed;

    public WordDocumentRange(Range range)
    {
        _range = range ?? throw new ArgumentNullException(nameof(range));
    }

    // DOMAIN INTERFACE PROPERTIES
    public int Start => _range?.Start ?? 0;
    public int End => _range?.End ?? 0;
    public string? Text => _range?.Text;

    // EXPOSE UNDERLYING RANGE - for interop layer only
    internal Range Range => _range ?? throw new ObjectDisposedException(nameof(WordDocumentRange));

    // SAFE OPERATIONS - check disposed state
    public void Select()
    {
        ThrowIfDisposed();
        _range!.Select();
    }

    public WordDocumentRange Duplicate()
    {
        ThrowIfDisposed();
        return new WordDocumentRange(_range!.Duplicate);
    }

    public void Collapse(bool toEnd = false)
    {
        ThrowIfDisposed();
        _range!.Collapse(toEnd
            ? WdCollapseDirection.wdCollapseEnd
            : WdCollapseDirection.wdCollapseStart);
    }

    public void MoveEnd(int count)
    {
        ThrowIfDisposed();
        _range!.MoveEnd(WdUnits.wdCharacter, count);
    }

    // DISPOSE PATTERN
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (_range is not null)
        {
            try
            {
                Marshal.ReleaseComObject(_range);
            }
            catch
            {
                // Swallow - COM object may already be released
            }
            _range = null;
        }

        _disposed = true;
    }

    ~WordDocumentRange()
    {
        Dispose(false);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WordDocumentRange));
    }
}

/// <summary>
/// Domain interface - no Word dependency.
/// </summary>
public interface IDocumentRange
{
    int Start { get; }
    int End { get; }
    string? Text { get; }
}

// USAGE PATTERN
// using var range = new WordDocumentRange(document.Range(0, 100));
// var text = range.Text;
// // Automatically released when scope ends

// KEY PATTERNS:
// 1. IDisposable wraps COM objects
// 2. Internal Range property - only interop layer accesses
// 3. Domain interface (IDocumentRange) hides Word dependency
// 4. Null check after disposal
// 5. Finalizer as safety net (but prefer using statement)
// 6. Swallow exceptions in Dispose - COM may be dead
// 7. ThrowIfDisposed for safety
