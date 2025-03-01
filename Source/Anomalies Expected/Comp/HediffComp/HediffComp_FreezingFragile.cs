using Verse;

namespace AnomaliesExpected
{
    public class HediffComp_FreezingFragile : HediffComp
    {
        public override string CompLabelInBracketsExtra => $"{(parent.Severity).ToStringPercent()}";
    }
}
