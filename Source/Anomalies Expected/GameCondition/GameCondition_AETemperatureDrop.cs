using RimWorld;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class GameCondition_AETemperatureDrop : GameCondition
    {
        public float MaxTempOffset = -80;

        public override int TransitionTicks => translationTicks;
        public int translationTicks = 120000;

        public override float TemperatureOffset()
        {
            //Log.Message($"GameConditionUtility.LerpInOutValue(this, {TransitionTicks}, {MaxTempOffset}) = {GameConditionUtility.LerpInOutValue(this, TransitionTicks, MaxTempOffset)} | {this.TicksPassed} | {this.LetterText}");
            //Log.Message($"Mathf.Lerp(0, {MaxTempOffset}, {Mathf.Clamp((float)this.TicksPassed / translationTicks, 0, 1)}) = {Mathf.Lerp(0, MaxTempOffset, Mathf.Clamp((float)this.TicksPassed / translationTicks, 0, 1))}");
            //return GameConditionUtility.LerpInOutValue(this, TransitionTicks, MaxTempOffset);
            return Mathf.Lerp(0, MaxTempOffset, Mathf.Clamp((float)this.TicksPassed / translationTicks, 0, 1));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref MaxTempOffset, "MaxTempOffset", -80);
            Scribe_Values.Look(ref translationTicks, "translationTicks", 120000);
        }
    }
}
