using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Comp_AtmosphericCooler : CompTempControl, IThingGlower
    {
        private class Command_GroupedTempChange : Command_Action
        {
            private Comp_AtmosphericCooler cooler;

            private float offset;

            public Command_GroupedTempChange(Comp_AtmosphericCooler cooler, float offset)
            {
                this.cooler = cooler;
                this.offset = offset;
            }

            public override void ProcessInput(Event ev)
            {
            }

            public override void ProcessGroupInput(Event ev, List<Gizmo> group)
            {
                cooler.InterfaceChangeTargetTemperature_NewTemp(offset);
            }
        }

        private CompPowerTrader powerTraderComp;

        private CompHeatPusher heatPusherComp;

        private CompRefuelable refuelableComp;

        private GameCondition_UnnaturalCold Condition
        {
            get
            {
                GameCondition_UnnaturalCold gameCondition_UnnaturalCold = parent.Map.gameConditionManager.GetActiveCondition(GameConditionDefOfLocal.AE_UnnaturalCold) as GameCondition_UnnaturalCold;
                if (gameCondition_UnnaturalCold == null)
                {
                    gameCondition_UnnaturalCold = (GameCondition_UnnaturalCold)GameConditionMaker.MakeCondition(GameConditionDefOfLocal.AE_UnnaturalCold);
                    parent.Map.GameConditionManager.RegisterCondition(gameCondition_UnnaturalCold);
                    gameCondition_UnnaturalCold.Permanent = true;
                }
                return gameCondition_UnnaturalCold;
            }
        }

        public override float TargetTemperature
        {
            get
            {
                return Condition.coolerTargetTemp;
            }
            set
            {
                Condition.coolerTargetTemp = value;
            }
        }

        private bool Powered => powerTraderComp.PowerOn;

        public int TempOffset
        {
            get
            {
                if (!Powered || !(TargetTemperature < parent.Map.mapTemperature.OutdoorTemp))
                {
                    return 0;
                }
                return -10;
            }
        }

        private bool Working
        {
            get
            {
                if (Powered && refuelableComp.HasFuel)
                {
                    return TargetTemperature < parent.Map.mapTemperature.OutdoorTemp + 1f;
                }
                return false;
            }
        }

        public bool ShouldBeLitNow()
        {
            return Working;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!ModLister.CheckAnomaly("Atmospheric heater"))
            {
                parent.Destroy();
                return;
            }
            base.PostSpawnSetup(respawningAfterLoad);
            powerTraderComp = parent.GetComp<CompPowerTrader>();
            heatPusherComp = parent.GetComp<CompHeatPusher>();
            refuelableComp = parent.GetComp<CompRefuelable>();
        }

        public override void CompTick()
        {
            if (Working)
            {
                powerTraderComp.PowerOutput = 0f - powerTraderComp.Props.PowerConsumption;
                refuelableComp.Notify_UsedThisTick();
                heatPusherComp.enabled = true;
            }
            else
            {
                powerTraderComp.PowerOutput = (0f - powerTraderComp.Props.PowerConsumption) * base.Props.lowPowerConsumptionFactor;
                heatPusherComp.enabled = false;
            }
            int num = (Working ? 1 : 0);
            if (parent.overrideGraphicIndex != num)
            {
                parent.overrideGraphicIndex = num;
                parent.DirtyMapMesh(parent.Map);
                parent.TryGetComp<CompGlower>()?.UpdateLit(parent.Map);
            }
            operatingAtHighPower = Working;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            float num = RoundedToCurrentTempModeOffset_NewTemp(-10f);
            Command_GroupedTempChange command_GroupedTempChange = new Command_GroupedTempChange(this, num);
            command_GroupedTempChange.defaultLabel = num.ToStringTemperatureOffset("F0");
            command_GroupedTempChange.defaultDesc = "CommandLowerTempDesc".Translate();
            command_GroupedTempChange.hotKey = KeyBindingDefOf.Misc5;
            command_GroupedTempChange.icon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower");
            yield return command_GroupedTempChange;
            float num2 = RoundedToCurrentTempModeOffset_NewTemp(-1f);
            Command_GroupedTempChange command_GroupedTempChange2 = new Command_GroupedTempChange(this, num2);
            command_GroupedTempChange2.defaultLabel = num2.ToStringTemperatureOffset("F0");
            command_GroupedTempChange2.defaultDesc = "CommandLowerTempDesc".Translate();
            command_GroupedTempChange2.hotKey = KeyBindingDefOf.Misc4;
            command_GroupedTempChange2.icon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower");
            yield return command_GroupedTempChange2;
            Command_Action command_Action = new Command_Action();
            command_Action.action = delegate
            {
                TargetTemperature = 21f;
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                ThrowCurrentTemperatureText_NewTemp();
            };
            command_Action.defaultLabel = "CommandResetTemp".Translate();
            command_Action.defaultDesc = "CommandResetTempDesc".Translate();
            command_Action.hotKey = KeyBindingDefOf.Misc1;
            command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/TempReset");
            yield return command_Action;
            float num3 = RoundedToCurrentTempModeOffset_NewTemp(1f);
            Command_GroupedTempChange command_GroupedTempChange3 = new Command_GroupedTempChange(this, num3);
            command_GroupedTempChange3.defaultLabel = "+" + num3.ToStringTemperatureOffset("F0");
            command_GroupedTempChange3.defaultDesc = "CommandRaiseTempDesc".Translate();
            command_GroupedTempChange3.hotKey = KeyBindingDefOf.Misc2;
            command_GroupedTempChange3.icon = ContentFinder<Texture2D>.Get("UI/Commands/TempRaise");
            yield return command_GroupedTempChange3;
            float num4 = RoundedToCurrentTempModeOffset_NewTemp(10f);
            Command_GroupedTempChange command_GroupedTempChange4 = new Command_GroupedTempChange(this, num4);
            command_GroupedTempChange4.defaultLabel = "+" + num4.ToStringTemperatureOffset("F0");
            command_GroupedTempChange4.defaultDesc = "CommandRaiseTempDesc".Translate();
            command_GroupedTempChange4.hotKey = KeyBindingDefOf.Misc3;
            command_GroupedTempChange4.icon = ContentFinder<Texture2D>.Get("UI/Commands/TempRaise");
            yield return command_GroupedTempChange4;
        }
    }
}
