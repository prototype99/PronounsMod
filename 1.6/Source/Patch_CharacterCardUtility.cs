using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace PronounsMod
{
    [HarmonyPatch(typeof(CharacterCardUtility))]
    [HarmonyPatch("DoTopStack")]
    public static class Patch_CharacterCardUtility_DoTopStack
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionsList = instructions.ToList();
            int index = instructionsList.IndexOf(instructionsList.Where(i => i.operand is MethodInfo info && info == typeof(List<GenUI.AnonymousStackElement>).Method(nameof(List<GenUI.AnonymousStackElement>.Add))).ToList()[1]);
            instructionsList.InsertRange(index + 1, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldsfld, typeof(CharacterCardUtility).Field("tmpStackElements")),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call, typeof(PatchUtility_CharacterCardUtility).Method(nameof(PatchUtility_CharacterCardUtility.DoPronounsStackElement)))
            });
            return instructionsList;
        }
    }

    [StaticConstructorOnStartup]
    public static class PatchUtility_CharacterCardUtility
    {
        private static readonly Texture2D changePronounsButtonTex = ContentFinder<Texture2D>.Get("UI/Buttons/PronounsMod_ChangePronouns");

        public static void DoPronounsStackElement(Pawn pawn, List<GenUI.AnonymousStackElement> stack, bool creationMode)
        {
            Comp_Pronouns comp = pawn.TryGetComp<Comp_Pronouns>();
            if (comp != null)
            {
                stack.Add(new GenUI.AnonymousStackElement
                {
                    drawer = r => 
                    {
                        GUI.color = CharacterCardUtility.StackElementBackground;
                        GUI.DrawTexture(r, BaseContent.WhiteTex);
                        GUI.color = Color.white;
                        Widgets.DrawHighlightIfMouseover(r);
                        GUI.DrawTexture(new Rect(r.x + 1f, r.y + 1f, 20f, 20f), changePronounsButtonTex);
                        using (new TextBlock(TextAnchor.MiddleCenter)) Widgets.Label(new Rect(r.x + 22f, r.y + 1f, r.width - 22f, 20f), comp.Label);
                        bool allowChange = pawn.IsPlayerControlled || DebugSettings.ShowDevGizmos;
                        if (allowChange && Widgets.ButtonInvisible(r))
                        {
                            Find.WindowStack.Add(new Dialog_ChangePronouns(pawn, creationMode));
                        }
                        TooltipHandler.TipRegion(r, ("PronounsMod_Pronouns".Translate() + ": " + comp.Label).Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "PronounsMod_PronounsDesc".Translate(pawn.Named("PAWN")) + (allowChange ? "\n\n" + "PronounsMod_ClickToChange".Translate().Colorize(ColoredText.SubtleGrayColor) : string.Empty));
                    },
                    width = Text.CalcSize(comp.Label).x + 22f + 15f
                });
            }
        }
    }
}
