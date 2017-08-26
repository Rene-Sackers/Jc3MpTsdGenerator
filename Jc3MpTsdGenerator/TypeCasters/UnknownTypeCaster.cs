namespace Jc3MpTsdGenerator.TypeCasters
{
    public class UnknownTypeCaster : ITypeCaster
    {
        public string Execute(string type)
        {
            return type == "unknown" ? "any" : type;
        }
    }
}