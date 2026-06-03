namespace Duck.Serialization;

[AttributeUsage(validOn: AttributeTargets.Method)]
public class GenerateListSerializerAttribute<TElementType> : Attribute
{
}
