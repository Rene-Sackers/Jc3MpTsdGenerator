using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jc3MpTsdGenerator.Models;
using Jc3MpTsdGenerator.TypeCasters;

namespace Jc3MpTsdGenerator
{
    public class DefinitionsGenerator
    {
        private const string ClientDefinitionFilePath = "client.d.ts";
        private const string ServerDefinitionFilePath = "server.d.ts";

        private static readonly List<ITypeCaster> TypeCasters = new List<ITypeCaster>
        {
            new OrTypeCaster(),
            new ArrayTypeCaster(),
            new PlayerWeapnTypeCaster(),
            new FunctionToLambdaCaster(),
            new NullBackTickCaster()
        };

        private readonly ApiDefinition _clientDefinition;
        private readonly ApiDefinition _serverDefinition;
        private readonly string _clientDefinitionFilePath;
        private readonly string _serverDefinitionFilePath;

        public DefinitionsGenerator(ApiDefinition clientDefinition, ApiDefinition serverDefinition, string outputDirectory)
        {
            _clientDefinition = clientDefinition;
            _serverDefinition = serverDefinition;

            _clientDefinitionFilePath = Path.Combine(outputDirectory, ClientDefinitionFilePath);
            _serverDefinitionFilePath = Path.Combine(outputDirectory, ServerDefinitionFilePath);
        }
        
        public void Generate()
        {
            using (var clientDefinitionFileStream = File.Create(_clientDefinitionFilePath))
            using (var clientDefinitionStreamWriter = new StreamWriter(clientDefinitionFileStream))
            using (var definitionWriterState = new DefinitionWriterState(clientDefinitionStreamWriter))
                GenerateForDefinition(_clientDefinition, definitionWriterState);

            using (var serverDefinitionFileStream = File.Create(_serverDefinitionFilePath))
            using (var serverDefinitionStreamWriter = new StreamWriter(serverDefinitionFileStream))
            using (var definitionWriterState = new DefinitionWriterState(serverDefinitionStreamWriter))
                GenerateForDefinition(_serverDefinition, definitionWriterState);
        }

        private static void GenerateForDefinition(ApiDefinition definition, DefinitionWriterState writer)
        {
            definition.Classes.ToList().ForEach(@class => GenerateClassDefinition(@class, writer));
        }

        private static void GenerateClassDefinition(Class @class, DefinitionWriterState writer)
        {
            // Description
            writer.WriteLine("/**");
            WriteCommentBlockContent(@class.Description, writer);
            writer.WriteLine(" */");

            // Declaration
            WriteInterfaceDeclaration(@class, writer);
            writer.IncreaseIndentation();

            // Properties
            @class.Properties.ToList().ForEach(property => WriteClassProperty(property, writer));

            // Constructor
            WriteClassConstructor(@class, writer);

            // Methods
            @class.Functions.ToList().ForEach(method => WriteClassMethod(method, writer));

            // Closing
            writer.DecreaseIndentation();
            writer.WriteLine("}");
            writer.WriteBlankLine();
        }

        private static void WriteClassMethod(Function method, DefinitionWriterState writer)
        {
            writer.WriteLine("/**");
            WriteCommentBlockContent(method.Description, writer);
            WriteParametersCommentBlock(method.Parameters, writer);
            if (!string.IsNullOrWhiteSpace(method.Example))
                WriteCommentBlockContent("@example " + method.Example, writer);
            writer.WriteLine(" */");

            writer.WriteLine($"{method.Name}{RenderMethodParameters(method.Parameters)}: {ProjectType(method.ReturnType)}");
            writer.WriteBlankLine();
        }

        private static void WriteClassConstructor(Class @class, DefinitionWriterState writer)
        {
            if (!@class.HasConstructor) return;
            
            writer.WriteLine($"constructor{RenderMethodParameters(@class.Constructor.Parameters)};");
            writer.WriteBlankLine();
        }

        private static void WriteInterfaceDeclaration(Class @class, DefinitionWriterState writer)
        {
            var interfaceDeclarationBuilder = $"declare interface {@class.ClassName}";
            if (!string.IsNullOrWhiteSpace(@class.InstanceOf))
                interfaceDeclarationBuilder += $" extends {@class.InstanceOf}";

            writer.WriteLine($"{interfaceDeclarationBuilder} {{");
        }

        private static void WriteParametersCommentBlock(ICollection<Parameter> parameters, DefinitionWriterState writer)
        {
            if (parameters == null || !parameters.Any()) return;

            parameters.ToList().ForEach(parameter => WriteParametersCommentBlockParameter(parameter, writer));
            writer.WriteLine(" *");
        }

        private static void WriteParametersCommentBlockParameter(Parameter parameter, DefinitionWriterState writer)
        {
            writer.WriteLine($" * @param {ProjectType(parameter.Type)} {parameter.Name.Replace("...", "")}");
        }


        private static void WriteCommentBlockContent(string comment, DefinitionWriterState writer)
        {
            if (string.IsNullOrWhiteSpace(comment))
                return;

            comment = comment.Replace("\r\n", "\n").Replace("\r", "\n");

            foreach (var line in comment.Split(new[] { "\n" }, StringSplitOptions.None))
                writer.WriteLine($" * {line}");
        }

        private static string ProjectTypeAsArray(string returnType)
        {
            returnType = ProjectType(returnType);

            return $"Array<{returnType}>";
        }

        private static string ProjectType(string returnType)
        {
            if (string.IsNullOrWhiteSpace(returnType))
                return "any";

            TypeCasters.ForEach(typeCaster => returnType = typeCaster.Execute(returnType));

            return returnType;
        }

        private static void WriteClassProperty(Property property, DefinitionWriterState writer)
        {
            writer.WriteLine("/**");
            WriteCommentBlockContent(property.Description, writer);
            writer.WriteLine(" */");

            var propertyBuilder = string.Empty;

            if (property.ReadOnly)
                propertyBuilder += "readonly ";

            propertyBuilder += property.Name;

            if (property.HasParameters == true && property.Parameters != null)
                propertyBuilder += RenderMethodParameters(property.Parameters);

            propertyBuilder += $": {ProjectType(property.Type ?? property.ReturnType)};";

            writer.WriteLine(propertyBuilder);
            writer.WriteBlankLine();
        }

        private static string RenderMethodParameters(IEnumerable<Parameter> parameters) =>
            "(" + string.Join(", ", parameters.Select(RenderMethodParameter).ToArray()) + ")";

        private static string RenderMethodParameter(Parameter parameter)
        {
            var isParamsParameter = parameter.Name.StartsWith("...");
            var returnType = isParamsParameter ? ProjectTypeAsArray(parameter.Type) : ProjectType(parameter.Type);
            var optionalSpecifier = !isParamsParameter && parameter.Optional ? "?" : string.Empty;

            return $"{parameter.Name}{optionalSpecifier}: {returnType}";
        }
    }
}