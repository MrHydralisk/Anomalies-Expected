﻿using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_AnomalyHospitalBed : CompProperties
    {
        public int ClipboardSize = 5;
        public int MaxMissed = 10;
        public int ticksPerSign = 10000;
        public float TendQuality = 1;
        public float MaxDamage = 5;
        public float MultInjury = 1;
        public float MultMissingPartSkin = 1;
        public float MultMissingPartOrgan = 4;
        public float MultInfection = 100;
        public float MultBloodLoss = 50;
        public float StudyHealMult = 0.005f;
        public float StudyConsumeMult = 0.015f;
        public float SeverityPerDmgDonorMult = 0.5f;
        public float ConsumptionAbsorbMult = 0.25f;

        public CompProperties_AnomalyHospitalBed()
        {
            compClass = typeof(Comp_AnomalyHospitalBed);
        }
    }
}
