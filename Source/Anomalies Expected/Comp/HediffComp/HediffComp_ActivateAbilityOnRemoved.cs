using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class HediffComp_ActivateAbilityOnRemoved : HediffComp
    {
        public void ActivateDistabilization()
        {
            foreach (Ability ability in parent.AllAbilitiesForReading)
            {
                if (ability.CanCast)
                {
                    ability.Activate(Pawn);
                }
            }
        }

        public override void CompPostPostRemoved()
        {
            ActivateDistabilization();
            base.CompPostPostRemoved();
        }
    }
}
