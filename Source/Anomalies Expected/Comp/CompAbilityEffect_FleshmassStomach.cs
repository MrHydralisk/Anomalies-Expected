using RimWorld;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class CompAbilityEffect_FleshmassStomach : CompAbilityEffect
    {
        public new CompProperties_AbilityFleshmassStomach Props => (CompProperties_AbilityFleshmassStomach)props;

        private Pawn pawn => parent.pawn;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            pawn.needs.food.CurLevel += Props.nutAmount;
            pawn.records.AddTo(RecordDefOf.NutritionEaten, Props.nutAmount);
            DamageInfo dmg = new DamageInfo(DamageDefOf.AcidBurn, Props.damAmount, hitPart: pawn.def.race.body.GetPartsWithDef(BodyPartDefOfLocal.Stomach).First());
            dmg.SetAllowDamagePropagation(false);
            dmg.SetIgnoreArmor(true);
            pawn.TakeDamage(dmg);            
        }
    }
}
