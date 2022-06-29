namespace Binarity.Atributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class BinartiryFieldAttribute : Attribute
{
    private string _name;
    public string Name => _name;
    public BinartiryFieldAttribute(string name) => _name = name;
}