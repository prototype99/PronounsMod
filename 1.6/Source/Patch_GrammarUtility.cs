using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace PronounsMod
{
    [HarmonyPatch(typeof(GrammarUtility))]
    [HarmonyPatch(nameof(GrammarUtility.RulesForPawn))]
    [HarmonyPatch(new[] { typeof(string), typeof(Pawn), typeof(Dictionary<string, string>), typeof(bool), typeof(bool) })]
    public static class Patch_GrammarUtility
    {
        public static void Postfix(string pawnSymbol, Pawn pawn, ref IEnumerable<Rule> __result)
        {
            Comp_Pronouns comp = pawn?.TryGetComp<Comp_Pronouns>();
            if (comp != null)
            {
                List<Rule> rules = __result.ToList();
                rules.Replace(rules.First(r => r.keyword == pawnSymbol + "_pronoun"), new Rule_String(pawnSymbol + "_pronoun", comp.Subjective));
                rules.Replace(rules.First(r => r.keyword == pawnSymbol + "_objective"), new Rule_String(pawnSymbol + "_objective", comp.Objective));
                rules.Replace(rules.First(r => r.keyword == pawnSymbol + "_possessive"), new Rule_String(pawnSymbol + "_possessive", comp.Possessive));
                __result = rules;
            }
        }
    }
}
