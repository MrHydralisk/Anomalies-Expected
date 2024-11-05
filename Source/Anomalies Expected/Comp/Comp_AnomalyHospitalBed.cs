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

        protected CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = parent.TryGetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

        private List<SeverityRecord> storedSeverityList = new List<SeverityRecord>();
        private float combinedStoredSeverity => (storedSeverityList ?? (storedSeverityList = new List<SeverityRecord>())).Sum(sr => sr.Severity);

        Building_Bed Bed => parent as Building_Bed;

        private int tickSign;

        public bool canSign => Find.TickManager.TicksGame - tickSign > Props.ticksPerSign;
        public bool isDonorMode;

        public void Sign()
        {
            List<Pawn> BedPawns = Bed.CurOccupants.ToList();
            if (BedPawns.Count > 0)
            {
                for (int i = 0; i < BedPawns.Count; i++)
                {
                    Pawn BedPawn = BedPawns[i];
                    if (isDonorMode || Rand.Range(1, Props.ClipboardSize) <= storedSeverityList.Count)
                    {
                        Consume(BedPawn);
                    }
                    else
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
                SeverityRecord severityRecord = storedSeverityList.FirstOrDefault();
                float severityDealt = Mathf.Min(severityRecord.Severity, Props.MaxDamage);
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
                if (isDonorMode)
                {
                    severityDealt *= Props.SeverityPerDmgDonorMult;
                }
                float severityLeft = severityRecord.Severity - severityDealt;
                if (severityLeft > 0)
                {
                    storedSeverityList.Replace(severityRecord, new SeverityRecord(severityRecord.Name, severityLeft));
                }
                else
                {
                    storedSeverityList.Remove(severityRecord);
                }
                Consumed += severityDealt;
            }
            if (Consumed > 0)
            {
                storedSeverityList.SortByDescending(sr => sr.Severity);
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
                storedSeverityList.Add(new SeverityRecord(pawn.LabelCap, severityHealed));
                storedSeverityList.SortByDescending(sr => sr.Severity);
                Studiable.Study(pawn, 0, severityHealed * Props.StudyHealMult);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            yield return new Command_Action
            {
                action = delegate
                {
                    Sign();
                    tickSign = Find.TickManager.TicksGame;
                },
                defaultLabel = "AnomaliesExpected.AnomalyHospitalBed.Sign.Label".Translate(),
                defaultDesc = "AnomaliesExpected.AnomalyHospitalBed.Sign.Desc" .Translate(),
                icon = parent.def.uiIcon,
                hotKey = KeyBindingDefOf.Misc6,
                Disabled = !canSign || ((Bed.CurOccupants?.Count() ?? 0) == 0),
                disabledReason = canSign ? "AnomaliesExpected.AnomalyHospitalBed.Sign.Empty".Translate() : "AnomaliesExpected.AnomalyHospitalBed.Sign.Reloading".Translate((tickSign + Props.ticksPerSign - Find.TickManager.TicksGame).ToStringTicksToPeriod())

            };
            if ((StudyUnlocks?.NextIndex ?? 5) >= 5)
            {
                Command_Toggle command_Toggle = new Command_Toggle();
                command_Toggle.defaultLabel = "AnomaliesExpected.AnomalyHospitalBed.isDonor.Label".Translate();
                command_Toggle.defaultDesc = "AnomaliesExpected.AnomalyHospitalBed.isDonor.Desc".Translate();
                command_Toggle.isActive = () => isDonorMode;
                command_Toggle.toggleAction = delegate
                {
                    isDonorMode = !isDonorMode;
                };
                command_Toggle.activateSound = SoundDefOf.Tick_Tiny;
                command_Toggle.icon = parent.def.uiIcon;
                command_Toggle.defaultIconColor = isDonorMode ? Color.red : Color.white;
                command_Toggle.hotKey = KeyBindingDefOf.Misc5;
                yield return command_Toggle;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        Sign();
                    },
                    defaultLabel = "Dev: Sign now",
                    defaultDesc = $"Put a signature [Can = {(Bed.CurOccupants?.Count() ?? 0) > 0}]"
                };
                yield return new Command_Action
                {
                    action = delegate
                    {
                        Pawn BedPawn = Bed.CurOccupants?.FirstOrDefault();
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
                        Pawn BedPawn = Bed.CurOccupants?.FirstOrDefault();
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
            Scribe_Collections.Look(ref storedSeverityList, "storedSeverityList", LookMode.Deep);
            storedSeverityList.RemoveAll(sr => sr == null || sr.Severity == 0);
            Scribe_Values.Look(ref tickSign, "tickSign", Find.TickManager.TicksGame);
            Scribe_Values.Look(ref isDonorMode, "isDonorMode", false);
        }

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            for (int i = 0; i < Props.ClipboardSize; i++)
            {
                SeverityRecord severityRecord = storedSeverityList.ElementAtOrDefault(i);
                if (severityRecord == null)
                {
                    inspectStrings.Add($"{i + 1}. ____________ (___)");
                }
                else
                {
                    inspectStrings.Add($"{i + 1}. {severityRecord.Name} ({severityRecord.Severity})");
                }
            }
            return String.Join("\n", inspectStrings);
        }

        public class SeverityRecord : IExposable
        {
            public string Name;
            public float Severity;

            public SeverityRecord()
            {
                Name = "Unknown";
                Severity = 0;
            }

            public SeverityRecord(string name, float severity)
            {
                Name = name;
                Severity = severity;
            }

            public void ExposeData()
            {
                Scribe_Values.Look(ref Name, "Name");
                Scribe_Values.Look(ref Severity, "Severity");
            }

            public override string ToString()
            {
                return $"{Name}, {Severity}";
            }
        }
    }
}
