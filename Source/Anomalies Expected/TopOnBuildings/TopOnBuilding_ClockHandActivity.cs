using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class TopOnBuilding_ClockHandActivity : TopOnBuilding_Clockwork
    {
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
            }
        }
    }
}
