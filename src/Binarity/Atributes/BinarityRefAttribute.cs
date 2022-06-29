namespace Binarity.Atributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public class BinarityRefAttribute : Attribute
{
    public string Name => _name;
    private string _name;
    public BinarityRefAttribute(string name) => _name = name;
}