using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class GameCondition_AETemperatureDrop : GameCondition_ForceWeather
    {
        private float tempOffset;
        public float MaxTempOffset = -100;

        public const int exitTicks = 10000;

        private SnowArmyMapComponent snowArmyMapComponent => snowArmyMmapComponentCached ?? (snowArmyMmapComponentCached = SingleMap?.GetComponent<SnowArmyMapComponent>() ?? null);
        private SnowArmyMapComponent snowArmyMmapComponentCached;

        private float TargetTempOffset
        {
            get
            {
                return snowArmyMapComponent.TempOffset;
            }
        }
        public override string Description
        {
            get
            {
                List<string> inspectStrings = new List<string>();
                inspectStrings.Add(base.Description);
                if (Prefs.DevMode && AEMod.Settings.DevModeInfo)
                {
                    inspectStrings.Add($"tempOffset ({tempOffset})\nTargetTempOffset ({TargetTempOffset})");
                }
                return String.Join("\n", inspectStrings);
            }
        }

        public override float TemperatureOffset()
        {
            if (!base.Permanent)
            {
                return Mathf.Lerp(0f, Mathf.Max(tempOffset - 10, MaxTempOffset), Mathf.Min(1f, (float)base.TicksLeft / (float)TransitionTicks));
            }
            return tempOffset;
        }

        public override void GameConditionTick()
        {
            float OutdoorTemp = SingleMap.mapTemperature.OutdoorTemp;
            if (OutdoorTemp > 20)
            {
                tempOffset += Mathf.Sign(TargetTempOffset - tempOffset) * 0.032f;
            }
            else if (OutdoorTemp > 0)
            {
                tempOffset += Mathf.Sign(TargetTempOffset - tempOffset) * 0.016f;
            }
            else if (OutdoorTemp > -10)
            {
                tempOffset += Mathf.Sign(TargetTempOffset - tempOffset) * 0.008f;
            }
            else if (OutdoorTemp > -80)
            {
                tempOffset += Mathf.Sign(TargetTempOffset - tempOffset) * 0.001f;
            }
            else
            {
                tempOffset += Mathf.Sign(TargetTempOffset - tempOffset) * 0.0005f;
            }
            if (Mathf.Abs(TargetTempOffset) < 1)
            {
                if (Mathf.Abs(tempOffset) < 1 && base.Permanent)
                {
                    base.Permanent = false;
                    Duration = (Find.TickManager.TicksGame - startTick) + exitTicks;
                }
            }
            else if (!base.Permanent)
            {
                base.Permanent = true;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref tempOffset, "tempOffset", 0f);
        }
    }
}
