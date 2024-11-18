﻿using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class Comp_BloodSource : ThingComp
    {
        public CompProperties_BloodSource Props => (CompProperties_BloodSource)props;

        public List<ThingWithComps> bloodPumps = new List<ThingWithComps>();
        public bool isCanAdd => bloodPumps.Count() < Props.MaxPumps;

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            foreach (ThingWithComps bloodPump in bloodPumps)
            {
                bloodPump.GetComp<Comp_BloodPump>().LostSource();
            }
            base.PostDestroy(mode, previousMap);
        }

        public void AddPump(ThingWithComps bloodPump)
        {
            bloodPumps.Add(bloodPump);
        }

        public void RemovePump(ThingWithComps bloodPump)
        {
            bloodPumps.Remove(bloodPump);
        }

        public override void PostExposeData()
        {
            Scribe_Collections.Look(ref bloodPumps, "bloodPumps", LookMode.Reference);
        }

        public override string CompInspectStringExtra()
        {
            TaggedString taggedString;
            if (Props.UnlockedByResearchDef.IsFinished)
            {
                taggedString += "AnomaliesExpected.BloodPump.Available".Translate(bloodPumps.Count(), Props.MaxPumps);
            }
            return taggedString.Resolve();
        }
    }
}
