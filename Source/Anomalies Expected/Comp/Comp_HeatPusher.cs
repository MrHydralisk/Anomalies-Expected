using Verse;

namespace AnomaliesExpected
{
    public class Comp_HeatPusher : CompHeatPusher
    {
        public bool isNegative;
        public float Energy => isNegative ? -Props.heatPerSecond : Props.heatPerSecond;

        public override void CompTick()
        {
            if (parent.IsHashIntervalTick(60) && ShouldPushHeatNow)
            {
                GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, Energy);
            }
        }

        public override void CompTickRare()
        {
            if (ShouldPushHeatNow)
            {
                GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, Energy * 4.1666665f);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isNegative, "isNegative", defaultValue: false);
        }
    }
}
