using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_MeatGrinder : CompProperties
    {
        public StorageSettings defaultStorageSettings;

        public CompProperties_MeatGrinder()
        {
            compClass = typeof(Comp_MeatGrinder);
        }
    }
}
