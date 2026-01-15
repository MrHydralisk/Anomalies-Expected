using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_ZoneOfEffect : CompProperties
    {
        public ThingDef mote;

        public DamageDef damageDef;

        public SoundDef sound;

        public float radius = 5f;

        public int ticksPerDamage = 50;

        public int DamageAmount = 1;

        public EffecterDef effecterOnDamage;

        public SoundDef soundOnDamage;

        public CompProperties_ZoneOfEffect()
        {
            compClass = typeof(Comp_ZoneOfEffect);
        }
    }
}
