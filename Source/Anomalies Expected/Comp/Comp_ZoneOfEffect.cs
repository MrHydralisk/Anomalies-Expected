using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    [StaticConstructorOnStartup]
    public class Comp_ZoneOfEffect : ThingComp
    {
        public CompProperties_ZoneOfEffect Props => (CompProperties_ZoneOfEffect)props;

        private int startTick;

        private int totalDuration;

        private int fadeOutDuration;

        public Thing instigator;

        public ThingDef weaponDef;

        private Sustainer sustainer;

        private int TicksPassed => Find.TickManager.TicksGame - startTick;

        private int TicksLeft => totalDuration - TicksPassed;

        private static List<Thing> tmpThings = new List<Thing>();

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            CheckSpawnSustainer();
        }

        public void StartAttack(Thing instigator, ThingDef weaponDef, int totalDuration = 6000, int fadeOutDuration = 300)
        {
            startTick = Find.TickManager.TicksGame;
            this.totalDuration = totalDuration;
            this.fadeOutDuration = fadeOutDuration;
            this.instigator = instigator;
            this.weaponDef = weaponDef;
            CheckSpawnSustainer();
            Mote obj = (Mote)ThingMaker.MakeThing(Props.mote);
            obj.exactPosition = parent.Position.ToVector3Shifted();
            obj.Scale = Props.radius * 6;
            obj.rotationRate = 1.2f;
            GenSpawn.Spawn(obj, parent.Position, parent.Map);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (TicksPassed % Props.ticksPerDamage == 0)
            {
                DealDamage();
            }
            if (TicksPassed >= totalDuration)
            {
                parent.Destroy();
            }
            if (sustainer != null)
            {
                sustainer.Maintain();
                if (TicksLeft < fadeOutDuration)
                {
                    sustainer.End();
                    sustainer = null;
                }
            }
        }

        private void CheckSpawnSustainer()
        {
            if (TicksLeft >= fadeOutDuration && Props.sound != null)
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    sustainer = Props.sound.TrySpawnSustainer(SoundInfo.InMap(parent, MaintenanceType.PerTick));
                });
            }
        }

        private void DealDamage()
        {
            Map map = parent.Map;
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(parent.Position, Props.radius, useCenter: true))
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }
                tmpThings.Clear();
                tmpThings.AddRange(cell.GetThingList(map));
                for (int i = 0; i < tmpThings.Count; i++)
                {
                    int num = Props.DamageAmount;
                    Pawn pawn = tmpThings[i] as Pawn;
                    BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = null;
                    if (pawn != null)
                    {
                        if (pawn.DeadOrDowned)
                        {
                            num = Props.DamageAmountDowned;
                        }
                        battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_PowerBeam, instigator as Pawn);
                        Find.BattleLog.Add(battleLogEntry_DamageTaken);
                    }
                    tmpThings[i].TakeDamage(new DamageInfo(Props.damageDef, num, instigator: instigator, weapon: weaponDef)).AssociateWithLog(battleLogEntry_DamageTaken);
                }
                tmpThings.Clear();
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref startTick, "startTick", 0);
            Scribe_Values.Look(ref totalDuration, "totalDuration", 0);
            Scribe_Values.Look(ref fadeOutDuration, "fadeOutDuration", 0);
            Scribe_References.Look(ref instigator, "instigator");
            Scribe_Defs.Look(ref weaponDef, "weaponDef");
        }
    }
}
