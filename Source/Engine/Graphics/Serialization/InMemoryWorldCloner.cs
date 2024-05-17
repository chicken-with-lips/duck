using System.Reflection;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions.Dangerous;
using Arch.Core.Utils;
using Arch.LowLevel.Jagged;
using Duck.ECS.Components;

namespace Duck.Graphics.Serialization;

public static class InMemoryWorldCloner
{
    public static World Copy(World sourceWorld)
    {
        var destWorld = World.Create();
        var oldArchetypeToNewArchetypeMap = new Dictionary<Archetype, Archetype>();
        var versions = new JaggedArray<int>(16_000 / Unsafe.SizeOf<int>(), sourceWorld.GetVersions().Capacity);

        for (var i = 0; i < sourceWorld.GetVersions().Capacity; i++) {
            versions.Add(i, sourceWorld.GetVersions()[i]);
        }

        // var slots = sourceWorld.GetSlots();
        var slots = new JaggedArray<(Archetype, (int, int))>(16_000 / Unsafe.SizeOf<(Archetype, (int, int))>(), sourceWorld.GetSlots().Capacity);
        //
        // for (var i = 0; i < sourceWorld.GetSlots().Capacity; i++) {
        //     slots.Add(i, sourceWorld.GetSlots()[i]);
        // }

        var recycledEntityIds = new List<(int, int)>(sourceWorld.GetRecycledEntityIds().Count);

        foreach (var recycledEntityId in sourceWorld.GetRecycledEntityIds()) {
            recycledEntityIds.Add(recycledEntityId);
        }

        var archetypes = new List<Archetype>(sourceWorld.Archetypes.Count);

        foreach (var sourceArchetype in sourceWorld.Archetypes) {
            var archetypeTypes = (ComponentType[])sourceArchetype.Types.Clone();
            var archetypeLookupArray = (int[])sourceArchetype.GetLookupArray().Clone();
            var archetypeChunkCount = sourceArchetype.ChunkCount;
            var archetypeChunks = new List<Chunk>(sourceArchetype.Chunks.Length);
            var archetypeEntityCount = 0;

            var newArchetype = DangerousArchetypeExtensions.CreateArchetype(archetypeTypes);
            newArchetype.SetSize(archetypeChunkCount);

            for (var i = 0; i < archetypeChunkCount; i++) {
                ref var sourceChunk = ref sourceArchetype.Chunks[i];
                var newChunkEntities = new List<Entity>(sourceChunk.Size);

                foreach (var chunkEntity in sourceChunk.Entities) {
                    newChunkEntities.Add(
                        DangerousEntityExtensions.CreateEntityStruct(chunkEntity.Id, destWorld.Id)
                    );
                }

                var newChunk = DangerousChunkExtensions.CreateChunk(sourceChunk.Capacity, archetypeLookupArray, archetypeTypes);
                newChunkEntities.CopyTo(newChunk.Entities, 0);
                newChunk.SetSize(sourceChunk.Size);

                for (var index = 0; index < sourceChunk.Size; index++) {
                    ref var entity = ref sourceChunk.Entity(index);
                    entity = DangerousEntityExtensions.CreateEntityStruct(entity.Id, destWorld.Id);
                    destWorld.SetArchetype(entity, newArchetype);
                }

                for (var index = 0; index < archetypeTypes.Length; index++) {
                    ref var type = ref archetypeTypes[index];

                    var array = (Array)sourceChunk.GetArray(type).Clone();
                    var chunkArray = newChunk.GetArray(array.GetType().GetElementType());
                    Array.Copy(array, chunkArray, sourceChunk.Size);
                }

                archetypeEntityCount += newChunk.Size;
                archetypeChunks.Add(newChunk);
            }

            newArchetype.SetChunks(archetypeChunks);
            newArchetype.SetEntities(archetypeEntityCount);

            archetypes.Add(newArchetype);
            oldArchetypeToNewArchetypeMap.Add(sourceArchetype, newArchetype);
        }

        var sourceSlots = sourceWorld.GetSlots();

        for (var i = 0; i < sourceSlots.Buckets; i++) {
            var slot = sourceSlots[i];
            var archetype = null != slot.Item1 ? oldArchetypeToNewArchetypeMap[slot.Item1] : null;

            slots.Add(i, new ValueTuple<Archetype, (int, int)>(archetype, slot.Item2));
        }

        destWorld.SetArchetypes(archetypes);
        destWorld.SetVersions(versions);
        destWorld.SetSlots(slots);
        destWorld.SetRecycledEntityIds(recycledEntityIds);
        destWorld.EnsureCapacity(versions.Capacity);

        // remove runtime components
        List<ComponentType> typesToRemove = new();

        foreach (var archetype in destWorld.Archetypes) {
            foreach (var componentType in archetype.Types) {
                // TOOD: replace with source generator?
                if (componentType.Type.GetCustomAttribute(typeof(RuntimeComponentAttribute)) != null
                    || componentType.Type.GetCustomAttribute(typeof(EditorComponentAttribute)) != null) {
                    typesToRemove.Add(componentType);
                }
            }
        }

        var queryDesc = new QueryDescription();
        queryDesc.Any = typesToRemove.Distinct().ToArray();

        destWorld.Destroy(queryDesc);

        return destWorld;
    }
}
