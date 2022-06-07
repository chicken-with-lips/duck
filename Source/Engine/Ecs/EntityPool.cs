using Duck.Serialization;

namespace Duck.Ecs
{
    [AutoSerializable]
    public partial class EntityPool : IEntityPool
    {
        public int Count => _nextId;

        private IWorld _world;
        private IEntity[] _entityList;
        private int _nextId;

        public EntityPool(IWorld world, int initialSize)
        {
            _world = world;
            _entityList = new IEntity[initialSize];

            for (var i = 0; i < initialSize; i++) {
                _entityList[i] = new Entity(world, i);
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

            return _entityList[entityId];
        }

        public void Deallocate(IEntity entity)
        {
            // FIXME:
            ((Entity) _entityList[entity.Id]).IsAllocated = false;
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
