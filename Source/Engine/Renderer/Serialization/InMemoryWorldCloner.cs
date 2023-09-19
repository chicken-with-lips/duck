using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions.Dangerous;
using Arch.Core.Utils;
using CommunityToolkit.HighPerformance;
using Duck.ECS;
using Duck.Renderer.Components;

namespace Duck.Renderer.Serialization;

public static class InMemoryWorldCloner
{
    public static World Copy(World sourceWorld)
    {
        var destWorld = World.Create();
        var versions = (int[][])sourceWorld.GetVersions().Clone();
        var slots = Unsafe.As<(int, int)[][]>(sourceWorld.GetSlots().Clone());

        var archetypes = new List<Archetype>(sourceWorld.Archetypes.Count);

        foreach (var sourceArchetype in sourceWorld.Archetypes) {
            var archetypeTypes = (ComponentType[])sourceArchetype.Types.Clone();
            var archetypeLookupArray = (int[])sourceArchetype.GetLookupArray().Clone();
            var archetypeChunkSize = sourceArchetype.Size;
            var archetypeChunks = new List<Chunk>(sourceArchetype.Chunks.Length);
            var archetypeEntityCount = 0;

            var newArchetype = DangerousArchetypeExtensions.CreateArchetype(archetypeTypes);
            newArchetype.SetSize(archetypeChunkSize);

            for (var i = 0; i < archetypeChunkSize; i++) {
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
        }

        destWorld.SetArchetypes(archetypes);
        destWorld.SetVersions(versions);
        destWorld.SetSlots(slots);
        destWorld.EnsureCapacity(versions.Length);

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

        // TODO: profile. we're individually removing components instead of bulk removing. this will cause many archetype migrations
        var queryDesc = new QueryDescription();
        queryDesc.Any = typesToRemove.Distinct().ToArray();

        foreach (var componentType in typesToRemove) {
            destWorld.Remove(queryDesc, componentType);
        }

        return destWorld;
    }
}
