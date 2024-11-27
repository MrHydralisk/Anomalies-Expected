using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class HediffComp_WoundClotting : HediffComp
    {
        private const int ClotCheckInterval = 1250;

        private static readonly FloatRange TendingQualityRange = new FloatRange(0.4f, 0.6f);

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (!parent.pawn.IsHashIntervalTick(ClotCheckInterval))
            {
                return;
            }
            List<Hediff> hediffs = parent.pawn.health.hediffSet.hediffs;
            for (int num = hediffs.Count - 1; num >= 0; num--)
            {
                if (hediffs[num].Bleeding)
                {
                    hediffs[num].Tended(TendingQualityRange.RandomInRange, TendingQualityRange.TrueMax, 1);
                }
            }
        }
    }
}
