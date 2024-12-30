using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{

    public class Alert_ChristmasTreeUnstable : Alert_Critical
    {
        private static List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

        private Building_AEChristmasTreeExit ChristmasTree => targets.FirstOrDefault().Thing as Building_AEChristmasTreeExit;

        protected override Color BGColor
        {
            get
            {
                ChristmasTreeMapComponent christmasTreeMapComponent = ChristmasTree?.mapComponent;
                if (christmasTreeMapComponent != null && christmasTreeMapComponent.TickTillDestroy > 60000)
                {
                    return Color.clear;
                }
                return base.BGColor;
            }
        }

        public Alert_ChristmasTreeUnstable()
        {
            defaultLabel = "AnomaliesExpected.ChristmasStockings.AlertSubMap.Label".Translate();
            defaultExplanation = "AnomaliesExpected.ChristmasStockings.AlertSubMap.Desc".Translate();
            requireAnomaly = true;
        }

        public static void AddTarget(Building_AEChristmasTreeExit christmasTree)
        {
            if (targets.IndexOf(christmasTree) == -1)
            {
                targets.Add(christmasTree);
            }
        }

        public static void RemoveTarget(Building_AEChristmasTreeExit christmasTree)
        {
            int index = targets.IndexOf(christmasTree);
            if (index >= 0)
            {
                targets.RemoveAt(index);
            }
        }

        public override string GetLabel()
        {
            return defaultLabel + ": " + ChristmasTree?.mapComponent?.TickTillDestroy.ToStringTicksToPeriodVerbose();
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(targets);
        }
    }
}
