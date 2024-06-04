using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_BeamTarget : CompProperties_Interactable
    {
        public int beamMaxCount = 5;
        public int beamDuration = 2500;
        public int beamSubRadius = 10;
        public int ticksPerBeamActivationPreparation = 5000;
        public int ticksWhenCarried = 500;
        public float teleportationFleckRadius = 0.4f;
        public IntRange beamIntervalRange = new IntRange(60000, 180000);

        public CompProperties_BeamTarget()
        {
            compClass = typeof(Comp_BeamTarget);
        }
    }
}
