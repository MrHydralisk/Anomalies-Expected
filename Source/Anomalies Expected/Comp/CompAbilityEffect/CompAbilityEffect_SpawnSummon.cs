using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class CompAbilityEffect_SpawnSummon : CompAbilityEffect
    {
        public new CompProperties_AbilitySpawnSummon Props => (CompProperties_AbilitySpawnSummon)props;

        protected Pawn pawn => parent.pawn;
        public List<Pawn> children
        {
            get
            {
                childrenCached.RemoveAll((Pawn p) => p == null || p.Dead || p.Destroyed);
                return childrenCached;
            }
        }
        public List<Pawn> childrenCached = new List<Pawn>();

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            for (int i = 0; i < Props.pawnKindCount.count && i + children.Count() < Props.maxAmount; i++)
            {
                Pawn pawn2 = PawnGenerator.GeneratePawn(new PawnGenerationRequest(Props.pawnKindCount.kindDef, pawn.Faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, 0f, 0f));
                GenSpawn.Spawn(pawn2, CellFinder.StandableCellNear(target.Cell, pawn.Map, 2f), pawn.Map);
                ApplyPerEach(pawn2, target, dest);
                if (Props.addHediff != null)
                {
                    pawn2.health.AddHediff(Props.addHediff);
                }
                childrenCached.Add(pawn2);
            }
        }

        public virtual void ApplyPerEach(Pawn summon, LocalTargetInfo target, LocalTargetInfo dest)
        {

        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (pawn.Faction != null)
            {
                foreach (IntVec3 item in GenRadial.RadialCellsAround(target.Cell, 25, true))
                {
                    List<Thing> thingList = item.GetThingList(pawn.Map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        if (thingList[i].Faction != pawn.Faction)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
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

        public override bool GizmoDisabled(out string reason)
        {
            if (children.Count() >= Props.maxAmount)
            {
                reason = "AnomaliesExpected.Fleshmass.MaxSummons".Translate(children.Count(), Props.maxAmount);
                return true;
            }
            reason = null;
            return false;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref childrenCached, "children", LookMode.Reference);
        }
    }
}
