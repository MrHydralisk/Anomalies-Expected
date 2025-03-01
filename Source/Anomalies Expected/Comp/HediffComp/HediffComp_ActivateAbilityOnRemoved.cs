using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class HediffComp_ActivateAbilityOnRemoved : HediffComp
    {
        public void ActivateDestabilization()
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
            ActivateDestabilization();
            base.CompPostPostRemoved();
        }
    }
}
