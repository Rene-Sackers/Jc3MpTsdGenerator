using System.Collections.Generic;
using Jc3MpTsdGenerator.TypeCasters;

namespace Jc3MpTsdGenerator.Helpers
{
    public class TypeProjection
    {
        private static readonly List<ITypeCaster> TypeCasters = new List<ITypeCaster>
        {
            new UnknownTypeCaster(),
            new OrTypeCaster(),
            new ArrayTypeCaster(),
            new PlayerWeapnTypeCaster(),
            new FunctionToLambdaCaster(),
            new NullBackTickCaster()
        };
        
        public static string ProjectTypeAsArray(string returnType)
        {
            returnType = ProjectType(returnType);

            return $"Array<{returnType}>";
        }

        public static string ProjectType(string returnType)
        {
            if (string.IsNullOrWhiteSpace(returnType))
                return "any";

            TypeCasters.ForEach(typeCaster => returnType = typeCaster.Execute(returnType));

            return returnType;
        }
    }
}