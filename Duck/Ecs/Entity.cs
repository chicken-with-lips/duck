using System;

namespace Duck.Ecs
{
    public class Entity : IEntity
    {
        public IWorld World {
            get;

            // FIXME: used to restore the World reference after a reload. serialization library doesn't handle circular
            // references
            internal set;
        }

        private int[] _components;
        private bool _isAllocated;

        /// <summary>
        /// Used for serialization.
        /// </summary>
#pragma warning disable 8618
        internal Entity()
#pragma warning restore 8618
        {
        }

        public Entity(int index)
        {
            Id = index;

            _components = new int[100];

            for (var i = 0; i < _components.Length; i++) {
                _components[i] = -1;
            }
        }

        #region IEntity

        public int Id { get; }

        public ref T Get<T>() where T : struct
        {
            var typeIndex = World.GetTypeIndexForComponent<T>();

            if (_components[typeIndex] != IWorld.NotFound) {
                return ref World.GetComponent<T>(typeIndex, _components[typeIndex]);
            }

            var componentReference = World.AllocateComponent<T>(this);
            _components[componentReference.TypeIndex] = componentReference.ComponentIndex;

            World.InternalNotifyComponentAllocated(componentReference);

            return ref World.GetComponent<T>(typeIndex, componentReference.ComponentIndex);
        }

        public void Remove<T>() where T : struct
        {
            var typeIndex = World.GetTypeIndexForComponent<T>();

            if (_components[typeIndex] == IWorld.NotFound) {
                return;
            }

            var cmpIndex = _components[typeIndex];

            _components[typeIndex] = IWorld.NotFound;

            World.DeallocateComponent<T>(cmpIndex);
            World.InternalNotifyComponentDeallocated(this);
        }

        public ComponentReference GetComponentReference<T>() where T : struct
        {
            var typeIndex = World.GetTypeIndexForComponent<T>();

            if (_components[typeIndex] != IWorld.NotFound) {
                return World.GetComponentReference<T>(typeIndex, _components[typeIndex]);
            }

            throw new Exception("Component reference not found");
        }

        public bool Has(Type type)
        {
            var typeIndex = World.GetTypeIndexForComponent(type);

            return typeIndex != IWorld.NotFound && _components[typeIndex] != IWorld.NotFound;
        }

        public bool Has<T>()
        {
            return Has(typeof(T));
        }

        public bool IsAllocated {
            get { return _isAllocated; }
            set { _isAllocated = value; }
        }

        #endregion
    }
}
