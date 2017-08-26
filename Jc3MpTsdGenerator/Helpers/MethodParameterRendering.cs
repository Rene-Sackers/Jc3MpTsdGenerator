using System.Collections.Generic;
using System.Linq;
using Jc3MpTsdGenerator.Models;

namespace Jc3MpTsdGenerator.Helpers
{
    public static class MethodParameterRendering
    {
        public static string RenderMethodParameters(IEnumerable<Parameter> parameters) =>
            "(" + string.Join(", ", parameters.Select(RenderMethodParameter).ToArray()) + ")";


        private static string RenderMethodParameter(Parameter parameter)
        {
            var isParamsParameter = parameter.Name.StartsWith("...");
            var returnType = isParamsParameter ? TypeProjection.ProjectTypeAsArray(parameter.Type) : TypeProjection.ProjectType(parameter.Type);
            var optionalSpecifier = !isParamsParameter && parameter.Optional ? "?" : string.Empty;

            return $"{parameter.Name}{optionalSpecifier}: {returnType}";
        }
    }
}