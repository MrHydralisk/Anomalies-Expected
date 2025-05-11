namespace AnomaliesExpected
{
    public class CompProperties_AbilitySpawnSnowArmy : CompProperties_AbilitySpawnSummon
    {
        public float snowRadius = 2.9f;

        public CompProperties_AbilitySpawnSnowArmy()
        {
            compClass = typeof(CompAbilityEffect_SpawnSnowArmy);
        }
    }
}
