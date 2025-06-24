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
            for (int i = 0; i < argsObjectsArg.Count; i++)
            {
                object arg = argsObjectsArg[i];
                if (arg is Pawn pawn)
                {
                    Comp_Pronouns comp = pawn.TryGetComp<Comp_Pronouns>();
                    if (comp != null)
                    {
                        string label = argsLabelsArg[i];
                        if (comp.VerbForm == VerbForm.Plural && Find.ActiveLanguageWorker is LanguageWorker_English)
                        {
                            str = Regex.Replace(str, $@"[\[{{]{label}_pronoun[\]}}] (\w+)", m => $"{comp.Subjective} {VerbPluralizer.Pluralize(m.Groups[1].Value)}");
                            str = Regex.Replace(str, $@"[\[{{]{label}_pronoun[\]}}]'s", m => $"{comp.Subjective}'re");
                            str = str
                                .Replace($"{{{label}_pronoun}}", comp.Subjective)
                                .Replace($"{{{label}_objective}}", comp.Objective)
                                .Replace($"{{{label}_possessive}}", comp.Possessive)
                                .Replace($"[{label}_pronoun]", comp.Subjective)
                                .Replace($"[{label}_objective]", comp.Objective)
                                .Replace($"[{label}_possessive]", comp.Possessive);
                        }
                        else if (comp.VerbForm == VerbForm.Singular)
                        {
                            str = str
                                .Replace($"{{{label}_pronoun}}", comp.Subjective)
                                .Replace($"{{{label}_objective}}", comp.Objective)
                                .Replace($"{{{label}_possessive}}", comp.Possessive)
                                .Replace($"[{label}_pronoun]", comp.Subjective)
                                .Replace($"[{label}_objective]", comp.Objective)
                                .Replace($"[{label}_possessive]", comp.Possessive);
                        }
                    }
                }
            }
        }
    }
}
