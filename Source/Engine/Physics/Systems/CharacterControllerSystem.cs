namespace Duck.Physics.Systems;

/*public class CharacterControllerSystem : SystemBase
{
    private readonly IFilter<CharacterControllerComponent, BodyDetailsComponent> _filter;

    private readonly Simulation _simulation;
    private readonly CharacterControllerIntegration _characterControllerIntegration;

    public CharacterControllerSystem(IWorld world, IPhysicsModule physicsModule)
    {
        PhysicsWorld physicsWorld = (PhysicsWorld)physicsModule.GetOrCreatePhysicsWorld(world);

        _simulation = physicsWorld.Simulation;
        _characterControllerIntegration = physicsWorld.CharacterControllerIntegration;

        _filter = Filter<CharacterControllerComponent, BodyDetailsComponent>(world)
            .Build();
    }

    public override void Run()
    {
        HandleAdded();
        Iterate();
    }

    private void Iterate()
    {
        foreach (int entityId in _filter.EntityList) {
            ref CharacterControllerComponent controllerComponent = ref _filter.Get1(entityId);
            BodyReference characterBody = new BodyReference(controllerComponent.BodyHandle, _simulation.Bodies);

            // Modifying the character's raw data does not automatically wake the character up, so we do so explicitly if necessary.
            // If you don't explicitly wake the character up, it won't respond to the changed motion goals.
            // (You can also specify a negative deactivation threshold in the BodyActivityDescription to prevent the character from sleeping at all.)
            if (!characterBody.Awake && controllerComponent.AwakeBody) {
                _simulation.Awakener.AwakenBody(controllerComponent.BodyHandle);
                controllerComponent.AwakeBody = false;
            }
        }
    }

    private void HandleAdded()
    {
        foreach (int entityId in _filter.EntityAddedList) {
            ref CharacterControllerComponent controllerComponent = ref _filter.Get1(entityId);
            BodyDetailsComponent bodyDetailsComponent = _filter.Get2(entityId);

            _characterControllerIntegration.Allocate(entityId, ref controllerComponent, bodyDetailsComponent);
        }

        if (_filter.EntityRemovedList.Length > 0) {
            // FIXME: RemoveCharacterByIndex
            throw new Exception("NOT IMPLEMENTED");
        }
    }
}
*/
