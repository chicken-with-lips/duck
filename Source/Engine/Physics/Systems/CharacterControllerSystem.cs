using BepuPhysics;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Physics.Components;

namespace Duck.Physics.Systems
{
    public class CharacterControllerSystem : SystemBase
    {
        private readonly IFilter<CharacterControllerComponent, PhysicsBodyComponent> _filter;

        private readonly Simulation _simulation;
        private readonly CharacterControllerIntegration _characterControllerIntegration;

        public CharacterControllerSystem(IWorld world, IPhysicsSubsystem physicsSubsystem)
        {
            PhysicsWorld physicsWorld = (PhysicsWorld)physicsSubsystem.GetOrCreatePhysicsWorld(world);

            _simulation = physicsWorld.Simulation;
            _characterControllerIntegration = physicsWorld.CharacterControllerIntegration;

            _filter = Filter<CharacterControllerComponent, PhysicsBodyComponent>(world)
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
                PhysicsBodyComponent bodyComponent = _filter.Get2(entityId);

                _characterControllerIntegration.Allocate(entityId, ref controllerComponent, bodyComponent);
            }

            if (_filter.EntityRemovedList.Length > 0) {
                // FIXME: RemoveCharacterByIndex
                throw new Exception("NOT IMPLEMENTED");
            }
        }
    }
}
