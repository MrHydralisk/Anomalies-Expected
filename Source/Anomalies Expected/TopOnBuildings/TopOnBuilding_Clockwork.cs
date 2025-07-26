namespace AnomaliesExpected
{
    public class TopOnBuilding_Clockwork : TopOnBuilding
    {
        public CompObelisk_Clockwork compObelisk_Clockwork;

        public bool isActive => compObelisk_Clockwork.ActivityComp.IsActive;

        public override float ticksFullRotationPerTick => isActive ? compObelisk_Clockwork.Props.ticksFullRotationPerActiveTick : 1;

        public TopOnBuilding_Clockwork(TopOnBuildingStructure TopOnBuildingStructure) : base(TopOnBuildingStructure)
        {
        }
    }
}
