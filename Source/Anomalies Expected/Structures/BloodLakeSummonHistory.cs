using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class BloodLakeSummonHistory : IExposable
    {
        public BloodLakeSummonPattern summonPattern;
        public string patternName;
        public int tickNextSummon;
        public int summonedTimes;

        public BloodLakeSummonHistory()
        {
            summonPattern = null;
            patternName = "";
            tickNextSummon = 0;
            summonedTimes = 0;
        }

        public BloodLakeSummonHistory(BloodLakeSummonPattern SummonPattern, int TickNextSummon, int SummonedTimes)
        {
            summonPattern = SummonPattern;
            patternName = summonPattern.name;
            tickNextSummon = TickNextSummon;
            summonedTimes = SummonedTimes;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref patternName, "patternName");
            Scribe_Values.Look(ref tickNextSummon, "tickNextSummon");
            Scribe_Values.Look(ref summonedTimes, "summonedTimes");
        }
    }
}
