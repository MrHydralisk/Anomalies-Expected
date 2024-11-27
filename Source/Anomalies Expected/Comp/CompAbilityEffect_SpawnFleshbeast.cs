using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class CompAbilityEffect_SpawnFleshbeast : CompAbilityEffect
    {
        public new CompProperties_AbilitySpawnFleshbeast Props => (CompProperties_AbilitySpawnFleshbeast)props;

        private Pawn pawn => parent.pawn;
        public List<Pawn> children = new List<Pawn>();

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            children.RemoveAll((Pawn p) => p == null || p.Dead || p.Destroyed);
            for (int i = 0; i + children.Count() < Props.pawnKindCount.count; i++)
            {
                Pawn pawn2 = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Fingerspike, pawn.Faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, 0f, 0f));
                GenSpawn.Spawn(pawn2, CellFinder.StandableCellNear(target.Cell, pawn.Map, 2f), pawn.Map);
                FilthMaker.TryMakeFilth(target.Cell, pawn.Map, ThingDefOf.Filth_CorpseBile);
                if (Props.addHediff != null)
                {
                    pawn2.health.AddHediff(Props.addHediff);
                }
                else
                {
                    children.Add(pawn2);
                }
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (target.Cell.Filled(parent.pawn.Map))
            {
                if (throwMessages)
                {
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityOccupiedCells".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            return true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref children, "children", LookMode.Reference);
        }
    }
}
