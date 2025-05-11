using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class HediffComp_SpawnPawnOnDeath : HediffComp
    {
        public HediffCompProperties_SpawnPawnOnDeath Props => (HediffCompProperties_SpawnPawnOnDeath)props;

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
            IntVec3 pos = parent.pawn.Corpse.Position;
            Map map = parent.pawn.Corpse.Map;
            parent.pawn.Corpse.Destroy(DestroyMode.KillFinalizeLeavingsOnly);
            Faction faction = Find.FactionManager.FirstFactionOfDef(Props.factionDef) ?? Faction.OfEntities ?? parent.pawn.Faction;
            for (int i = 0; i < Props.pawnKindCount.count; i++)
            {
                Pawn pawn2 = PawnGenerator.GeneratePawn(new PawnGenerationRequest(Props.pawnKindCount.kindDef, faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, 0f, 0f));
                GenSpawn.Spawn(pawn2, CellFinder.StandableCellNear(pos, map, 2f), map);
            }
        }
    }
}
