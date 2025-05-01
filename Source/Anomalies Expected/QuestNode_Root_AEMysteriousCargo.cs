using RimWorld.QuestGen;
using Verse;

namespace AnomaliesExpected
{
    public class QuestNode_Root_AEMysteriousCargo : QuestNode_Root_MysteriousCargo
    {
        public ThingDef DeployableObjectDef;

        protected override Thing GenerateThing(Pawn _)
        {
            return ThingMaker.MakeThing(DeployableObjectDef);
        }
    }
}
