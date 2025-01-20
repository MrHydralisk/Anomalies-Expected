using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class GameCondition_AEBloodFog : GameCondition_ForceWeather
    {
        private int ticksPerEffect = 250;
        public float addedSeverity = 0.007f;

        public override void Init()
        {
            if (!ModLister.AnomalyInstalled)
            {
                End();
            }
            else
            {
                base.Init();
            }
        }

        public override void GameConditionTick()
        {
            base.GameConditionTick();
            if (Find.TickManager.TicksGame % ticksPerEffect == 0)
            {
                List<Map> affectedMaps = base.AffectedMaps;
                for (int i = 0; i < affectedMaps.Count; i++)
                {
                    Map map = affectedMaps[i];
                    List<Pawn> colonists = map.mapPawns.FreeColonistsAndPrisonersSpawned;
                    foreach (Pawn colonist in colonists)
                    {
                        if (colonist.RaceProps.Humanlike && (colonist.GetRoom()?.OutdoorsForWork ?? true))
                        {
                            HealthUtility.AdjustSeverity(colonist, HediffDefOfLocal.Hediff_AEBloodLiquidConcentration, addedSeverity);
                        }
                    }
                }
            }
        }
    }
}
