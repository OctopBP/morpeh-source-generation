using System;
using System.Text;

namespace MorpehAttributes.Common; 

public class CodeBuilder
{
    public enum IdentChange
    {
        None,
        IncreaseBefore,
        DecreaseBefore,
        IncreaseAfter,
        DecreaseAfter
    }

    private const string IndentSymbol = "\t";

    private readonly StringBuilder _stringBuilder = new();
    private int _indent;

    public void Append(string text) => _stringBuilder.Append(text);
    public void AppendEmptyLine() => _stringBuilder.AppendLine();
    
    public void AppendOpenBraces() => AppendLine("{", IdentChange.IncreaseAfter);
    public void AppendCloseBraces() => AppendLine("}", IdentChange.DecreaseBefore);

    public void AppendLine(string text, IdentChange identChange = IdentChange.None)
    {
        TryToUpdateIdentBefore(identChange);
        _stringBuilder.AppendLine(IdentToSpaces() + text);
        TryToUpdateIdentAfter(identChange);
    }

    private void TryToUpdateIdentAfter(IdentChange identChange)
    {
        switch (identChange)
        {
            case IdentChange.None:
            case IdentChange.IncreaseBefore:
            case IdentChange.DecreaseBefore:
                break;
            case IdentChange.IncreaseAfter:
                _indent++; break;
            case IdentChange.DecreaseAfter:
                _indent--; break;
            default: throw new ArgumentOutOfRangeException(nameof(identChange), identChange, null);
        }
    }

    private void TryToUpdateIdentBefore(IdentChange identChange)
    {
        switch (identChange) {
            case IdentChange.None:
            case IdentChange.IncreaseAfter:
            case IdentChange.DecreaseAfter:
                break;
            case IdentChange.IncreaseBefore:
                _indent++; break;
            case IdentChange.DecreaseBefore:
                _indent--; break;
            default: throw new ArgumentOutOfRangeException(nameof(identChange), identChange, null);
        }
    }

    private string IdentToSpaces()
    {
        if (_indent <= 0) return string.Empty;
        var textAsSpan = IndentSymbol.AsSpan();
        var span = new Span<char>(new char[textAsSpan.Length * _indent]);
        for (var idx = 0; idx < _indent; idx++)
        {
            textAsSpan.CopyTo(span.Slice(idx * textAsSpan.Length, textAsSpan.Length));
        }

        return span.ToString();
    }

    public override string ToString()
    {
        return _stringBuilder.ToString();
    }
}