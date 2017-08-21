namespace Jc3MpTsdGenerator.TypeCasters
{
    public class PlayerWeapnTypeCaster : ITypeCaster
    {
        public string Execute(string type)
        {
            return type == "PlayerWeapn" ? "PlayerWeapon" : type;
        }
    }
}