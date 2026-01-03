using Verse;

namespace AnomaliesExpected
{
    public class TopOnBuilding_Clockwork : TopOnBuilding
    {
        public Thing Obelisk_Clockwork;
        protected CompObelisk_Clockwork compObelisk_Clockwork => compObelisk_ClockworkCached ?? (compObelisk_ClockworkCached = Obelisk_Clockwork.TryGetComp<CompObelisk_Clockwork>());
        private CompObelisk_Clockwork compObelisk_ClockworkCached;

        public bool isActive => compObelisk_Clockwork.ActivityComp.IsActive;

        public override float ticksFullRotationPerTick => isActive ? compObelisk_Clockwork.Props.ticksFullRotationPerActiveTick : 1;

        public virtual void Rotate(float amount, bool isFromDamage = false)
        {
            if (ticksTillFullRotation > 0)
            {
                amount = amount * ticksFullRotationPerTick;
                if (isFromDamage)
                {
                    amount = amount * topOnBuildingStructure.rotationPerDmgMult;
                }
                ticksTillFullRotation -= amount;
                if (ticksTillFullRotation <= 0)
                {
                    OnTimerEnd();
                }
            }
        }

        public TopOnBuilding_Clockwork() : base()
        {
        }

        public TopOnBuilding_Clockwork(TopOnBuildingStructure TopOnBuildingStructure) : base(TopOnBuildingStructure)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref Obelisk_Clockwork, "Obelisk_Clockwork");
        }
    }
}
