using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_DogTag : CompProperties_Interactable
    {
        public HediffDef hediffOnUse;

        public CompProperties_DogTag()
        {
            compClass = typeof(Comp_DogTag);
        }
    }
}
