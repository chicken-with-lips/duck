using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Duck.SerializationGenerators;

[Generator]
public class SerializationFactoryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var enumTypes = context.SyntaxProvider
            .CreateSyntaxProvider(IsOfInterestToSyntaxProvider, TransformTypeForSyntaxProvider)
            .Where(type => type is not null)
            .Collect();

        context.RegisterSourceOutput(enumTypes, Generate);
    }

    private static void Generate(SourceProductionContext context, ImmutableArray<ITypeSymbol?> types)
    {
        if (types.IsDefaultOrEmpty) {
            return;
        }

        context.CancellationToken.ThrowIfCancellationRequested();

        var fileName = $"SerializationFactory.Generated.cs";
        var code = GenerateCode(context, types);

        context.AddSource(fileName, CSharpSyntaxTree.ParseText(code).GetRoot().NormalizeWhitespace().ToFullString());
    }

    private static string GenerateCode(SourceProductionContext context, ImmutableArray<ITypeSymbol?> types)
    {
        var supportsBuilder = new StringBuilder();
        var serializeBuilder = new StringBuilder();
        var deserializeBuilder = new StringBuilder();
        var deserializeT1Builder = new StringBuilder();

        foreach (var type in types.Distinct(SymbolEqualityComparer.Default)
                     .Cast<INamedTypeSymbol>()
                     .Where(type => type != null)) {
            context.CancellationToken.ThrowIfCancellationRequested();


            var fqdn = Util.MakeFullyQualifiedTypeName(type, true);
            var fqdnWithoutGenerics = Util.MakeFullyQualifiedTypeName(type, true, false);

            supportsBuilder.AppendLine($"""case "{fqdn}":""");

            

            if (type.IsGenericType) {
                // deserializeBuilder.AppendLine($@"""{fqdn}"" => {type.ContainingNamespace}.{type.ConstructUnboundGenericType().Name}Serializer.Deserialize(deserializer, context),");
                // serializeBuilder.AppendLine($@"case ""{fqdn}"": {type.ContainingNamespace}.{type.ConstructUnboundGenericType().Name}Serializer.Serialize(({fqdn}) value, graphSerializer, context); break;");
            } else {
                deserializeBuilder.AppendLine($@"""{fqdn}"" => {fqdn}Serializer.Deserialize(deserializer, context),");
                serializeBuilder.AppendLine($@"case ""{fqdn}"": {fqdn}Serializer.Serialize(({fqdn}) value, graphSerializer, context); break;");
            }
        }

        return
            $$"""
              // <auto-generated />

              using Duck.Serialization;
              using Duck.Serialization.Exception;

              namespace Generated.Serializer;

              public class SerializationFactory : ISerializationFactory
              {
                  public bool Supports(string typeName)
                  {
                      switch(typeName) {
                        {{supportsBuilder}}
                              return true;
                      }
              
                      return false;
                  }
                  
                  public void Serialize(object value, IGraphSerializer graphSerializer, ISerializationContext context)
                  {
                      switch(value.GetType().FullName) {
                        {{serializeBuilder}}
                          default: throw new System.NotImplementedException();
                      };
                  }
              
                  public object Deserialize(string typeName, IDeserializer deserializer, IDeserializationContext context)
                  {
                      return typeName switch {
                        {{deserializeBuilder}}
                          _ => throw new System.NotImplementedException(),
                      };
                  }
              }
              """;
    }

    private static ITypeSymbol? TransformTypeForSyntaxProvider(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;

        if (attributeSyntax.Parent?.Parent is not ClassDeclarationSyntax
            && attributeSyntax.Parent?.Parent is not StructDeclarationSyntax
            && attributeSyntax.Parent?.Parent is not InterfaceDeclarationSyntax) {
            return null;
        }

        return context.SemanticModel.GetDeclaredSymbol(attributeSyntax.Parent.Parent) as ITypeSymbol;
    }

    private static bool IsOfInterestToSyntaxProvider(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        return Util.IsMarkedAsAutoSerializable(syntaxNode);
    }
}
