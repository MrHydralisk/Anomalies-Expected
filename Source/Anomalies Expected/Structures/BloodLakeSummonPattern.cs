using System.Collections.Generic;

namespace AnomaliesExpected
{

    public class BloodLakeSummonPattern
    {
        public string name;
        public int initialInterval = 60000;
        public bool assaultColony = false;
        public bool isRaid = false;
        public List<BloodLakeSummonPatternStage> stages = new List<BloodLakeSummonPatternStage>();
    }
}
