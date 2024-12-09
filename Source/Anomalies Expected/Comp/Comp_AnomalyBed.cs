using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class Comp_AnomalyBed : ThingComp
    {
        public CompProperties_AnomalyBed Props => (CompProperties_AnomalyBed)props;

        protected CompStudiable Studiable => studiableCached ?? (studiableCached = parent.TryGetComp<CompStudiable>());
        private CompStudiable studiableCached;

        Building_Bed Bed => parent as Building_Bed;

        public override void CompTickRare()
        {
            base.CompTickRare();
            List<Pawn> BedPawns = Bed.CurOccupants.Where((Pawn p1) => p1.needs.rest.CurLevel < p1.needs.rest.MaxLevel).ToList();
            if (BedPawns.Count > 0)
            {
                List<Pawn> AvailablePawns = Bed.Map.mapPawns.AllHumanlikeSpawned.Where((Pawn p1) => !BedPawns.Any((Pawn p2) => p1 == p2) && (p1.needs?.rest?.CurLevel ?? 0) > 0).ToList();
                if (AvailablePawns.Count > BedPawns.Count)
                {
                    for (int i = 0; i < BedPawns.Count; i++)
                    {
                        Pawn BedPawn = BedPawns[i];
                        Need_Rest need = BedPawn.needs.rest;
                        while (need.CurLevel < need.MaxLevel && AvailablePawns.Count > 0)
                        {
                            Pawn UsedPawn = Rand.Element(AvailablePawns.ToArray());
                            float taken = TakeRest(UsedPawn, need.MaxLevel - need.CurLevel);
                            need.CurLevel += taken;
                            Studiable.Study(BedPawn, 0, taken);
                            AvailablePawns.Remove(UsedPawn);
                        }
                    }
                }
            }
        }

        public float TakeRest(Pawn pawn, float needed)
        {
            Need_Rest need = pawn.needs.rest;
            float taken = Mathf.Min(needed, need.CurLevel);
            HealthUtility.AdjustSeverity(pawn, HediffDefOfLocal.Hediff_AEFatigue, taken);
            need.CurLevel -= taken;
            return taken;
        }
    }
}
