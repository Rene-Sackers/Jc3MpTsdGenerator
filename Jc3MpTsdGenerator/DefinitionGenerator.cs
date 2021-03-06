﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jc3MpTsdGenerator.ClassDefinitionAppenders;
using Jc3MpTsdGenerator.Extensions;
using Jc3MpTsdGenerator.Helpers;
using Jc3MpTsdGenerator.Models;

namespace Jc3MpTsdGenerator
{
    public class DefinitionGenerator
    {
        private readonly ApiDefinition _definition;
        private readonly string _outputPath;

        private DefinitionWriterState _writer;

        private static readonly List<IClassDefinitionAppender> ClassDefinitionAppenders = new List<IClassDefinitionAppender>
        {
            new EventSystemDefinitionAppender()
        };

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
                _writer.WriteCommentBlockContent(@class.Description);
                _writer.WriteLine(" */");
            }

            // Declaration
            WriteInterfaceDeclaration(@class);
            _writer.IncreaseIndentation();

            // Custom properties
            if (@class.CustomProperties)
                WriteCustomPropertiesIndexers();

            // Properties
            @class.Properties.ToList().ForEach(WriteClassProperty);

            // Constructor
            WriteClassConstructor(@class);

            // Methods
            @class.Functions.ToList().ForEach(WriteClassMethod);

            // Appenders
            ClassDefinitionAppenders.ForEach(appender => appender.AppendDefinition(_definition, @class, _writer));

            // Closing
            _writer.DecreaseIndentation();
            _writer.WriteLine("}");
            _writer.WriteBlankLine();
        }

        private void WriteCustomPropertiesIndexers()
        {
            _writer.WriteLine("[customProperty: string]: any;");
            _writer.WriteLine("[customProperty: number]: any;");
            _writer.WriteBlankLine();
        }

        private void DeclareClassInstance(Class @class)
        {
            // Description
            if (!string.IsNullOrWhiteSpace(@class.Description))
            {
                _writer.WriteLine("/**");
                _writer.WriteCommentBlockContent(@class.Description);
                _writer.WriteLine(" */");
            }

            // Declaration
            _writer.WriteLine($"declare const {@class.ClassName}: {@class.InstanceOf}");
            _writer.WriteBlankLine();
        }

        private void WriteClassMethod(Function method)
        {
            if (!string.IsNullOrWhiteSpace(method.Description) || method.Parameters.Any() || !string.IsNullOrWhiteSpace(method.Example))
            {
                _writer.WriteLine("/**");
                _writer.WriteCommentBlockContent(method.Description);
                WriteParametersCommentBlock(method.Parameters);
                if (!string.IsNullOrWhiteSpace(method.Example))
                    _writer.WriteCommentBlockContent("@example " + method.Example);
                _writer.WriteLine(" */");
            }

            _writer.WriteLine($"{method.Name}{MethodParameterRendering.RenderMethodParameters(method.Parameters)}: {TypeProjection.ProjectType(method.ReturnType)}");
            _writer.WriteBlankLine();
        }

        private void WriteClassConstructor(Class @class)
        {
            if (!@class.HasConstructor) return;

            _writer.WriteLine($"constructor{MethodParameterRendering.RenderMethodParameters(@class.Constructor.Parameters)}: {@class.ClassName};");
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
            _writer.WriteLine($" * @param {{{TypeProjection.ProjectType(parameter.Type)}}} {parameter.Name.Replace("...", "")}");
        }

        private void WriteClassProperty(Property property)
        {
            if (!string.IsNullOrWhiteSpace(property.Description))
            {
                _writer.WriteLine("/**");
                _writer.WriteCommentBlockContent(property.Description);
                _writer.WriteLine(" */");
            }

            var propertyBuilder = string.Empty;

            if (property.ReadOnly)
                propertyBuilder += "readonly ";

            propertyBuilder += property.Name;

            if (property.HasParameters == true && property.Parameters != null)
                propertyBuilder += MethodParameterRendering.RenderMethodParameters(property.Parameters);

            propertyBuilder += $": {TypeProjection.ProjectType(property.Type ?? property.ReturnType)};";

            _writer.WriteLine(propertyBuilder);
            _writer.WriteBlankLine();
        }
    }
}