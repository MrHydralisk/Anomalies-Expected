using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class CompPropertiesObelisk_Clockwork : CompProperties_Interactable
    {
        public List<TopOnBuildingStructure> topOnBuildingStructures;

        public int radius = 3;
        public int sizeLocation = 60;
        public float ticksFullRotationPerActiveTick = 2;
        public ThingDef MinuteHandZoneDef;
        public ThingDef SpeedometerDef;
        public ThingDef DecoySpeedometerDef;
        public ThingDef ClockworkPartDef;
        public DamageDef damageDef;
        public int damageAmountWave;
        public float DamageMultActive = 2;
        public float teleportationFleckRadius = 2f;
        public ResearchProjectDef fakeSpeedometerResearch;
        public EffecterDef EffecterOnActive;
        public SoundDef SoundActive;
        public EffecterDef EffecterOnPassive;
        public SoundDef SoundPassive;

        public CompPropertiesObelisk_Clockwork()
        {
            compClass = typeof(CompObelisk_Clockwork);
        }
    }
}
