using RimWorld;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class Building_AEChristmasGiftBox : Building_Crate
    {
        public override int OpenTicks => (int)def.building.uninstallWork;

        public override void PostMake()
        {
            base.PostMake();
            DrawColor = new Color(Rand.Range(0, 1f), Rand.Range(0, 1f), Rand.Range(0, 1f));
        }

        public override void EjectContents()
        {
            base.EjectContents();
            Destroy();
        }
    }
}
