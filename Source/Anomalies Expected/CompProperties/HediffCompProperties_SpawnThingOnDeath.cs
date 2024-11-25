using Verse;

namespace AnomaliesExpected
{
    public class HediffCompProperties_SpawnThingOnDeath : HediffCompProperties_LetterOnDeath
    {
        public ThingDef spawnedThingDef;

        [MustTranslate]
        public string messageText;

        public HediffCompProperties_SpawnThingOnDeath()
        {
            compClass = typeof(HediffComp_SpawnThingOnDeath);
        }
    }
}
