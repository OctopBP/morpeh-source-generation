using System;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Morpeh.SourceGeneration.Common;

public class CodeBuilder
{
    public class BracketsBlock : IDisposable
    {
        private readonly CodeBuilder _builder;

        public BracketsBlock(CodeBuilder builder)
        {
            _builder = builder;
            _builder.OpenBrackets();
        }

        public void Dispose()
        {
            _builder.CloseBrackets();
        }
    }
    
    public class NamespaceBlock : IDisposable
    {
        private readonly CodeBuilder _builder;
        private readonly string _namespace;

        public NamespaceBlock(CodeBuilder builder, ITypeSymbol typeSymbol)
        {
            _builder = builder;
            _namespace = typeSymbol.GetNamespace();
            if (!string.IsNullOrEmpty(_namespace))
            {
                builder.AppendIdent().Append("namespace ").Append(_namespace).AppendLine();
                _builder.OpenBrackets();
            }
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(_namespace))
            {
                _builder.CloseBrackets();
            }
        }
    }
    
    private readonly StringBuilder _sb = new StringBuilder();
    private int _ident = 0;

    public CodeBuilder AppendLine()
    {
        _sb.AppendLine();
        return this;
    }
    
    public CodeBuilder AppendLine(string value)
    {
        _sb.AppendLine(value);
        return this;
    }
    
    public CodeBuilder AppendLineWithIdent(string value)
    {
        return AppendIdent().AppendLine(value);
    }
    
    public CodeBuilder Append(string value)
    {
        _sb.Append(value);
        return this;
    }
    
    public CodeBuilder Append(char value)
    {
        _sb.Append(value);
        return this;
    }

    public CodeBuilder AppendIdent()
    {
        for (var i = 0; i < _ident; i++)
        {
            _sb.Append("    ");
        }
        return this;
    }

    public CodeBuilder OpenBrackets()
    {
        return AppendLineWithIdent("{").IncreaseIdent();
    }
    
    public CodeBuilder CloseBrackets()
    {
        return DecreaseIdent().AppendLineWithIdent("}");
    }
    
    public CodeBuilder IncreaseIdent()
    {
        _ident++;
        return this;
    }
    
    public CodeBuilder DecreaseIdent()
    {
        if (_ident > 0)
        {
            _ident--;
        }
        return this;
    }

    public override string ToString()
    {
        return _sb.ToString();
    }
}