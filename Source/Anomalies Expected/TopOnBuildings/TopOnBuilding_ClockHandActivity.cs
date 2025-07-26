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
            if (tickTillFullRotation > 0)
            {
                tickTillFullRotation -= 1;
                CurRotation = 360 * (1 - tickTillFullRotation / topOnBuildingStructure.tickPerFullRotation) + InitialRotation;
                if (tickTillFullRotation <= 0)
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
