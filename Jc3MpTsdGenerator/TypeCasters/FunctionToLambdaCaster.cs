namespace Jc3MpTsdGenerator.TypeCasters
{
    public class FunctionToLambdaCaster : ITypeCaster
    {
        public string Execute(string type)
        {
            return type == "function" ? "() => {}" : type;
        }
    }
}