using HarmonyLib;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Verse;

namespace PronounsMod
{
    [HarmonyPatch(typeof(GrammarResolverSimple))]
    [HarmonyPatch(nameof(GrammarResolverSimple.Formatted))]
    public static class Patch_GrammarResolverSimple
    {
        public static void Prefix(ref TaggedString str, List<string> argsLabelsArg, List<object> argsObjectsArg)
        {
            if (argsObjectsArg == null || argsLabelsArg == null)
            {
                return;
            }

            int count = argsObjectsArg.Count < argsLabelsArg.Count ? argsObjectsArg.Count : argsLabelsArg.Count;
            for (int i = 0; i < count; i++)
            {
                object arg = argsObjectsArg[i];
                if (arg is Pawn pawn)
                {
                    Comp_Pronouns comp = pawn.TryGetComp<Comp_Pronouns>();
                    if (comp != null)
                    {
                        string label = argsLabelsArg[i];
                        if (label.NullOrEmpty())
                        {
                            continue;
                        }

                        // pull non-null, safe replacements
                        string subj = comp.Subjective ?? "they";
                        string obj = comp.Objective ?? "them";
                        string poss = comp.Possessive ?? "their";

                        if (comp.VerbForm == VerbForm.Plural && Find.ActiveLanguageWorker is LanguageWorker_English)
                        {
                            str = Regex.Replace(str, $@"[\[{{]{label}_pronoun[\]}}] (\w+)", m => $"{subj} {VerbPluralizer.Pluralize(m.Groups[1].Value)}");
                            str = Regex.Replace(str, $@"[\[{{]{label}_pronoun[\]}}]'s", m => $"{subj}'re");
                        }

                        // VerbForm can never be null, and we've already checked for pluralization above,
                        // therefore, either way, we can safely replace the pronouns
                        str = str
                            .Replace($"{{{label}_pronoun}}", subj)
                            .Replace($"{{{label}_objective}}", obj)
                            .Replace($"{{{label}_possessive}}", poss)
                            .Replace($"[{label}_pronoun]", subj)
                            .Replace($"[{label}_objective]", obj)
                            .Replace($"[{label}_possessive]", poss);
                    }
                }
            }
        }
    }
}
