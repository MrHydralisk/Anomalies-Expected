namespace AnomaliesExpected
{
    public class TopOnBuilding_Clockwork : TopOnBuilding
    {
        public CompObelisk_Clockwork compObelisk_Clockwork;

        public bool isActive => compObelisk_Clockwork.ActivityComp.IsActive;

        public TopOnBuilding_Clockwork(TopOnBuildingStructure TopOnBuildingStructure) : base(TopOnBuildingStructure)
        {
        }
    }
}
