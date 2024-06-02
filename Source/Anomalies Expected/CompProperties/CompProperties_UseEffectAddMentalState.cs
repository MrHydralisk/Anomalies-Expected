using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_UseEffectAddMentalState : CompProperties_UseEffect
    {
        public MentalStateDef mentalStateDef;

        public CompProperties_UseEffectAddMentalState()
        {
            compClass = typeof(CompUseEffect_UseEffectAddMentalState);
        }
    }
}
