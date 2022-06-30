namespace Binarity.Atributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class BinarityFieldAttribute : Attribute
{
    private string _name;
    public string Name => _name;
    public BinarityFieldAttribute(string name) => _name = name;
}