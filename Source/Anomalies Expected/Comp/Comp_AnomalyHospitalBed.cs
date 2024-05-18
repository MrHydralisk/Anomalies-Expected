using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class Comp_AnomalyHospitalBed : ThingComp
    {
        public CompProperties_AnomalyHospitalBed Props => (CompProperties_AnomalyHospitalBed)props;

        protected CompStudiable Studiable => studiableCached ?? (studiableCached = parent.TryGetComp<CompStudiable>());
        private CompStudiable studiableCached;

        private List<Tuple<string, float>> storedSeverityList = new List<Tuple<string, float>>();
        private float combinedStoredSeverity => storedSeverityList.Sum(e => e.Item2);

        Building_Bed Bed => parent as Building_Bed;

        public override void CompTickRare()
        {
            base.CompTickRare();
            List<Pawn> BedPawns = Bed.CurOccupants.ToList();
            if (BedPawns.Count > 0)
            {
                for (int i = 0; i < BedPawns.Count; i++)
                {
                    Pawn BedPawn = BedPawns[i];
                    if (Rand.Range(1, Props.ClipboardSize) <= storedSeverityList.Count)
                    {
                        Consume(BedPawn);
                    }
                    else if (BedPawn.health.HasHediffsNeedingTend())
                    {
                        Heal(BedPawn);
                    }
                }
            }
        }

        public void Consume(Pawn pawn)
        {
            float Consumed = 0;
            int Missed = 0;
            while (combinedStoredSeverity > 0 && Missed < Props.MaxMissed && !pawn.Dead)
            {
                Tuple<string, float> tuple = storedSeverityList.FirstOrDefault();
                float severityDealt = Mathf.Min(tuple.Item2, Props.MaxDamage);
                DamageWorker.DamageResult damageResult = pawn.TakeDamage(new DamageInfo(Rand.RangeInclusive(0, 3) switch
                {
                    0 => DamageDefOf.Blunt,
                    1 => DamageDefOf.Stab,
                    2 => DamageDefOf.Scratch,
                    3 => DamageDefOf.Cut,
                    _ => null,
                }, severityDealt, instigator: this.parent));
                severityDealt = damageResult.totalDamageDealt;
                if (severityDealt == 0)
                {
                    Missed++;
                }
                float severityLeft = tuple.Item2 - severityDealt;
                if (severityLeft > 0)
                {
                    storedSeverityList.Replace(tuple, new Tuple<string, float>(tuple.Item1, severityLeft));
                }
                else
                {
                    storedSeverityList.Remove(tuple);
                }
                Consumed += severityDealt;
            }
            if (Consumed > 0)
            {
                storedSeverityList.SortByDescending(e => e.Item2);
                Studiable.Study(pawn, 0, Consumed * Props.StudyConsumeMult);
            }
        }

        public void Heal(Pawn pawn)
        {
            float severityHealed = 0;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int j = 0; j < hediffs.Count; j++)
            {
                Hediff hediff = hediffs[j];
                float severityToHeal = hediff.Severity;
                if (hediff.TendableNow())
                {
                    hediff.Tended(Props.TendQuality, Props.TendQuality);
                }
                if (hediff is Hediff_Injury)
                {
                    hediff.Heal(severityToHeal);
                    severityHealed += severityToHeal * Props.MultInjury;
                }
                else if (hediff.def.isInfection)
                {
                    hediff.Heal(severityToHeal);
                    severityHealed += severityToHeal * Props.MultInfection;
                }
                else if (hediff.def == HediffDefOf.BloodLoss)
                {
                    hediff.Heal(severityToHeal);
                    severityHealed += severityToHeal * Props.MultBloodLoss;
                }
            }
            if (severityHealed > 0)
            {
                Tuple<string, float> tuple = storedSeverityList.FirstOrDefault();
                storedSeverityList.Add(Tuple.Create(pawn.LabelCap, severityHealed));
                storedSeverityList.SortByDescending(e => e.Item2);
                Studiable.Study(pawn, 0, severityHealed * Props.StudyHealMult);
            }
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
                        Pawn BedPawn = Bed.CurOccupants?.First();
                        if (BedPawn != null)
                        {
                            Heal(BedPawn);
                        }
                    },
                    defaultLabel = "Dev: Heal now",
                    defaultDesc = "Try to heal Pawn"
                };
                yield return new Command_Action
                {
                    action = delegate
                    {
                        Pawn BedPawn = Bed.CurOccupants?.First();
                        if (BedPawn != null)
                        {
                            Consume(BedPawn);
                        }
                    },
                    defaultLabel = "Dev: Consume now",
                    defaultDesc = "Try to consume Pawn"
                };
                yield return new Command_Action
                {
                    action = delegate
                    {
                        if (storedSeverityList.Count > 0)
                        {
                            storedSeverityList.RemoveAt(0);
                        }
                    },
                    defaultLabel = "Dev: Remove row",
                    defaultDesc = "Remove row from clipboard"
                };
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref storedSeverityList, "storedSeverityList", LookMode.Value);
        }

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            for (int i = 0; i < Props.ClipboardSize; i++)
            {
                Tuple<string, float> tuple = storedSeverityList.ElementAtOrDefault(i);
                if (tuple == null)
                {
                    inspectStrings.Add($"{i + 1}. ____________ (___)");
                }
                else
                {
                    inspectStrings.Add($"{i + 1}. {tuple.Item1} ({tuple.Item2})");
                }
            }
            return String.Join("\n", inspectStrings);
        }
    }
}
