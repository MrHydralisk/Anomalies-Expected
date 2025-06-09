using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class CompAbilityEffect_GrindBody : CompAbilityEffect
    {
        public new CompProperties_AbilityGrindBody Props => (CompProperties_AbilityGrindBody)props;

        private Pawn Pawn => parent.pawn;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (target.Thing is Corpse corpse && corpse.InnerPawn != null)
            {
                IEnumerable<Thing> products = corpse.InnerPawn.ButcherProducts(Pawn, Props.butcherEfficiency);
                if (corpse.InnerPawn.RaceProps.BloodDef != null)
                {
                    FilthMaker.TryMakeFilth(corpse.Position, corpse.Map, corpse.InnerPawn.RaceProps.BloodDef, corpse.InnerPawn.LabelIndefinite());
                }
                if (corpse.InnerPawn.RaceProps.Humanlike)
                {
                    Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.ButcheredHuman, new SignalArgs(Pawn.Named(HistoryEventArgsNames.Doer), corpse.InnerPawn.Named(HistoryEventArgsNames.Victim))));
                }
                foreach (Thing product in products)
                {
                    GenPlace.TryPlaceThing(product, corpse.Position, corpse.Map, ThingPlaceMode.Near);
                }
                corpse.Destroy();
            }
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (Pawn.Faction != null && target.Thing is Corpse corpse && corpse.InnerPawn != null && corpse.Faction != Pawn.Faction)
            {
                return true;
            }
            return false;
        }
    }
}
