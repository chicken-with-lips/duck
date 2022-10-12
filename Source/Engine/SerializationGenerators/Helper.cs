using Microsoft.CodeAnalysis;

namespace Duck.SourceGenerators.Serialization;

public static class Helper
{
    public static string MakeGenericParameterString(ITypeSymbol symbol)
    {
        if (symbol is INamedTypeSymbol namedSymbol && namedSymbol.IsGenericType) {
            var genericParameters = new List<string>();

            foreach (var typeSymbol in namedSymbol.TypeParameters) {
                genericParameters.Add(typeSymbol.Name);
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
                genericParameters.Add(typeSymbol.Name);
            }

            return String.Join(", ", genericParameters);
        }

        return string.Empty;
    }

    public static string MakeTypeName(ITypeSymbol symbol, bool useGenericParametersInsteadOfArguments = true)
    {
        if (symbol is INamedTypeSymbol namedSymbol && namedSymbol.IsGenericType) {
            var genericParameters = useGenericParametersInsteadOfArguments ? MakeGenericParameterString(symbol) : MakeGenericArgumentString(symbol);

            return $"{symbol.Name}<{genericParameters}>";
        }

        return symbol.Name;
    }

    public static string MakeFullyQualifiedTypeName(ITypeSymbol symbol)
    {
        return $"{symbol.ContainingNamespace}.{MakeTypeName(symbol)}";
    }
}
