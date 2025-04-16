using ADyn;
using ADyn.Collision;
using ADyn.Components;
using Duck.Serialization;

namespace Duck.Physics.Serialization;

public class PhysicsSerializationFactory : ISerializationFactory
{
    public bool Supports(string typeName)
    {
        switch (typeName) {
            case "ADyn.RowCache":
            case "ADyn.Collision.IslandTreeResident":
            case "ADyn.Components.Aabb":
            case "ADyn.Components.AngularVelocity":
            case "ADyn.Components.DeltaLinearVelocity":
            case "ADyn.Components.Inertia":
            case "ADyn.Components.InertiaInv":
            case "ADyn.Components.InertiaWorldInv":
            case "ADyn.Components.Island":
            case "ADyn.Components.IslandAabb":
            case "ADyn.Components.IslandTag":
            case "ADyn.Components.LinearVelocity":
            case "ADyn.Components.Position":
            case "ADyn.Components.Orientation":
            case "ADyn.Mass":
            case "ADyn.MassInv":
                return true;
        }

        return false;
    }

    public void Serialize(object value, IGraphSerializer graphSerializer, ISerializationContext context)
    {
        switch (value.GetType().FullName) {
            case "ADyn.RowCache":
                RowCacheSerializer.Serialize((RowCache)value, graphSerializer, context);
                break;
            case "ADyn.Collision.IslandTreeResident":
                IslandTreeResidentSerializer.Serialize((IslandTreeResident)value, graphSerializer, context);
                break;
            case "ADyn.Components.Aabb":
                AabbSerializer.Serialize((Aabb)value, graphSerializer, context);
                break;
            case "ADyn.Components.AngularVelocity":
                AngularVelocitySerializer.Serialize((AngularVelocity)value, graphSerializer, context);
                break;
            case "ADyn.Components.DeltaLinearVelocity":
                DeltaLinearVelocitySerializer.Serialize((DeltaLinearVelocity)value, graphSerializer, context);
                break;
            case "ADyn.Components.Inertia":
                InertiaSerializer.Serialize((Inertia)value, graphSerializer, context);
                break;
            case "ADyn.Components.InertiaInv":
                InertiaInvSerializer.Serialize((InertiaInv)value, graphSerializer, context);
                break;
            case "ADyn.Components.InertiaWorldInv":
                InertiaWorldInvSerializer.Serialize((InertiaWorldInv)value, graphSerializer, context);
                break;
            case "ADyn.Components.Island":
                IslandSerializer.Serialize((Island)value, graphSerializer, context);
                break;
            case "ADyn.Components.IslandAabb":
                IslandAabbSerializer.Serialize((IslandAabb)value, graphSerializer, context);
                break;
            case "ADyn.Components.IslandTag":
                IslandTagSerializer.Serialize((IslandTag)value, graphSerializer, context);
                break;
            case "ADyn.Components.LinearVelocity":
                LinearVelocitySerializer.Serialize((LinearVelocity)value, graphSerializer, context);
                break;
            case "ADyn.Components.Position":
                PositionSerializer.Serialize((Position)value, graphSerializer, context);
                break;
            case "ADyn.Components.Orientation":
                OrientationSerializer.Serialize((Orientation)value, graphSerializer, context);
                break;
            case "ADyn.Mass":
                MassSerializer.Serialize((Mass)value, graphSerializer, context);
                break;
            case "ADyn.MassInv":
                MassInvSerializer.Serialize((MassInv)value, graphSerializer, context);
                break;
            default:
                throw new System.NotImplementedException();
        }
    }

    public object Deserialize(string typeName, IDeserializer deserializer, IDeserializationContext context)
    {
        return typeName switch {
            "ADyn.RowCache" => RowCacheSerializer.Deserialize(deserializer, context),
            "ADyn.Collision.IslandTreeResident" => IslandTreeResidentSerializer.Deserialize(deserializer, context),
            "ADyn.Components.Aabb" => AabbSerializer.Deserialize(deserializer, context),
            "ADyn.Components.AngularVelocity" => AngularVelocitySerializer.Deserialize(deserializer, context),
            "ADyn.Components.DeltaLinearVelocity" => DeltaLinearVelocitySerializer.Deserialize(deserializer, context),
            "ADyn.Components.Inertia" => InertiaSerializer.Deserialize(deserializer, context),
            "ADyn.Components.InertiaInv" => InertiaInvSerializer.Deserialize(deserializer, context),
            "ADyn.Components.InertiaWorldInv" => InertiaWorldInvSerializer.Deserialize(deserializer, context),
            "ADyn.Components.Island" => IslandSerializer.Deserialize(deserializer, context),
            "ADyn.Components.IslandAabb" => IslandAabbSerializer.Deserialize(deserializer, context),
            "ADyn.Components.IslandTag" => IslandTagSerializer.Deserialize(deserializer, context),
            "ADyn.Components.LinearVelocity" => LinearVelocitySerializer.Deserialize(deserializer, context),
            "ADyn.Components.Position" => PositionSerializer.Deserialize(deserializer, context),
            "ADyn.Components.Orientation" => OrientationSerializer.Deserialize(deserializer, context),
            "ADyn.Mass" => MassSerializer.Deserialize(deserializer, context),
            "ADyn.MassInv" => MassInvSerializer.Deserialize(deserializer, context),
            _ => throw new System.NotImplementedException(),
        };
    }
}
