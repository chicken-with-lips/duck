﻿using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Duck.SerializationGenerators;

[Generator]
public class SerializerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var enumTypes = context.SyntaxProvider
            .CreateSyntaxProvider(IsInterestingEnum, ResolveTypeOrNull)
            .Where(type => type is not null)
            .Collect();

        context.RegisterSourceOutput(enumTypes, GenerateCode);
    }

    private static void GenerateCode(SourceProductionContext context, ImmutableArray<ITypeSymbol?> enumerations)
    {
        if (enumerations.IsDefaultOrEmpty) {
            return;
        }

        foreach (var type in enumerations.Distinct(SymbolEqualityComparer.Default)
                     .Cast<INamedTypeSymbol>()
                     .Where(type => type is not null)) {
            context.CancellationToken.ThrowIfCancellationRequested();

            var code = GenerateCode(type);
            var typeNamespace = type.ContainingNamespace.IsGlobalNamespace
                ? null
                : $"{type.ContainingNamespace}.";

            context.AddSource($"{typeNamespace}{type.Name}.Generated.cs", code);
        }
    }

    private static bool IsSerializable(IFieldSymbol symbol)
    {
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
            if (data.AttributeClass?.Name is "AutoSerializable" or "AutoSerializableAttribute") {
                return true;
            }
        }

        foreach (var typeSymbol in symbol.Type.AllInterfaces) {
            if (typeSymbol.Name == "ISerializable" && typeSymbol.ContainingNamespace.Name == "Duck.Serialization") {
                return true;
            }
        }

        return false;
    }

    private static bool IsSupportedContainerType(ITypeSymbol? symbol)
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

    private static string GenerateCode(INamedTypeSymbol type)
    {
        var ns = type.ContainingNamespace.IsGlobalNamespace ? null : type.ContainingNamespace.ToString();
        var name = type.Name;
        var typeIdentifier = type.TypeKind switch {
            TypeKind.Class => "class",
            TypeKind.Struct => "struct",
            TypeKind.Interface => "interface",
            _ => null
        };

        if (!type.TypeParameters.IsEmpty) {
            var typeBuilder = new List<string>();

            foreach (var typeParameterSymbol in type.TypeParameters) {
                typeBuilder.Add(typeParameterSymbol.Name);
            }

            if (name == "ComponentPool") {
                name += "<" + String.Join(", ", typeBuilder) + "> : ISerializable, IComponentPool<T> where T : struct";
            } else {
                name += "<" + String.Join(", ", typeBuilder) + "> : ISerializable";
            }
        } else {
            name += " : ISerializable";
        }

        if (type.TypeKind == TypeKind.Interface) {
            return @$"// <auto-generated />

using Duck.Serialization;

{(ns is null ? null : $@"namespace {ns};")}

public partial {typeIdentifier} {name}
{{
}}
";
        }

        var hasSerializeBeenImplemented = false;
        var hasDeserializeBeenImplemented = false;

        foreach (var member in type.GetMembers()) {
            // is serialize already implemented?
            if (member.Kind == SymbolKind.Method && !member.IsAbstract) {
                if (member.Name == "Serialize") {
                    hasSerializeBeenImplemented = true;
                } else if (member.Name == "Deserialize") {
                    hasDeserializeBeenImplemented = true;
                }
            }
        }

        // nothing to do here
        if (hasSerializeBeenImplemented && hasDeserializeBeenImplemented) {
            return string.Empty;
        }

        var serialize = new StringBuilder();
        var deserialize = new StringBuilder();

        foreach (var member in type.GetMembers()) {
            if (member is not IFieldSymbol fieldSymbol) {
                continue;
            }

            if (fieldSymbol.Kind != SymbolKind.Field || !IsSerializable(fieldSymbol)) {
                continue;
            }

            var symbolName = fieldSymbol.AssociatedSymbol?.Name ?? fieldSymbol.Name;
            var fieldTypeSymbol = fieldSymbol.Type as INamedTypeSymbol;

            if (fieldTypeSymbol == null) {
                continue;
            }

            if (!hasSerializeBeenImplemented) {
                if (IsSupportedContainerType(fieldSymbol.Type)) {
                    serialize.AppendLine($@"        serializer.Write(""{symbolName}"", {symbolName}.ToArray(), ""{fieldSymbol.Type.ContainingNamespace}.{fieldSymbol.Type.Name}"");");
                } else {
                    serialize.AppendLine($@"        serializer.Write(""{symbolName}"", {symbolName});");
                }
            }

            if (!hasDeserializeBeenImplemented) {
                if (fieldSymbol.Type.IsValueType) {
                    deserialize.AppendLine($@"                case ""{symbolName}"": {symbolName} = deserializer.Read{fieldSymbol.Type.Name}(entry.OffsetStart); break;");
                } else if (IsSupportedContainerType(fieldSymbol.Type)) {
                    deserialize.AppendLine($@"                case ""{symbolName}"":
                     {symbolName} = deserializer.ReadObjectList<{fieldSymbol.Type.ToDisplayString()}, {fieldTypeSymbol?.TypeArguments[0].ToDisplayString()}>(
                         (objectDeserializer, objectContext, entry) => Instanciator.Create<{fieldTypeSymbol?.TypeArguments[0].Name}>({(fieldTypeSymbol.TypeArguments[0].IsAbstract ? "entry.ExplicitType" : $@"""{fieldSymbol.Type.ToDisplayString()}""")}, objectDeserializer, objectContext),
                         ""{fieldSymbol.Type.ContainingNamespace}.{fieldSymbol.Type.Name}"",
                         entry.OffsetStart
                     );
                     break;");
                } else {
                    var typeList = new List<string>();
                    typeList.Add(fieldSymbol.Type.ToDisplayString());

                    if (fieldTypeSymbol.IsGenericType) {
                        typeList.Add(Helper.MakeGenericArgumentString(fieldTypeSymbol));
                    }

                    deserialize.AppendLine($@"                case ""{symbolName}"":
                    {symbolName} = deserializer.ReadObjectReference<{fieldSymbol.Type.ToDisplayString()}>(
                        (objectDeserializer, objectContext, objectEntry) => Instanciator.Create<{String.Join(",", typeList)}>({(fieldSymbol.Type.IsAbstract ? "objectEntry.ExplicitType" : $@"""{fieldSymbol.Type.ToDisplayString()}""")}, objectDeserializer, objectContext),
                        entry
                    );
                    break;");
                }
            }
        }

        deserialize.Clear();
        serialize.Clear();

        return @$"// <auto-generated />

using Duck.Serialization;
using Duck.Serialization.Exception;

{(ns is null ? null : $@"namespace {ns};")}

public partial {typeIdentifier} {name}
{{
    {(hasSerializeBeenImplemented ? null : $@"public void Serialize(IGraphSerializer serializer, ISerializationContext context)
    {{
{serialize}
    }}")}

    {(hasDeserializeBeenImplemented ? null : $@"public {type.Name}(IDeserializer deserializer, IDeserializationContext context)
    {{{(type.IsValueType ? null : $@"
        if (context.ObjectId != null) {{ 
            context.AddObject(context.ObjectId.Value, this);
        }}")}

        foreach (var entry in deserializer.Index) {{
            switch(entry.Name) {{
{deserialize}
            }}
        }}
    }}")}
}}
";
    }

    private static ITypeSymbol? ResolveTypeOrNull(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;

        if (attributeSyntax.Parent?.Parent is not ClassDeclarationSyntax
            && attributeSyntax.Parent?.Parent is not StructDeclarationSyntax
            && attributeSyntax.Parent?.Parent is not InterfaceDeclarationSyntax) {
            return null;
        }

        return context.SemanticModel.GetDeclaredSymbol(attributeSyntax.Parent?.Parent) as ITypeSymbol;
    }

    private static bool IsInterestingEnum(
        SyntaxNode syntaxNode,
        CancellationToken cancellationToken)
    {
        if (syntaxNode is not AttributeSyntax attribute)
            return false;

        var name = ExtractName(attribute.Name);

        return name is "AutoSerializable" or "AutoSerializableAttribute";
    }

    private static string? ExtractName(NameSyntax? name)
    {
        return name switch {
            SimpleNameSyntax ins => ins.Identifier.Text,
            QualifiedNameSyntax qns => qns.Right.Identifier.Text,
            _ => null
        };
    }
}
