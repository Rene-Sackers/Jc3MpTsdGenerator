using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Jc3MpTsdGenerator.TypeCasters
{
    public class ArrayTypeCaster : ITypeCaster
    {
        public string Execute(string type)
        {
            // Array

            var arrayMatch = Regex.Match(type, @"(?!.*<\w+>)(?=Array)(Array)");
            return !arrayMatch.Success ? type : "Array<any>";
        }
    }
}