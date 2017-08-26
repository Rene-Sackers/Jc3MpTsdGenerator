﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Jc3MpTsdGenerator.Models;
using Jc3MpTsdGenerator.TypeCasters;

namespace Jc3MpTsdGenerator
{
    public class DefinitionGenerator
    {
        private static readonly List<ITypeCaster> TypeCasters = new List<ITypeCaster>
        {
            new OrTypeCaster(),
            new ArrayTypeCaster(),
            new PlayerWeapnTypeCaster(),
            new FunctionToLambdaCaster(),
            new NullBackTickCaster()
        };

        private readonly ApiDefinition _definition;
        private readonly string _outputPath;

        private DefinitionWriterState _writer;

        public DefinitionGenerator(ApiDefinition definition, string outputPath)
        {
            _definition = definition;
            _outputPath = outputPath;
        }
        
        public void Generate()
        {
            using (var serverDefinitionFileStream = File.Create(_outputPath))
            using (var serverDefinitionStreamWriter = new StreamWriter(serverDefinitionFileStream))
            using (_writer = new DefinitionWriterState(serverDefinitionStreamWriter))
                _definition.Classes.ToList().ForEach(GenerateClassDefinition);

            _writer = null;
        }

        private void GenerateClassDefinition(Class @class)
        {
            if (!string.IsNullOrWhiteSpace(@class.InstanceOf))
            {
                DeclareClassInstance(@class);
                return;
            }

            // Description
            if (!string.IsNullOrWhiteSpace(@class.Description))
            {
                _writer.WriteLine("/**");
                WriteCommentBlockContent(@class.Description);
                _writer.WriteLine(" */");
            }

            // Declaration
            WriteInterfaceDeclaration(@class);
            _writer.IncreaseIndentation();

            // Properties
            @class.Properties.ToList().ForEach(WriteClassProperty);

            // Constructor
            WriteClassConstructor(@class);

            // Methods
            @class.Functions.ToList().ForEach(WriteClassMethod);

            // Closing
            _writer.DecreaseIndentation();
            _writer.WriteLine("}");
            _writer.WriteBlankLine();
        }

        private void DeclareClassInstance(Class @class)
        {
            // Description
            if (!string.IsNullOrWhiteSpace(@class.Description))
            {
                _writer.WriteLine("/**");
                WriteCommentBlockContent(@class.Description);
                _writer.WriteLine(" */");
            }

            // Declaration
            _writer.WriteLine($"declare const {@class.ClassName}: {@class.InstanceOf}");
            _writer.WriteBlankLine();
        }

        private static string ProcessCodeComment(string comment)
        {
            var linkedTypeRegexReplace = new Regex(@"\{\{linked_type '(?<type>[^']+)'\}\}");
            comment = linkedTypeRegexReplace.Replace(comment, "{$1}");

            return comment;
        }

        private void WriteClassMethod(Function method)
        {
            if (!string.IsNullOrWhiteSpace(method.Description) || method.Parameters.Any() || !string.IsNullOrWhiteSpace(method.Example))
            {
                _writer.WriteLine("/**");
                WriteCommentBlockContent(method.Description);
                WriteParametersCommentBlock(method.Parameters);
                if (!string.IsNullOrWhiteSpace(method.Example))
                    WriteCommentBlockContent("@example " + method.Example);
                _writer.WriteLine(" */");
            }

            _writer.WriteLine($"{method.Name}{RenderMethodParameters(method.Parameters)}: {ProjectType(method.ReturnType)}");
            _writer.WriteBlankLine();
        }

        private void WriteClassConstructor(Class @class)
        {
            if (!@class.HasConstructor) return;
            
            _writer.WriteLine($"constructor{RenderMethodParameters(@class.Constructor.Parameters)};");
            _writer.WriteBlankLine();
        }

        private void WriteInterfaceDeclaration(Class @class)
        {
            var interfaceDeclarationBuilder = $"declare interface {@class.ClassName}";

            _writer.WriteLine($"{interfaceDeclarationBuilder} {{");
        }

        private void WriteParametersCommentBlock(ICollection<Parameter> parameters)
        {
            if (parameters == null || !parameters.Any()) return;

            parameters.ToList().ForEach(WriteParametersCommentBlockParameter);
            _writer.WriteLine(" *");
        }

        private void WriteParametersCommentBlockParameter(Parameter parameter)
        {
            _writer.WriteLine($" * @param {{{ProjectType(parameter.Type)}}} {parameter.Name.Replace("...", "")}");
        }
        
        private void WriteCommentBlockContent(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
                return;

            comment = comment.Replace("\r\n", "\n").Replace("\r", "\n");
            comment = ProcessCodeComment(comment);

            foreach (var line in comment.Split(new[] { "\n" }, StringSplitOptions.None))
                _writer.WriteLine($" * {line}");
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

        private void WriteClassProperty(Property property)
        {
            if (!string.IsNullOrWhiteSpace(property.Description))
            {
                _writer.WriteLine("/**");
                WriteCommentBlockContent(property.Description);
                _writer.WriteLine(" */");
            }

            var propertyBuilder = string.Empty;

            if (property.ReadOnly)
                propertyBuilder += "readonly ";

            propertyBuilder += property.Name;

            if (property.HasParameters == true && property.Parameters != null)
                propertyBuilder += RenderMethodParameters(property.Parameters);

            propertyBuilder += $": {ProjectType(property.Type ?? property.ReturnType)};";

            _writer.WriteLine(propertyBuilder);
            _writer.WriteBlankLine();
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