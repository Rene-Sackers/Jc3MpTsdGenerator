using System.Text.RegularExpressions;

namespace Jc3MpTsdGenerator.TypeCasters
{
    public class NullBackTickCaster : ITypeCaster
    {
        public string Execute(string type)
        {
            return Regex.Replace(type, "`null`", "null");
        }
    }
}