using System;
using MessagePack;

namespace Duck.Ecs
{
    [Serializable]
    public class EntityPool : IEntityPool
    {
        [IgnoreMember]
        public IWorld World {
            get;

            // FIXME: used to restore the World reference after a reload. serialization library doesn't handle circular
            // references
            internal set;
        }

        public int Count => _nextId;

        private IEntity[] _entityList;
        private int _nextId;

        /// <summary>
        /// Used for serialization.
        /// </summary>
#pragma warning disable 8618
        internal EntityPool()
#pragma warning restore 8618
        {

        }

        public EntityPool(IWorld world, int initialSize)
        {
            World = world;

            _entityList = new IEntity[initialSize];

            for (var i = 0; i < initialSize; i++) {
                _entityList[i] = new Entity(i);
            }
        }

        #region IEntityPool

        public IEntity Allocate()
        {
            // TODO: resize+gc
            
            if ((_nextId + 1) == _entityList.Length) {
                throw new Exception("EntityPool ran out");
            }

            var entityId = _nextId;
            _nextId += 1;

            // FIXME:
            ((Entity) _entityList[entityId]).IsAllocated = true;
            ((Entity) _entityList[entityId]).World = World;

            return _entityList[entityId];
        }

        public IEntity Get(int entityId)
        {
            return _entityList[entityId];
        }

        public bool IsAllocated(int entityId)
        {
            return _entityList[entityId].IsAllocated;
        }

        #endregion
    }
}
