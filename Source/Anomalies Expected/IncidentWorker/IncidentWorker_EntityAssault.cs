using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace AnomaliesExpected
{
    public class IncidentWorker_EntityAssault : IncidentWorker
    {
        public IncidentDefExtension Ext => def.GetModExtension<IncidentDefExtension>();
        public List<EntityCodexEntryDef> entityCodexEntryDefsRequired => Ext?.entityCodexEntryDefsRequired ?? null;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (ModsConfig.AnomalyActive)
            {
                if (Find.Anomaly.LevelDef.anomalyThreatTier > Ext.maxAnomalyThreatLevel || !Find.Anomaly.GenerateMonolith)
                {
                    return false;
                }
                if (entityCodexEntryDefsRequired?.Any(eced => !Find.EntityCodex.Discovered(eced)) ?? false)
                {
                    return false;
                }
            }
            return base.CanFireNowSub(parms);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(Ext.factionDef) ?? Faction.OfEntities;
            parms.faction = faction;
            parms.raidArrivalMode = Ext.pawnsArrivalModeDef.RandomElement();
            PawnGroupKindDef pawnGroupKindDef = Ext.pawnGroupKindDef.RandomElement();
            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(pawnGroupKindDef, parms);
            float num = Ext.factionDef.MinPointsToGeneratePawnGroup(pawnGroupKindDef);
            if (parms.points < num)
            {
                parms.points = (defaultPawnGroupMakerParms.points = num * 2f);
            }
            List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();
            if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
            {
                return false;
            }
            parms.raidArrivalMode.Worker.Arrive(list, parms);
            if (AnomalyIncidentUtility.IncidentShardChance(parms.points))
            {
                AnomalyIncidentUtility.PawnShardOnDeath(list.RandomElement());
            }
            LordMaker.MakeNewLord(faction, new LordJob_AssaultColony(), parms.target as Map, list);
            SendStandardLetter(def.letterLabel, def.letterText, def.letterDef, parms, list);
            return true;
        }
    }
}
