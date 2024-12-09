using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class Recipe_AEInitiateFleshmassMutation : Recipe_Surgery
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, delegate (BodyPartRecord record)
            {
                if (!pawn.health.hediffSet.GetNotMissingParts().Contains(record))
                {
                    return false;
                }
                if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record))
                {
                    return false;
                }
                HediffDef mutation = null;
                switch (recipe.displayPriority)
                {
                    case 1300:
                        {
                            mutation = HediffDefOfLocal.Hediff_AEFleshmassCystAssimilation;
                            break;
                        }
                    case 1301:
                        {
                            mutation = HediffDefOfLocal.Hediff_AEFleshmassTumorAssimilation;
                            break;
                        }
                }
                return (!pawn.health.hediffSet.hediffs.Any((Hediff x) => x.Part == record && ((x.def == recipe.addsHediff && (!(x is Hediff_Level hl) || (hl.level >= x.def.maxSeverity))) || ((x.def == mutation || x.def == HediffDefOfLocal.Hediff_AEFleshmassPartRestoration) && ((x as HediffWithComps).GetComp<HediffComp_FleshmassMutation>()?.hediffToAdd ?? null) == recipe.addsHediff) || !recipe.CompatibleWithHediff(x.def)))) ? true : false;
            });
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            }
            HediffDef mutation = null;
            switch (recipe.displayPriority)
            {
                case 1300:
                    {
                        mutation = HediffDefOfLocal.Hediff_AEFleshmassCystAssimilation;
                        break;
                    }
                case 1301:
                    {
                        mutation = HediffDefOfLocal.Hediff_AEFleshmassTumorAssimilation;
                        break;
                    }
            }
            if (mutation == null)
            {
                Log.Error("Proper displayPriority not found for Recipe_AEInitiateFleshmassMutation");
                return;
            }
            HediffWithComps hediff = pawn.health.AddHediff(mutation, part) as HediffWithComps;
            HediffComp_FleshmassMutation hediffComp_FleshmassMutation = hediff.GetComp<HediffComp_FleshmassMutation>();
            if (hediffComp_FleshmassMutation == null)
            {
                Log.Error("HediffComp_FleshmassMutation not found for Recipe_AEInitiateFleshmassMutation");
            }
            else
            {
                hediffComp_FleshmassMutation.hediffToAdd = recipe.addsHediff;
            }
        }
    }
}
