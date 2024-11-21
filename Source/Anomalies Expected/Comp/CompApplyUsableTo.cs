using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AnomaliesExpected
{
    public class CompApplyUsableTo : CompUsable
    {
        private Texture2D icon;

        private Color? iconColor;

        public new CompProperties_ApplyUsableTo Props => (CompProperties_ApplyUsableTo)props;

        private Texture2D Icon
        {
            get
            {
                if (icon == null && Props.floatMenuFactionIcon != null)
                {
                    icon = Find.FactionManager.FirstFactionOfDef(Props.floatMenuFactionIcon)?.def?.FactionIcon;
                }
                return icon;
            }
        }

        private Color IconColor
        {
            get
            {
                if (!iconColor.HasValue && Props.floatMenuFactionIcon != null)
                {
                    iconColor = Find.FactionManager.FirstFactionOfDef(Props.floatMenuFactionIcon)?.Color;
                }
                return iconColor ?? Color.white;
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
        {
            if (Props.useJob != null)
            {
                AcceptanceReport acceptanceReport = CanBeUsedBy(myPawn, Props.ignoreOtherReservations);
                FloatMenuOption floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(FloatMenuOptionLabel(myPawn), delegate
                {
                    foreach (CompUseEffect comp in parent.GetComps<CompUseEffect>())
                    {
                        if (comp.SelectedUseOption(myPawn))
                        {
                            return;
                        }
                    }
                    TryStartUseJob(myPawn, myPawn, Props.ignoreOtherReservations);
                }, priority: Props.floatMenuOptionPriority, itemIcon: Icon, iconColor: IconColor), myPawn, parent);
                if (!acceptanceReport.Accepted)
                {
                    floatMenuOption.Disabled = true;
                    floatMenuOption.Label = floatMenuOption.Label + " (" + acceptanceReport.Reason + ")";
                }
                yield return floatMenuOption;
            }
            if (Props.applyToJob != null)
            {
                FloatMenuOption floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(Props.useToLabel.Formatted(myPawn), delegate
                {
                    foreach (CompUseEffect comp in parent.GetComps<CompUseEffect>())
                    {
                        if (comp.SelectedUseOption(myPawn))
                        {
                            return;
                        }
                    }
                    TryStartUseJob(myPawn, GetExtraTarget(myPawn), Props.ignoreOtherReservations);
                }, priority: Props.floatMenuOptionPriority, itemIcon: Icon, iconColor: IconColor), myPawn, parent);
                yield return floatMenuOption;
            }
        }

        public override void TryStartUseJob(Pawn pawn, LocalTargetInfo extraTarget, bool forced = false)
        {
            Pawn pawnTarget = null;
            bool isWithoutTarget = extraTarget == LocalTargetInfo.Invalid || extraTarget == pawn || (pawnTarget = extraTarget.Pawn) == null;
            if (isWithoutTarget)
            {
                if (Props.useJob == null || !CanBeUsedBy(pawn, forced))
                {
                    return;
                }
            }
            else
            {
                if (Props.applyToJob == null || !CanBeUsedBy(pawnTarget, forced))
                {
                    return;
                }
            }
            StringBuilder stringBuilder = new StringBuilder();
            foreach (CompUseEffect comp in parent.GetComps<CompUseEffect>())
            {
                TaggedString taggedString = comp.ConfirmMessage(pawn);
                if (!taggedString.NullOrEmpty())
                {
                    if (stringBuilder.Length != 0)
                    {
                        stringBuilder.AppendLine();
                    }
                    stringBuilder.AppendTagged(taggedString);
                }
            }
            string text = stringBuilder.ToString();
            if (text.NullOrEmpty())
            {
                StartJob(isWithoutTarget);
            }
            else
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, delegate
                {
                    StartJob(isWithoutTarget);
                }));
            }

            void StartJob(bool isWithoutTarget)
            {
                if (isWithoutTarget)
                {
                    Job job = (extraTarget == pawn ? JobMaker.MakeJob(Props.useJob, parent, extraTarget) : JobMaker.MakeJob(Props.useJob, parent));
                    job.count = 1;
                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }
                else
                {
                    Job job = JobMaker.MakeJob(Props.applyToJob, parent, extraTarget);
                    job.count = 1;
                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }
            }
        }
    }
}
