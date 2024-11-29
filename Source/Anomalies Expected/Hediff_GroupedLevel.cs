using RimWorld;
using System.Text;
using Verse;
using Verse.Noise;

namespace AnomaliesExpected
{
    public class Hediff_GroupedLevel : Hediff_Level
    {
        public override void Tick()
        {
            base.Tick();
            if (pawn.IsHashIntervalTick(2500))
            {
                CalculateGroupedHeddifs();
            }
        }

        public void CalculateGroupedHeddifs()
        {
            SetLevelTo(1 + pawn.health.hediffSet.hediffs.Count((Hediff h) => h.def.tags?.Contains("FleshmassBodyMutation") ?? false));
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            CalculateGroupedHeddifs();
        }

        public override void Notify_Spawned()
        {
            base.Notify_Spawned();
            CalculateGroupedHeddifs();
        }
    }
}
