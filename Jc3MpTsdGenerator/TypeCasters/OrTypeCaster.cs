using System.Text.RegularExpressions;

namespace Jc3MpTsdGenerator.TypeCasters
{
    public class OrTypeCaster : ITypeCaster
    {
        public string Execute(string type)
        {
            // Array<PlayerWeapon>|undefined
            // Array<PlayerWeapon> or undefined

            var match = Regex.Match(type, @"(?<first>[^\s]+)(?:\sor\s|\|)(?<second>[^\s]+)");
            if (!match.Success || match.Groups.Count != 3)
                return type;

            return match.Groups["first"].Value + " | " + match.Groups["second"].Value;
        }
    }
}