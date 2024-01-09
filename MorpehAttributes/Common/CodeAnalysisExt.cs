#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MorpehAttributes.Common;

/// <summary>
/// https://stackoverflow.com/questions/28947456/how-do-i-get-attributedata-from-attributesyntax-in-roslyn
/// https://stackoverflow.com/questions/67539903/converting-attributedata-into-a-known-attribute-type-roslyn
/// </summary>
public static class CodeAnalysisExt
{
    public static IReadOnlyList<AttributeData> GetAttributes(this AttributeListSyntax attributes, Compilation compilation)
    {
        var acceptedTrees = new HashSet<SyntaxTree>();
        foreach (var attribute in attributes.Attributes)
        {
            acceptedTrees.Add(attribute.SyntaxTree);
        }

        var parentSymbol = attributes.Parent!.GetDeclaredSymbol(compilation)!;
        var parentAttributes = parentSymbol.GetAttributes();
        var ret = new List<AttributeData>();
        
        foreach (var attribute in parentAttributes)
        {
            if (acceptedTrees.Contains(attribute.ApplicationSyntaxReference!.SyntaxTree))
                ret.Add(attribute);
        }

        return ret;
    }
    
    public static AttributeData? GetAttribute(this AttributeSyntax attribute, Compilation compilation)
    {
        var parentSymbol = attribute.Parent!.GetDeclaredSymbol(compilation)!;
        var parentAttributes = parentSymbol.GetAttributes();
        
        foreach (var attributeData in parentAttributes)
        {
            if (attribute.SyntaxTree == attributeData.ApplicationSyntaxReference!.SyntaxTree)
                return attributeData;
        }
        
        return null;
    }

    public static ISymbol? GetDeclaredSymbol(this SyntaxNode node, Compilation compilation)
    {
        var model = compilation.GetSemanticModel(node.SyntaxTree);
        return model.GetDeclaredSymbol(node);
    }
    
    
    
    
    
    
    public static T MapToType<T>(this AttributeData attributeData) where T : Attribute
    {
        T attribute;
        if (attributeData.AttributeConstructor != null && attributeData.ConstructorArguments.Length > 0)
        {
            attribute = (T) Activator.CreateInstance(typeof(T), attributeData.GetActualConstuctorParams().ToArray());
        }
        else
        {
            attribute = (T) Activator.CreateInstance(typeof(T));
        }
        foreach (var p in attributeData.NamedArguments)
        {
            typeof(T).GetField(p.Key).SetValue(attribute, p.Value.Value);
        }
        return attribute;
    }

    public static IEnumerable<object> GetActualConstuctorParams(this AttributeData attributeData)
    {
        foreach (var arg in attributeData.ConstructorArguments)
        {
            if (arg.Kind == TypedConstantKind.Array)
            {
                // Assume they are strings, but the array that we get from this
                // should actually be of type of the objects within it, be it strings or ints
                // This is definitely possible with reflection, I just don't know how exactly. 
                yield return arg.Values.Select(a => a.Value).OfType<string>().ToArray();
            }
            else
            {
                yield return arg.Value;
            }
        }
    }
    
    public static bool TryGetAttributeData(this ISymbol symbol, ISymbol attributeType, out AttributeData attributeData)
    {
        foreach (var a in symbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType))
            {
                attributeData = a;
                return true;
            }
        }
        attributeData = default;
        return false;
    }

    public struct AttributeSymbolWrapper<T>
    {
        public INamedTypeSymbol Symbol;

        private static INamedTypeSymbol GetKnownSymbol(Compilation compilation, Type t)
        {
            return (INamedTypeSymbol) compilation.GetTypeByMetadataName(t.FullName);
        }

        public void Init(Compilation compilation)
        {
            Symbol = GetKnownSymbol(compilation, typeof(T));
        }
    }

    public static bool TryGetAttribute<T>(this ISymbol symbol, AttributeSymbolWrapper<T> attributeSymbolWrapper, out T attribute) where T : Attribute
    {
        if (TryGetAttributeData(symbol, attributeSymbolWrapper.Symbol, out var attributeData))
        {
            attribute = attributeData.MapToType<T>();
            return true;
        }
        attribute = default;
        return false;
    }

    public static bool HasAttribute(this ISymbol symbol, ISymbol attributeType)
    {
        foreach (var a in symbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType))
            {
                return true;
            }
        }
        return false;
    }
}
