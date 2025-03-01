using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class Thought_AEMemory : Thought_Memory
    {
        public string LabelCapAddition;
        public string DescAddition;
        public override string LabelCap
        {
            get
            {
                string text = base.LabelCap;
                if (!LabelCapAddition.NullOrEmpty())
                {
                    text += LabelCapAddition;
                }
                return text;
            }
        }
        public override string Description
        {
            get
            {
                string text = base.Description;
                if (!DescAddition.NullOrEmpty())
                {
                    text += DescAddition;
                }
                return text;
            }
        }
        public override void ExposeData()
        {
            Scribe_Values.Look(ref LabelCapAddition, "LabelCapAddition");
            Scribe_Values.Look(ref DescAddition, "DescAddition");
            base.ExposeData();
        }
    }
}
