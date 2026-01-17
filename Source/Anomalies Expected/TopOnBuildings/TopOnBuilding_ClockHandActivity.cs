using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class TopOnBuilding_ClockHandActivity : TopOnBuilding_Clockwork
    {

        public TopOnBuilding_ClockHandActivity() : base()
        {
        }

        public TopOnBuilding_ClockHandActivity(TopOnBuildingStructure TopOnBuildingStructure) : base(TopOnBuildingStructure)
        {
        }

        public override void Tick()
        {
            if (ticksTillFullRotation > 0)
            {
                ticksTillFullRotation -= compObelisk_Clockwork.ActivityComp.ActivityResearchFactor;
                CurRotation = 360 * (1 - ticksTillFullRotation / topOnBuildingStructure.tickPerFullRotation) + InitialRotation;
                if (ticksTillFullRotation <= 0)
                {
                    OnTimerEnd();
                }
            }
        }

        public override void OnTimerEnd()
        {
            if (!isActive)
            {
                compObelisk_Clockwork.ActivityComp.EnterActiveState();
                Find.TickManager.slower.SignalForceNormalSpeedShort();
                if (AEMod.Settings.NotifyClockworkHandDay)
                {
                    Messages.Message("AnomaliesExpected.ObeliskClockwork.HandDay.Aiming".Translate(), new TargetInfo(Obelisk_Clockwork.Position, Obelisk_Clockwork.Map), MessageTypeDefOf.NegativeEvent);
                }
            }
        }
    }
}
