using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompTargetable_SinglePawnUseFirstEffect : CompTargetable_SinglePawn
    {

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!base.ValidateTarget(target, showMessages))
            {
                return false;
            }
            CompUseEffect compUseEffect = parent.TryGetComp<CompUseEffect>();
            if (compUseEffect == null || !compUseEffect.CanBeUsedBy(target.Pawn))
            {
                return false;
            }
            return true;
        }
    }
}
