using HW21_Roslyn.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;

namespace HW21_Roslyn
{
    [Generator]
    public class GenerateSerializerSourceGenerator : IIncrementalGenerator
    {
        private readonly List<string> _attrNames = new List<string>()
        {
            "GenerateBinarySerializer",
            "GenerateBinarySerializerAttribute"
        };

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => IsSyntaxTarget(node),
                    transform: (context, _) => GetSemanticTarget(context))
                .Where(static c => c != null)
                .Collect();

            context.RegisterSourceOutput(provider, Execute);
        }

        private void Execute(SourceProductionContext context, ImmutableArray<ClassInfo?> classInfos)
        {
            foreach (var classInfo in classInfos)
            {
                if (classInfo == null)
                    continue;

                var sourceCode = GenerateSourceCode(classInfo);
                context.AddSource($"{classInfo.ClassName}.g.cs", sourceCode);
            }
        }

        private bool IsSyntaxTarget(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax classDecl && classDecl.AttributeLists.Any();
        }
        private ClassInfo? GetSemanticTarget(GeneratorSyntaxContext context)
        {
            var cds = (ClassDeclarationSyntax)context.Node;
            foreach (var attrs in cds.AttributeLists)
            {
                foreach (var attr in attrs.Attributes)
                {
                    if (_attrNames.Contains(attr.Name.ToString()))
                    {
                        var model = context.SemanticModel;
                        var classSymbol = model.GetDeclaredSymbol(cds);

                        if (classSymbol == null)
                            return null;

                        var properties = classSymbol.GetMembers()
                            .OfType<IPropertySymbol>()
                            .Where(p => p.DeclaredAccessibility == Accessibility.Public)
                            .ToList();

                        if (!properties.Any())
                            return null;

                        return new ClassInfo(
                            classSymbol.Name,
                            classSymbol.ContainingNamespace.ToDisplayString(),
                            properties.Select(p => new PropertyInfo(p.Name, p.Type.SpecialType, p.Type.ToDisplayString(), GetTypeSize(p.Type))).ToList());
                    }
                }
            }

            return null;
        }
        private int? GetTypeSize(ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Int32: return 4;
                case SpecialType.System_String: return null;
                case SpecialType.System_DateTime: return 8;
                default: return null;
            }
        }
        private string GenerateSourceCode(ClassInfo classInfo)
        {
            var code = new StringBuilder();

            code.AppendLine(@$"using System;");
            code.AppendLine(@$"using System.IO;");
            code.AppendLine(@$"using System.Text;").AppendLine();


            code.AppendLine(@$"namespace {classInfo.Namespace}");
            code.AppendLine("{");

            code.AppendLine($@"public partial class {classInfo.ClassName}");
            code.AppendLine("{");

            GenerateSerialize(classInfo, code);
            GenerateDeserialize(classInfo, code);
            
            code.AppendLine("}");

            code.AppendLine("}");


            return code.ToString();
        }

        private void GenerateSerialize(ClassInfo classInfo, StringBuilder code)
        {
            code.AppendLine($@"public void SerializeToBinary(Stream stream)");
            code.AppendLine("{");

            code.AppendLine($@"using(var writer = new BinaryWriter(stream, Encoding.UTF8, true))");
            code.AppendLine("{");

            foreach (var prop in classInfo.Properties)
            {
                switch (prop.SpecialType)
                {
                    case SpecialType.System_Int32:
                        code.AppendLine($"writer.Write((int){prop.Size});");
                        code.AppendLine($"writer.Write(this.{prop.Name});");
                        break;
                    case SpecialType.System_String:
                        code.AppendLine($@"if (this.{prop.Name}?.Length > 0)");
                        code.AppendLine("{");
                        code.AppendLine($@"var bytes = Encoding.UTF8.GetBytes(this.{prop.Name});");
                        code.AppendLine($@"writer.Write(bytes.Length);");
                        code.AppendLine($@"writer.Write(bytes);");
                        code.AppendLine("}");
                        code.AppendLine("else");
                        code.AppendLine("{");
                        code.AppendLine("writer.Write((int)0);");
                        code.AppendLine("}");
                        break;
                    case SpecialType.System_DateTime:
                        code.AppendLine($"writer.Write((int){prop.Size});");
                        code.AppendLine($"writer.Write(this.{prop.Name}.Ticks);");
                        break;
                }
            }

            code.AppendLine("}");

            code.AppendLine("}");
        }
        private void GenerateDeserialize(ClassInfo classInfo, StringBuilder code)
        {
            code.AppendLine($@"public static {classInfo.ClassName}? DeserializeData(ReadOnlySpan<byte> byteSpan)");
            code.AppendLine("{");

            code.AppendLine($@"var result = new {classInfo.ClassName}();");
            code.AppendLine("if (byteSpan.Length == 0) return null;");
            code.AppendLine("var index = 0;");
            code.AppendLine("var count = 0;");

            for (int i = 0; i < classInfo.Properties.Count; i++)
            {
                var prop = classInfo.Properties[i];

                code.AppendLine("if (index + 4 > byteSpan.Length) return result;");
                code.AppendLine("count = BitConverter.ToInt32(byteSpan.Slice(index, 4));");
                code.AppendLine("index = index + 4;");

                code.AppendLine("if (index + count > byteSpan.Length) return result;");

                switch (prop.SpecialType)
                {
                    case SpecialType.System_Int32:
                        code.AppendLine($@"result.{prop.Name} = BitConverter.ToInt32(byteSpan.Slice(index, count));");
                        break;
                    case SpecialType.System_String:
                        code.AppendLine($@"result.{prop.Name} = Encoding.UTF8.GetString(byteSpan.Slice(index, count));");
                        break;
                    case SpecialType.System_DateTime:
                        code.AppendLine($@"result.{prop.Name} = DateTime.FromBinary(BitConverter.ToInt64(byteSpan.Slice(index, count)));");
                        break;
                }

                if (i + 1 != classInfo.Properties.Count)
                    code.AppendLine("index = index + count;");
            }

            code.AppendLine("return result;");

            code.AppendLine("}");
        }
    }
}
