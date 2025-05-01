using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class IncidentWorker_DeployFromUnderground : IncidentWorker_AE
    {
        public static readonly LargeBuildingSpawnParms AnomalySpawnParms = new LargeBuildingSpawnParms
        {
            ignoreTerrainAffordance = true
        };

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!LargeBuildingCellFinder.TryFindCell(out var cell, map, AnomalySpawnParms.ForThing(DeployableObjectDef)))
            {
                return false;
            }
            BuildingGroundSpawner buildingGroundSpawner = (BuildingGroundSpawner)ThingMaker.MakeThing(SkyfallerDef);
            Thing obj = buildingGroundSpawner.ThingToSpawn;
            obj.SetFaction(Faction.OfEntities);
            GenSpawn.Spawn(buildingGroundSpawner, cell, map);
            SendStandardLetter(def.letterLabel, def.letterText, def.letterDef ?? LetterDefOf.NeutralEvent, parms, isHaveArrow ? buildingGroundSpawner : null);
            return true;
        }
    }
}
