namespace Duck.Physics.Components
{
    public enum BodyType
    {
        Dynamic,
        Kinematic,
        Static
    }

    public struct PhysicsBoxComponent
    {
        public BodyType BodyType;
        public float Mass;
    }
}
