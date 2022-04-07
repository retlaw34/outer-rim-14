using System.Collections.Generic;
using Content.Server.Storage;
using Content.Server.Worldgen.Systems;
using Content.Shared.Maps;
using Content.Shared.Storage;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Server.Worldgen.Populators.Debris;

public sealed class ScrapyardPopulator : DebrisPopulator
{
    [DataField("entityTable", required: true,
        customTypeSerializer: typeof(PrototypeIdDictionarySerializer<List<EntitySpawnEntry>, ContentTileDefinition>))]
    public Dictionary<string, List<EntitySpawnEntry>> EntityTable = default!;

    public override void Populate(EntityUid gridEnt, IMapGrid grid)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var random = IoCManager.Resolve<IRobustRandom>();
        var deferred = EntitySystem.Get<DeferredSpawnSystem>();

        foreach (var tile in grid.GetAllTiles())
        {
            var name = tile.Tile.GetContentTileDefinition().Name;
            if (!EntityTable.ContainsKey(name)) continue;

            var coords = grid.GridTileToLocal(tile.GridIndices);

            foreach (var spawn in EntitySpawnCollection.GetSpawns(EntityTable[name]))
            {
                deferred.SpawnEntityDeferred(spawn, coords);
            }
        }
    }
}
