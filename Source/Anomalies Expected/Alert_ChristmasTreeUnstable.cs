using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{

    public class Alert_ChristmasTreeUnstable : Alert_Critical
    {
        private static List<Thing> targets = new List<Thing>();

        private Building_AEChristmasTreeExit ChristmasTree => targets.FirstOrDefault() as Building_AEChristmasTreeExit;

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
            targets = new List<Thing>();
        }

        public static void AddTarget(Building_AEChristmasTreeExit christmasTree)
        {
            if (targets.All((Thing t) => t.ThingID != christmasTree.ThingID))
            {
                targets.Add(christmasTree);
            }
        }

        public static void RemoveTarget(Building_AEChristmasTreeExit christmasTree)
        {
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                if (targets[i].ThingID == christmasTree.ThingID)
                {
                    targets.RemoveAt(i);
                }
            }
        }

        public override string GetLabel()
        {
            return defaultLabel + ": " + ChristmasTree?.mapComponent?.TickTillDestroy.ToStringTicksToPeriodVerbose();
        }

        public override AlertReport GetReport()
        {
            if (targets.NullOrEmpty() || ChristmasTree.DestroyedOrNull() || !ChristmasTree.Spawned)
            {
                return false;
            }
            return AlertReport.CulpritsAre(targets);
        }
    }
}
