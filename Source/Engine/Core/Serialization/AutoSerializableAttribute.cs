namespace Duck.Serialization;

[AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class AutoSerializableAttribute : Attribute
{
}
