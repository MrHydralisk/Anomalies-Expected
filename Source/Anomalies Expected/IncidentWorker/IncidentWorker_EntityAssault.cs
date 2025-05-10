using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace AnomaliesExpected
{
    public class IncidentWorker_EntityAssault : IncidentWorker_AE
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            AssaultSummonPattern assaultSummonPattern = Ext.AssaultSummonPattern.RandomElementByWeight((AssaultSummonPattern asp) => asp.commonality);
            Faction faction = Find.FactionManager.FirstFactionOfDef(Ext.factionDef) ?? Faction.OfEntities;
            parms.faction = faction;
            parms.raidArrivalMode = assaultSummonPattern.pawnsArrivalModeDef.RandomElement();
            PawnGroupKindDef pawnGroupKindDef = assaultSummonPattern.pawnGroupKindDef.RandomElement();
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
            Map map = parms.target as Map;
            LordMaker.MakeNewLord(faction, new LordJob_AssaultColony(), map, list);

            if (def.gameCondition != null && map.gameConditionManager.GetActiveCondition(def.gameCondition) == null)
            {
                GameCondition gameCondition = GameConditionMaker.MakeCondition(def.gameCondition);
                map.GameConditionManager.RegisterCondition(gameCondition);
                gameCondition.Permanent = true;
            }
            SendStandardLetter(def.letterLabel, def.letterText, def.letterDef, parms, list);
            return true;
        }
    }
}
