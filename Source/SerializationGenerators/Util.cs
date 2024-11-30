using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Duck.SerializationGenerators;

public static class Util
{
    public static string MakeGenericParameterString(ITypeSymbol symbol, bool withContainer = false)
    {
        if (symbol is INamedTypeSymbol namedSymbol && namedSymbol.IsGenericType) {
            var genericParameters = new List<string>();

            foreach (var typeSymbol in namedSymbol.TypeParameters) {
                genericParameters.Add(typeSymbol.Name);
            }

            if (withContainer) {
                return $"<{string.Join(", ", genericParameters)}>";
            }

            return String.Join(", ", genericParameters);
        }

        return string.Empty;
    }

    public static string MakeGenericArgumentString(ITypeSymbol symbol)
    {
        if (symbol is INamedTypeSymbol namedSymbol && namedSymbol.IsGenericType) {
            var genericParameters = new List<string>();

            foreach (var typeSymbol in namedSymbol.TypeArguments) {
                genericParameters.Add(typeSymbol.ToDisplayString());
            }

            return String.Join(", ", genericParameters);
        }

        return string.Empty;
    }

    public static string MakeGenericConstraintString(ITypeSymbol symbol, bool withWhere = false)
    {
        if (symbol is INamedTypeSymbol namedSymbol && namedSymbol.IsGenericType) {
            var constraints = new List<string>();

            foreach (var typeSymbol in namedSymbol.TypeParameters) {
                var constraint = new List<string>();

                if (typeSymbol.IsUnmanagedType) {
                    constraint.Add("unmanaged");
                }

                foreach (var constraintType in typeSymbol.ConstraintTypes) {
                    constraint.Add(constraintType.ToDisplayString());
                }

                if (constraint.Count == 0) {
                    continue;
                }

                constraints.Add($"{typeSymbol.Name} : {string.Join(", ", constraint)}");
            }

            if (withWhere) {
                return $"where {string.Join(", ", constraints)}";
            }

            return String.Join(", ", constraints);
        }

        return string.Empty;
    }

    public static string MakeTypeName(ITypeSymbol symbol, bool useGenericParametersInsteadOfArguments = true, bool includeGenerics = true)
    {
        if (includeGenerics && symbol is INamedTypeSymbol namedSymbol && namedSymbol.IsGenericType) {
            var genericParameters = useGenericParametersInsteadOfArguments ? MakeGenericParameterString(symbol) : MakeGenericArgumentString(symbol);

            return $"{symbol.Name}<{genericParameters}>";
        }

        return symbol.Name;
    }

    public static string MakeFullyQualifiedTypeName(ITypeSymbol symbol, bool useGenericParametersInsteadOfArguments = true, bool includeGenerics = true)
    {
        return $"{symbol.ContainingNamespace}.{MakeTypeName(symbol, useGenericParametersInsteadOfArguments, includeGenerics)}";
    }

    public static bool IsAutoSerializableAttributeName(string name)
    {
        return name is "DuckSerializable" or "DuckSerializableAttribute";
    }

    public static bool IsMarkedAsAutoSerializable(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not AttributeSyntax attribute) {
            return false;
        }

        var name = attribute.Name switch {
            SimpleNameSyntax syntax => syntax.Identifier.Text,
            QualifiedNameSyntax syntax => syntax.Right.Identifier.Text,
            _ => null
        };

        return IsAutoSerializableAttributeName(name ?? string.Empty);
    }

    public static bool IsFieldSerializable(IFieldSymbol symbol)
    {
        if (symbol.DeclaredAccessibility != Accessibility.Public) {
            return false;
        }

        if (symbol.Type.IsValueType) {
            return true;
        }

        if (symbol.Type is IArrayTypeSymbol) {
            return true;
        }

        if (symbol.Type is INamedTypeSymbol && IsSupportedContainerType(symbol.Type)) {
            return true;
        }

        if (symbol.AssociatedSymbol != null) {
            return false;
        }

        foreach (var data in symbol.Type.GetAttributes()) {
            if (IsAutoSerializableAttributeName(data.AttributeClass?.Name ?? "")) {
                return true;
            }
        }

        return false;
    }

    public static bool IsSupportedContainerType(ITypeSymbol? symbol)
    {
        if (null == symbol) {
            return false;
        }

        if (symbol.ContainingNamespace.ToString() != "System.Collections.Generic") {
            return false;
        }

        return symbol.Name switch {
            "List" => true,
            _ => false,
        };
    }
}
