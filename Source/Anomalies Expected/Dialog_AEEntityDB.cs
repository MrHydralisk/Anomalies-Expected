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
        private Vector2 recordScrollPos;

        private Vector2 dbScrollPos;

        private float recordScrollHeight;

        private float dbScrollHeight;

        private AEEntityEntry selectedEntry;

        private List<(string, List<AEEntityEntry>)> EntriesByType = new List<(string, List<AEEntityEntry>)>();

        private Dictionary<string, float> categoryRectSizes = new Dictionary<string, float>();

        private static readonly Vector2 ButSize = new Vector2(150f, 38f);

        private const float HeaderHeight = 30f;

        //private const float LeftRectWidthPct = 0.35f;

        private const float EntrySize = 128f;

        private const float EntryGap = 8f;

        public override Vector2 InitialSize => new Vector2(UI.screenWidth * 0.9f, UI.screenHeight * 0.9f);

        private int state = 0;
        private bool isShowRecord;
        private bool[] lettersOpened;

        public Dialog_AEEntityDB(EntityCodexEntryDef selectedEntry = null)
        {
            doCloseX = true;
            forcePause = true;
            DoEntityCategory();
        }

        public void DoEntityCategory()
        {
            List<EntityCategoryDef> categoriesInOrder = DefDatabase<EntityCategoryDef>.AllDefsListForReading.OrderBy((EntityCategoryDef ecd) => ecd.listOrder).ToList();
            EntriesByType = new List<(string, List<AEEntityEntry>)>();
            categoryRectSizes.Clear();
            foreach (AEEntityEntry aeee in GameComponent_AnomaliesExpected.instance.EntityEntries)
            {
                List<AEEntityEntry> aeeeList;
                int index = EntriesByType.FindIndex(x => x.Item1 == (aeee.categoryLabelCap));
                if (index >= 0)
                {
                    aeeeList = EntriesByType[index].Item2;
                }
                else
                {
                    aeeeList = new List<AEEntityEntry>();
                    string key = aeee.categoryLabelCap;
                    EntriesByType.Add((key, aeeeList));
                    categoryRectSizes.Add(key, 0f);
                }
                aeeeList.Add(aeee);
            }
            foreach ((string key, List<AEEntityEntry> value) in EntriesByType)
            {
                value.SortBy((AEEntityEntry aeee) => aeee.EntityCodexEntryDef?.orderInCategory ?? int.MaxValue, (AEEntityEntry aeee) => aeee.EntityCodexEntryDef?.label ?? string.Empty);
            }
            EntriesByType.SortBy(x => categoriesInOrder.FindIndex((EntityCategoryDef ecd) => ecd.defName == x.Item1));
        }

        public void DoEntityClass()
        {
            EntriesByType = new List<(string, List<AEEntityEntry>)>();
            categoryRectSizes.Clear();
            foreach (AEEntityEntry aeee in GameComponent_AnomaliesExpected.instance.EntityEntries)
            {
                List<AEEntityEntry> aeeeList;
                (string, List<AEEntityEntry>) aeeePair = EntriesByType.FirstOrDefault(x => x.Item1 == aeee.threatClassString);
                if (aeeePair.Item2 == null)
                {
                    aeeeList = new List<AEEntityEntry>();
                    EntriesByType.Add((aeee.threatClassString, aeeeList));
                    categoryRectSizes.Add(aeee.threatClassString, 0f);
                }
                else
                {
                    aeeeList = aeeePair.Item2;
                }
                aeeeList.Add(aeee);
            }
            foreach ((string key, List<AEEntityEntry> value) in EntriesByType)
            {
                value.SortBy((AEEntityEntry aeee) => aeee.EntityCodexEntryDef?.orderInCategory ?? int.MaxValue, (AEEntityEntry aeee) => aeee.EntityCodexEntryDef?.label ?? string.Empty);
            }
            EntriesByType.SortBy(x => x.Item2.FirstOrDefault()?.ThreatClass ?? -1);
        }

        public void DoEntityGroup()
        {
            EntriesByType = new List<(string, List<AEEntityEntry>)>();
            categoryRectSizes.Clear();
            foreach (AEEntityEntry aeee in GameComponent_AnomaliesExpected.instance.EntityEntries)
            {
                List<AEEntityEntry> aeeeList;
                (string, List<AEEntityEntry>) aeeePair = EntriesByType.FirstOrDefault(x => x.Item1 == aeee.groupTag);
                if (aeeePair.Item2 == null)
                {
                    aeeeList = new List<AEEntityEntry>();
                    EntriesByType.Add((aeee.groupTag, aeeeList));
                    categoryRectSizes.Add(aeee.groupTag, 0f);
                }
                else
                {
                    aeeeList = aeeePair.Item2;
                }
                aeeeList.Add(aeee);
            }
            foreach ((string key, List<AEEntityEntry> value) in EntriesByType)
            {
                value.SortBy((AEEntityEntry aeee) => aeee.EntityCodexEntryDef?.orderInCategory ?? int.MaxValue, (AEEntityEntry aeee) => aeee.EntityCodexEntryDef?.label ?? string.Empty);
            }
            EntriesByType.SortBy(x => x.Item2.FirstOrDefault()?.groupTag ?? "");
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            Rect rect = inRect;
            rect.height -= ButSize.y + 10f;
            TaggedString taggedString = "AnomaliesExpected.EntityDataBase.Desc".Translate();
            Rect rect1 = new Rect(inRect.width / 2f - 132, 4, 264, 25);
            if (Widgets.ButtonText(rect1, "AnomaliesExpected.EntityDataBase.SortType".Translate($"AnomaliesExpected.EntityDataBase.SortType.{state}".Translate())))
            {
                List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                floatMenuOptions.Add(new FloatMenuOption("AnomaliesExpected.EntityDataBase.SortType.0".Translate(), delegate
                {
                    DoEntityCategory();
                    state = 0;
                }));
                floatMenuOptions.Add(new FloatMenuOption("AnomaliesExpected.EntityDataBase.SortType.1".Translate(), delegate
                {
                    DoEntityClass();
                    state = 1;
                }));
                floatMenuOptions.Add(new FloatMenuOption("AnomaliesExpected.EntityDataBase.SortType.2".Translate(), delegate
                {
                    DoEntityGroup();
                    state = 2;
                }));
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }
            Rect rect2 = new Rect(inRect.width - 39, 7, 32, 32);
            if (Widgets.ButtonImage(rect2, TexButton.CodexButton, tooltip: "EntityCodexGizmoTip".Translate()))
            {
                Find.WindowStack.Add(new Dialog_EntityCodex());
                Close();
            }
            if (isShowRecord)
            {
                Rect rect3 = new Rect(inRect.width / 2f + 159, 0, 33, 33);
                if (Widgets.ButtonImage(rect3, TexButton.CloseXSmall))
                {
                    isShowRecord = false;
                }
            }
            using (new TextBlock(GameFont.Medium))
            {
                Widgets.Label(new Rect(0f, 0f, rect.width, HeaderHeight), "AnomaliesExpected.EntityDataBase.Label".Translate());
            }
            //if (Prefs.DevMode && DebugSettings.godMode)
            //{
            //    Widgets.CheckboxLabeled(new Rect(rect.xMax - 150f, 0f, 150f, HeaderHeight), "DEV: Show all", ref devShowAll, disabled: false, null, null, placeCheckboxNearText: true);
            //}
            rect.yMin += 40f;
            TaggedString taggedString1 = "AnomaliesExpected.EntityDataBase.Desc".Translate();
            float num = Text.CalcHeight(taggedString1, rect.width);
            Widgets.Label(new Rect(0f, rect.y, rect.width, num), taggedString);
            rect.yMin += num + 10f;
            if (isShowRecord)
            {
                EntityRecord(rect);
            }
            else
                AllEntities(rect);
        }

        private void EntityRecord(Rect inRect)
        {
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, recordScrollHeight);
            Widgets.BeginScrollView(inRect, ref recordScrollPos, viewRect);
            if (selectedEntry != null)
            {
                EntityCodexEntryDef entityCodexEntryDef = selectedEntry.EntityCodexEntryDef;
                float num = 0f;
                bool flag = entityCodexEntryDef?.Discovered ?? true;
                using (new TextBlock(GameFont.Medium))
                {
                    Widgets.Label(new Rect(0f, num, viewRect.width, HeaderHeight), flag ? selectedEntry.AnomalyLabel : "UndiscoveredEntity".Translate());
                    num += 40f;
                }
                using (new TextBlock(newWordWrap: true))
                {
                    string text = (flag ? selectedEntry.AnomalyDesc : ((string)"UndiscoveredEntityDesc".Translate()));
                    float num2 = Text.CalcHeight(text, viewRect.width);
                    Widgets.Label(new Rect(0f, num, viewRect.width, num2), text);
                    num += num2 + EntryGap;
                }
                for (int i = 0; i < selectedEntry.letters.Count(); i++)
                {
                    ChoiceLetter choiceLetter = selectedEntry.letters[i];
                    using (new TextBlock(GameFont.Medium))
                    {
                        if (Widgets.ButtonText(new Rect(0f, num, viewRect.width, 25), choiceLetter.Label))
                        {
                            lettersOpened[i] = !lettersOpened[i];
                        }
                        num += 30f;
                    }
                    if (lettersOpened[i])
                    {
                        string text = choiceLetter.Text;
                        float num2 = Text.CalcHeight(text, viewRect.width);
                        Widgets.Label(new Rect(0f, num, viewRect.width, num2), text);
                        num += num2 + 5;
                    }
                }
                if (flag)
                {
                    if ((entityCodexEntryDef?.linkedThings.Count ?? 0) > 0)
                    {
                        foreach (ThingDef linkedThing in entityCodexEntryDef.linkedThings)
                        {
                            Rect rect = new Rect(0f, num, viewRect.width, Text.LineHeight);
                            if (Find.EntityCodex.Discovered(linkedThing))
                            {
                                Widgets.HyperlinkWithIcon(rect, new Dialog_InfoCard.Hyperlink(linkedThing));
                            }
                            else
                            {
                                rect.xMin += rect.height;
                                using (new TextBlock(ColoredText.SubtleGrayColor))
                                {
                                    Widgets.Label(rect, "Undiscovered".Translate());
                                }
                            }
                            num += rect.height;
                        }
                        num += EntryGap;
                    }
                    if ((entityCodexEntryDef?.discoveredResearchProjects.Count ?? 0) > 0)
                    {
                        Widgets.Label(new Rect(0f, num, viewRect.width, Text.LineHeight), "ResearchUnlocks".Translate() + ":");
                        num += Text.LineHeight;
                        foreach (ResearchProjectDef discoveredResearchProject in entityCodexEntryDef.discoveredResearchProjects)
                        {
                            Rect rect2 = new Rect(0f, num, viewRect.width, Text.LineHeight);
                            if (Widgets.ButtonText(rect2, "ViewHyperlink".Translate(discoveredResearchProject.LabelCap), drawBackground: false))
                            {
                                Close();
                                Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
                                ((MainTabWindow_Research)MainButtonDefOf.Research.TabWindow).Select(discoveredResearchProject);
                            }
                            num += rect2.height;
                        }
                    }
                }
                recordScrollHeight = num;
            }
            else
            {
                isShowRecord = false;
            }
            Widgets.EndScrollView();
        }

        private void AllEntities(Rect rect)
        {
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, dbScrollHeight);
            Widgets.BeginScrollView(rect, ref dbScrollPos, viewRect);
            float num = 0f;
            Text.Anchor = TextAnchor.MiddleCenter;
            foreach ((string key, List<AEEntityEntry> value) in EntriesByType)
            {
                float num2 = num;
                float height = categoryRectSizes[key];
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                Widgets.DrawHighlight(new Rect(0f, num, rect.width, height));
                GUI.color = Color.white;
                Widgets.Label(new Rect(0, num, rect.width, Text.LineHeight), key);
                num += Text.LineHeight + 4f;
                int MaxEntriesPerRow = Mathf.FloorToInt((viewRect.width - EntryGap) / (EntrySize + EntryGap));
                int EntryRows = Mathf.CeilToInt((float)value.Count / (float)MaxEntriesPerRow);
                for (int i = 0; i < value.Count; i++)
                {
                    AEEntityEntry aeee = value[i];
                    int num5 = i / MaxEntriesPerRow;
                    int num6 = i % MaxEntriesPerRow;
                    int num7 = ((i >= value.Count - value.Count % MaxEntriesPerRow) ? (value.Count % MaxEntriesPerRow) : MaxEntriesPerRow);
                    float num8 = (viewRect.width - (float)num7 * EntrySize - (float)(num7 - 1) * EntryGap) / 2f;
                    Rect rect2 = new Rect(num8 + (float)num6 * EntrySize + (float)num6 * EntryGap, num + (float)num5 * EntrySize + (float)num5 * EntryGap, EntrySize, EntrySize);
                    bool flag = aeee.EntityCodexEntryDef?.Discovered ?? true;
                    DrawEntry(rect2, aeee, flag);
                    if (flag)
                    {
                        Text.Font = GameFont.Tiny;
                        string name = aeee.EntityCodexEntryDef?.LabelCap ?? aeee.ThingDef.LabelCap;
                        float num9 = Text.CalcHeight(name, rect2.width);
                        Rect rect3 = new Rect(rect2.x, rect2.yMax - num9, rect2.width, num9);
                        Widgets.DrawBoxSolid(rect3, new Color(0f, 0f, 0f, 0.3f));
                        using (new TextBlock(TextAnchor.MiddleCenter))
                        {
                            Widgets.Label(rect3, name);
                        }
                        Text.Font = GameFont.Small;
                    }
                }
                num += EntryGap + (float)EntryRows * EntrySize + (float)(EntryRows - 1) * EntryGap;
                categoryRectSizes[key] = num - num2;
                num += EntryGap;
            }
            dbScrollHeight = num;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.EndScrollView();
        }

        private void DrawEntry(Rect rect, AEEntityEntry entry, bool discovered)
        {
            Widgets.DrawOptionBackground(rect, entry == selectedEntry);
            GUI.DrawTexture(rect.ContractedBy(2f), discovered ? (entry.EntityCodexEntryDef?.icon ?? entry.ThingDef.uiIcon) : entry.EntityCodexEntryDef.silhouette);
            if (Widgets.ButtonInvisible(rect))
            {
                selectedEntry = entry;
                isShowRecord = true;
                lettersOpened = new bool[selectedEntry.letters.Count()];
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
            }
        }
    }
}

