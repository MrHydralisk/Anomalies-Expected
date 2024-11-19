using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Comp_BloodPump : ThingComp
    {
        public CompProperties_BloodPump Props => (CompProperties_BloodPump)props;

        public CompPowerTrader PowerComp => powerCompCached ?? (powerCompCached = parent.TryGetComp<CompPowerTrader>());
        private CompPowerTrader powerCompCached;

        private int TickTillNextSpawn;
        protected float Radius => parent.def.specialDisplayRadius;

        private Sustainer sustainer;
        public ThingWithComps Source;
        public Comp_BloodSource SourceComp => sourceCompCached ?? (sourceCompCached = Source.GetComp<Comp_BloodSource>());
        private Comp_BloodSource sourceCompCached;
        public bool isConnected => Source != null;
        public bool isPowered => PowerComp?.PowerOn ?? true;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                if (!TryFindSource())
                {
                    LostSource();
                }
            }
        }

        public static Comp_BloodSource NearbyBloodSource(IntVec3 position, Map map, float radius)
        {
            return GenRadial.RadialCellsAround(position, radius, useCenter: true).SelectMany((IntVec3 iv3) => map.thingGrid.ThingsListAtFast(iv3)).Where((Thing t) => t is Building && t.HasComp<Comp_BloodSource>()).Select((Thing t) => t.TryGetComp<Comp_BloodSource>()).Where((Comp_BloodSource bloodSource) => bloodSource.isCanAdd).OrderBy((Comp_BloodSource bloodSource) => bloodSource.parent.Position.DistanceTo(position)).FirstOrDefault();
        }

        public bool TryFindSource()
        {
            Comp_BloodSource bloodSource = NearbyBloodSource(parent.Position, parent.Map, Radius);
            if (bloodSource != null)
            {
                Source = bloodSource.parent;
                SourceComp.AddPump(this.parent);
                TickTillNextSpawn = Props.TickPerSpawn;
                return true;
            }
            return false;
        }

        public override void CompTickRare()
        {
            if (isConnected && isPowered)
            {
                TickTillNextSpawn -= 250;
                if (TickTillNextSpawn <= 0)
                {
                    GenerateResource();
                    TickTillNextSpawn = Props.TickPerSpawn;
                }
                if (!Props.soundWorking.NullOrUndefined())
                {
                    if (sustainer == null || sustainer.Ended)
                    {
                        sustainer = Props.soundWorking.TrySpawnSustainer(SoundInfo.InMap(parent));
                    }
                    sustainer.Maintain();
                }
                else if (sustainer != null && !sustainer.Ended)
                {
                    sustainer.End();
                }
            }
        }

        public void GenerateResource()
        {
            Thing resource = ThingMaker.MakeThing(SourceComp.Props.ResourceDef);
            resource.stackCount = SourceComp.Props.ResourceAmount;
            GenPlace.TryPlaceThing(resource, parent.Position, parent.Map, ThingPlaceMode.Near, null);
        }

        public void LostSource()
        {
            Source = null;
            sourceCompCached = null;
            Messages.Message("AnomaliesExpected.BloodPump.Disconnected".Translate(parent.LabelCap).RawText, parent, MessageTypeDefOf.NeutralEvent);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            if (sustainer != null && !sustainer.Ended)
            {
                sustainer.End();
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (isConnected)
            {
                SourceComp.RemovePump(this.parent);
            }
            base.PostDestroy(mode, previousMap);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        GenerateResource();
                        TickTillNextSpawn = Props.TickPerSpawn;
                    },
                    defaultLabel = "Dev: Generate Resource",
                    defaultDesc = "Generate Resource from blood source"
                };
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref TickTillNextSpawn, "TickTillNextSpawn", Props.TickPerSpawn);
            Scribe_References.Look(ref Source, "Source");
        }

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            if (isConnected)
            {
                inspectStrings.Add("AnomaliesExpected.BloodPump.Connected".Translate(Source.LabelCap));
                inspectStrings.Add("AnomaliesExpected.BloodPump.TimeTillSpawn".Translate(TickTillNextSpawn.ToStringTicksToPeriod()));
            }
            else
            {
                inspectStrings.Add("AnomaliesExpected.BloodPump.NotConnected".Translate());
            }
            return String.Join("\n", inspectStrings);
        }
    }
}
