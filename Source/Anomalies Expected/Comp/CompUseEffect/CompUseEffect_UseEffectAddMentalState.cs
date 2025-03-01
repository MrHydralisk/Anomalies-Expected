using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompUseEffect_UseEffectAddMentalState : CompUseEffect
    {
        public CompProperties_UseEffectAddMentalState Props => (CompProperties_UseEffectAddMentalState)props;

        public override void DoEffect(Pawn user)
        {
            user.mindState.mentalStateHandler.TryStartMentalState(Props.mentalStateDef, forced: true);
        }
    }
}
