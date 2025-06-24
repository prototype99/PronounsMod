using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PronounsMod
{
    public class Dialog_ChangePronouns : Window
    {
        private static readonly List<string> controlNames = new List<string>()
        {
            "subjective",
            "objective",
            "possessive"
        };

        private Pawn pawn;
        private bool creationMode;
        private readonly Comp_Pronouns comp;
        private PronounDef pronouns;
        private string subjective;
        private string objective;
        private string possessive;
        private VerbForm verbForm;

        private bool showAdvanced = false;

        public override Vector2 InitialSize => new Vector2(400f, 320f);

        public Dialog_ChangePronouns(Pawn pawn, bool creationMode)
        {
            this.pawn = pawn;
            this.creationMode = creationMode;
            comp = pawn.GetComp<Comp_Pronouns>();
            if (comp.IsCustom)
            {
                pronouns = null;
                subjective = comp.Subjective;
                objective = comp.Objective;
                possessive = comp.Possessive;
                verbForm = comp.VerbForm;
            }
            else
            {
                pronouns = comp.Pronouns;
                subjective = objective = possessive = string.Empty;
                verbForm = comp.VerbForm;
            }

            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
        }

        private void Confirm()
        {
            bool success = false;
            if (pronouns != null)
            {
                comp.SetPronouns(pronouns);
                success = true;
            }
            else if (!subjective.NullOrEmpty() && !objective.NullOrEmpty() && !possessive.NullOrEmpty())
            {
                comp.SetPronouns(subjective, objective, possessive, verbForm);
                success = true;
            }
            else
            {
                Messages.Message("PronounsMod_PronounsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
            }

            if (success)
            {
                if (!creationMode)
                {
                    Messages.Message("PronounsMod_PawnChangesPronouns".Translate(pawn.Named("PAWN"), comp.Label.Named("PRONOUNS")), MessageTypeDefOf.PositiveEvent, false);
                }
                Close();
            }
        }

        public override void OnAcceptKeyPressed() => Confirm();

        public override void DoWindowContents(Rect inRect)
        {
            if (Event.current.type == EventType.KeyUp && (Event.current.keyCode == KeyCode.Tab || Event.current.keyCode == KeyCode.Slash))
            {
                bool forward = !Event.current.shift || Event.current.keyCode == KeyCode.Slash;
                GUI.FocusControl(forward ? controlNames[(controlNames.IndexOf(GUI.GetNameOfFocusedControl()) + 1) % controlNames.Count] : controlNames[(controlNames.IndexOf(GUI.GetNameOfFocusedControl()) - 1 + controlNames.Count) % controlNames.Count]);
                Event.current.Use();
            }

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            Rect headerRect = listing.GetRect(128f);
            Rect portraitRect = new Rect(headerRect.x, headerRect.y, headerRect.height, headerRect.height);
            RenderTexture renderTexture = PortraitsCache.Get(pawn, Vector2.one * headerRect.height, Rot4.South, healthStateOverride: PawnHealthState.Mobile);
            GUI.DrawTexture(portraitRect, renderTexture);
            Rect genderRect = new Rect(headerRect.xMax - 30f, headerRect.y + 29f, 30f, 30f);
            GUI.DrawTexture(genderRect, pawn.gender.GetIcon());
            TooltipHandler.TipRegion(genderRect, pawn.gender.GetLabel().CapitalizeFirst());
            Rect titleRect = new Rect(portraitRect.xMax, headerRect.y + 29f, inRect.width - portraitRect.width - genderRect.width, 30f);
            using (new TextBlock(GameFont.Medium))
            {
                Widgets.Label(titleRect, pawn.LabelShortCap);
            }

            Rect buttonRect = new Rect(portraitRect.xMax, headerRect.yMax - 59f, inRect.width - portraitRect.width, 30f);
            if (Widgets.ButtonText(buttonRect, pronouns?.label ?? "PronounsMod_Custom".Translate()))
            {
                Find.WindowStack.Add(new FloatMenu(DefDatabase<PronounDef>.AllDefsListForReading.Select(p => new FloatMenuOption(p.label, () =>
                {
                    pronouns = p;
                    subjective = objective = possessive = string.Empty;
                    verbForm = p.verbForm;
                })).ToList()));
            }
            
            listing.Gap();
            Rect customRect = listing.GetRect(30f);
            float textWidth = (customRect.width - 30f) / 3f;
            int i = 0;

            Rect subjectiveRect = new Rect(customRect.x, customRect.y, textWidth, customRect.height);
            GUI.SetNextControlName(controlNames[i++]);
            subjective = Widgets.TextField(subjectiveRect, pronouns?.subjective ?? subjective, 10, CharacterCardUtility.ValidNameRegex);

            using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter)) Widgets.Label(new Rect(subjectiveRect.xMax, customRect.y, 15f, customRect.height), "/");

            Rect objectiveRect = new Rect(subjectiveRect.xMax + 15f, customRect.y, textWidth, customRect.height);
            GUI.SetNextControlName(controlNames[i++]);
            objective = Widgets.TextField(objectiveRect, pronouns?.objective ?? objective, 10, CharacterCardUtility.ValidNameRegex);

            using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter)) Widgets.Label(new Rect(objectiveRect.xMax, customRect.y, 15f, customRect.height), "/");

            Rect possessiveRect = new Rect(objectiveRect.xMax + 15f, customRect.y, textWidth, customRect.height);
            GUI.SetNextControlName(controlNames[i++]);
            possessive = Widgets.TextField(possessiveRect, pronouns?.possessive ?? possessive, 10, CharacterCardUtility.ValidNameRegex);

            if (Find.ActiveLanguageWorker is LanguageWorker_English)
            {
                listing.Gap();
                Rect advancedRect = listing.GetRect(30f);
                using (new TextBlock(Mouse.IsOver(advancedRect) ? GenUI.MouseoverColor : Color.white))
                {
                    GUI.DrawTexture(new Rect(advancedRect.x, advancedRect.y, advancedRect.height, advancedRect.height).ContractedBy(5f), showAdvanced ? TexButton.Collapse : TexButton.Reveal);
                }
                using (new TextBlock(TextAnchor.MiddleLeft))
                {
                    Widgets.Label(new Rect(advancedRect.x + advancedRect.height, advancedRect.y, advancedRect.width - advancedRect.height, advancedRect.height), "PronounsMod_AdvancedSettings".Translate());
                }
                if (Widgets.ButtonInvisible(advancedRect))
                {
                    showAdvanced = !showAdvanced;
                    (showAdvanced ? SoundDefOf.TabOpen : SoundDefOf.TabClose).PlayOneShot(null);
                }

                if (showAdvanced)
                {
                    listing.Indent(30f);
                    Rect verbTenseRect = listing.GetRect(30f);
                    Rect verbTenseLabelRect = new Rect(verbTenseRect.x, verbTenseRect.y, 80f, verbTenseRect.height);
                    using (new TextBlock(TextAnchor.MiddleLeft)) Widgets.Label(verbTenseLabelRect, "PronounsMod_VerbForm".Translate() + ": ");
                    Rect verbTenseButtonRect = new Rect(verbTenseLabelRect.xMax + 10f, verbTenseRect.y, 80f, verbTenseLabelRect.height);
                    if (Widgets.ButtonText(verbTenseButtonRect, (pronouns?.verbForm ?? verbForm).GetLabel()))
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        foreach (VerbForm v in Enum.GetValues(typeof(VerbForm)))
                        {
                            options.Add(new FloatMenuOption(v.GetLabel(), () => verbForm = v));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                }
            }

            if (subjective != pronouns?.subjective || objective != pronouns?.objective || possessive != pronouns?.possessive || verbForm != pronouns?.verbForm)
            {
                pronouns = null;
            }

            listing.End();

            if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - 30f, inRect.width / 2f - 5f, 30f), "Cancel".Translate().CapitalizeFirst()))
            {
                Close();
            }
            if (Widgets.ButtonText(new Rect(inRect.width / 2f + 5f, inRect.yMax - 30f, inRect.width / 2f - 5f, 30f), "Accept".Translate().CapitalizeFirst()))
            {
                Confirm();
            }
        }
    }
}
