using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace AnomaliesExpected
{
    public class CompProperties_BloodLake : CompProperties_Interactable
    {
        public List<BloodLakeSummonPattern> summonPatterns = new List<BloodLakeSummonPattern>();
        public Color inactiveColor = new Color(0.392f, 0.117f, 0.117f);
        public Color activeColor = new Color(1, 0, 0);

        public CompProperties_BloodLake()
        {
            compClass = typeof(Comp_BloodLake);
        }
    }
}
