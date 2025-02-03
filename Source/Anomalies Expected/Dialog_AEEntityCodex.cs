using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Dialog_AEEntityDB : Window
    {
        private Vector2 leftScrollPos;

        private Vector2 rightScrollPos;

        private float leftScrollHeight;

        private float rightScrollHeight;

        private AEEntityEntry selectedEntry;

        //private List<EntityCategoryDef> categoriesInOrder;

        //private Dictionary<EntityCategoryDef, List<EntityCodexEntryDef>> entriesByCategory = new Dictionary<EntityCategoryDef, List<EntityCodexEntryDef>>();

        private List<(string, List<AEEntityEntry>)> EntriesByType = new List<(string, List<AEEntityEntry>)>();

        private Dictionary<string, float> categoryRectSizes = new Dictionary<string, float>();

        private bool devShowAll;

        private static readonly Vector2 ButSize = new Vector2(150f, 38f);

        private const float HeaderHeight = 30f;

        //private const float LeftRectWidthPct = 0.35f;

        private const float EntrySize = 128f;

        private const float EntryGap = 8f;

        public override Vector2 InitialSize => new Vector2(UI.screenWidth * 0.9f, UI.screenHeight * 0.9f);

        public Dialog_AEEntityDB(EntityCodexEntryDef selectedEntry = null)
        {
            doCloseX = true;
            doCloseButton = true;
            forcePause = true;
            DoEntityCategory();
        }

        public void DoEntityCategory()
        {
            List<EntityCategoryDef> categoriesInOrder = DefDatabase<EntityCategoryDef>.AllDefsListForReading.OrderBy((EntityCategoryDef ecd) => ecd.listOrder).ToList();
            EntriesByType = new List<(string, List<AEEntityEntry>)>();
            foreach (AEEntityEntry aeee in GameComponent_AnomaliesExpected.instance.EntityEntries)
            {
                List<AEEntityEntry> aeeeList;
                (string, List<AEEntityEntry>) aeeePair = EntriesByType.FirstOrDefault(x => x.Item1 == aeee.EntityCodexEntryDef.category.defName);
                if (aeeePair.Item2 == null)
                {
                    aeeeList = new List<AEEntityEntry>();
                    EntriesByType.Add((aeee.EntityCodexEntryDef.category.defName, aeeeList));
                    categoryRectSizes.Add(aeee.EntityCodexEntryDef.category.defName, 0f);
                }
                else
                {
                    aeeeList = aeeePair.Item2;
                }
                aeeeList.Add(aeee);
            }
            foreach ((string key, List<AEEntityEntry> value) in EntriesByType)
            {
                value.SortBy((AEEntityEntry aeee) => aeee.EntityCodexEntryDef.orderInCategory, (AEEntityEntry aeee) => aeee.EntityCodexEntryDef.label);
            }
            EntriesByType.SortBy(x => categoriesInOrder.FindIndex((EntityCategoryDef ecd) => ecd.defName == x.Item1));


            //categoriesInOrder = GameComponent_AnomaliesExpected.instance.EntityEntries.Select((AEEntityEntry aeee) => aeee.EntityCodexEntryDef.category).Distinct().OrderBy((EntityCategoryDef ecd) => DefDatabase<EntityCategoryDef>.AllDefsListForReading.IndexOf(ecd)).ToList();
            //foreach (EntityCategoryDef ecd in categoriesInOrder)
            //{
            //    entriesByCategory.Add(ecd, new List<EntityCodexEntryDef>());
            //    categoryRectSizes.Add(ecd, 0f);
            //}
            //foreach (EntityCodexEntryDef eced in GameComponent_AnomaliesExpected.instance.EntityEntries.Select((AEEntityEntry aeee) => aeee.EntityCodexEntryDef))
            //{
            //    if (eced.Visible)
            //    {
            //        entriesByCategory[eced.category].Add(eced);
            //    }
            //}

            //foreach (KeyValuePair<EntityCategoryDef, List<EntityCodexEntryDef>> ecdP in entriesByCategory)
            //{
            //    GenCollection.Deconstruct(ecdP, out var _, out var value);
            //    value.SortBy((EntityCodexEntryDef eced) => eced.orderInCategory, (EntityCodexEntryDef eced) => eced.label);
            //}
            //this.selectedEntry = selectedEntry ?? DefDatabase<EntityCodexEntryDef>.AllDefs.OrderBy((EntityCodexEntryDef x) => x.label).FirstOrDefault((EntityCodexEntryDef x) => x.Discovered);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            Rect rect = inRect;
            rect.height -= ButSize.y + 10f;
            using (new TextBlock(GameFont.Medium))
            {
                Widgets.Label(new Rect(0f, 0f, rect.width, HeaderHeight), "AnomaliesExpected.EntityDataBase.Label".Translate());
            }
            if (Prefs.DevMode && DebugSettings.godMode)
            {
                Widgets.CheckboxLabeled(new Rect(rect.xMax - 150f, 0f, 150f, HeaderHeight), "DEV: Show all", ref devShowAll, disabled: false, null, null, placeCheckboxNearText: true);
            }
            rect.yMin += 40f;
            TaggedString taggedString = "AnomaliesExpected.EntityDataBase.Desc".Translate();
            float num = Text.CalcHeight(taggedString, rect.width);
            Widgets.Label(new Rect(0f, rect.y, rect.width, num), taggedString);
            rect.yMin += num + 10f;
            //Rect inRect2 = rect.LeftPart(0.35f);
            //Rect rect2 = rect.RightPart(0.65f);
            //LeftRect(inRect2);
            AllEntities(rect);
        }

        //private void LeftRect(Rect inRect)
        //{
        //    Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, leftScrollHeight);
        //    Widgets.BeginScrollView(inRect, ref leftScrollPos, viewRect);
        //    if (selectedEntry != null)
        //    {
        //        float num = 0f;
        //        bool flag = devShowAll || selectedEntry.Discovered;
        //        using (new TextBlock(GameFont.Medium))
        //        {
        //            Widgets.Label(new Rect(0f, num, viewRect.width, HeaderHeight), flag ? selectedEntry.LabelCap : "UndiscoveredEntity".Translate());
        //            num += 40f;
        //        }
        //        using (new TextBlock(newWordWrap: true))
        //        {
        //            string text = (flag ? selectedEntry.Description : ((string)"UndiscoveredEntityDesc".Translate()));
        //            float num2 = Text.CalcHeight(text, viewRect.width);
        //            Widgets.Label(new Rect(0f, num, viewRect.width, num2), text);
        //            num += num2 + EntryGap;
        //        }
        //        if (flag)
        //        {
        //            if (selectedEntry.linkedThings.Count > 0)
        //            {
        //                foreach (ThingDef linkedThing in selectedEntry.linkedThings)
        //                {
        //                    Rect rect = new Rect(0f, num, viewRect.width, Text.LineHeight);
        //                    if (devShowAll || Find.EntityCodex.Discovered(linkedThing))
        //                    {
        //                        Widgets.HyperlinkWithIcon(rect, new Dialog_InfoCard.Hyperlink(linkedThing));
        //                    }
        //                    else
        //                    {
        //                        rect.xMin += rect.height;
        //                        using (new TextBlock(ColoredText.SubtleGrayColor))
        //                        {
        //                            Widgets.Label(rect, "Undiscovered".Translate());
        //                        }
        //                    }
        //                    num += rect.height;
        //                }
        //                num += EntryGap;
        //            }
        //            if (selectedEntry.discoveredResearchProjects.Count > 0)
        //            {
        //                Widgets.Label(new Rect(0f, num, viewRect.width, Text.LineHeight), "ResearchUnlocks".Translate() + ":");
        //                num += Text.LineHeight;
        //                foreach (ResearchProjectDef discoveredResearchProject in selectedEntry.discoveredResearchProjects)
        //                {
        //                    Rect rect2 = new Rect(0f, num, viewRect.width, Text.LineHeight);
        //                    if (Widgets.ButtonText(rect2, "ViewHyperlink".Translate(discoveredResearchProject.LabelCap), drawBackground: false))
        //                    {
        //                        Close();
        //                        Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
        //                        ((MainTabWindow_Research)MainButtonDefOf.Research.TabWindow).Select(discoveredResearchProject);
        //                    }
        //                    num += rect2.height;
        //                }
        //            }
        //        }
        //        else if (Prefs.DevMode && DebugSettings.godMode)
        //        {
        //            if (Widgets.ButtonText(new Rect(0f, num, viewRect.width, ButSize.y), "DEV: Discover"))
        //            {
        //                if (!selectedEntry.linkedThings.NullOrEmpty())
        //                {
        //                    for (int i = 0; i < selectedEntry.linkedThings.Count; i++)
        //                    {
        //                        Find.EntityCodex.SetDiscovered(selectedEntry, selectedEntry.linkedThings[i]);
        //                    }
        //                }
        //                else
        //                {
        //                    Find.EntityCodex.SetDiscovered(selectedEntry);
        //                }
        //            }
        //            num += ButSize.y;
        //        }
        //        leftScrollHeight = num;
        //    }
        //    Widgets.EndScrollView();
        //}

        private void AllEntities(Rect rect)
        {
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, rightScrollHeight);
            Widgets.BeginScrollView(rect, ref rightScrollPos, viewRect);
            float num = 0f;
            foreach ((string key, List<AEEntityEntry> value) in EntriesByType)
            {
                float num2 = num;
                float height = categoryRectSizes[key];
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                Widgets.DrawHighlight(new Rect(0f, num, rect.width, height));
                GUI.color = Color.white;
                Widgets.Label(new Rect(EntryGap, num, rect.width, Text.LineHeight), key);
                num += Text.LineHeight + 4f;
                int MaxEntriesPerRow = Mathf.FloorToInt((viewRect.width - EntryGap) / (EntrySize + EntryGap));
                int EntryRows = Mathf.CeilToInt((float)value.Count / (float)MaxEntriesPerRow);
                for (int i = 0; i < value.Count; i++)
                {
                    AEEntityEntry entityCodexEntryDef = value[i];
                    int num5 = i / MaxEntriesPerRow;
                    int num6 = i % MaxEntriesPerRow;
                    int num7 = ((i >= value.Count - value.Count % MaxEntriesPerRow) ? (value.Count % MaxEntriesPerRow) : MaxEntriesPerRow);
                    float num8 = (viewRect.width - (float)num7 * EntrySize - (float)(num7 - 1) * EntryGap) / 2f;
                    Rect rect2 = new Rect(num8 + (float)num6 * EntrySize + (float)num6 * EntryGap, num + (float)num5 * EntrySize + (float)num5 * EntryGap, EntrySize, EntrySize);
                    bool flag = devShowAll || entityCodexEntryDef.EntityCodexEntryDef.Discovered;
                    DrawEntry(rect2, entityCodexEntryDef, flag);
                    if (flag)
                    {
                        Text.Font = GameFont.Tiny;
                        float num9 = Text.CalcHeight(entityCodexEntryDef.EntityCodexEntryDef.LabelCap, rect2.width);
                        Rect rect3 = new Rect(rect2.x, rect2.yMax - num9, rect2.width, num9);
                        Widgets.DrawBoxSolid(rect3, new Color(0f, 0f, 0f, 0.3f));
                        using (new TextBlock(TextAnchor.MiddleCenter))
                        {
                            Widgets.Label(rect3, entityCodexEntryDef.EntityCodexEntryDef.LabelCap);
                        }
                        Text.Font = GameFont.Small;
                    }
                }
                num += EntryGap + (float)EntryRows * EntrySize + (float)(EntryRows - 1) * EntryGap;
                categoryRectSizes[key] = num - num2;
                num += EntryGap;
            }
            rightScrollHeight = num;
            Widgets.EndScrollView();
        }

        private void DrawEntry(Rect rect, AEEntityEntry entry, bool discovered)
        {
            Widgets.DrawOptionBackground(rect, entry == selectedEntry);
            GUI.DrawTexture(rect.ContractedBy(2f), discovered ? entry.EntityCodexEntryDef.icon : entry.EntityCodexEntryDef.silhouette);
            if (Widgets.ButtonInvisible(rect))
            {
                selectedEntry = entry;
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
            }
        }
    }
}

