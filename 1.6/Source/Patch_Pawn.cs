using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace PronounsMod
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch(nameof(Pawn.MainDesc))]
    public static class Patch_Pawn
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionsList = instructions.ToList();
            int index = instructionsList.FirstIndexOf(i => i.opcode == OpCodes.Stloc_1);
            instructionsList.InsertRange(index + 1, new[]
            {
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(PatchUtility_Pawn).Method(nameof(PatchUtility_Pawn.GetPronounsString))),
                new CodeInstruction(OpCodes.Call, typeof(String).Method(nameof(String.Concat), new[] { typeof(string), typeof(string) })),
                new CodeInstruction(OpCodes.Stloc_1)
            });
            return instructionsList;
        }
    }

    public static class PatchUtility_Pawn
    {
        public static string GetPronounsString(Pawn pawn, bool forInfoPane)
        {
            if (forInfoPane)
            {
                Comp_Pronouns comp = pawn.TryGetComp<Comp_Pronouns>();
                if (comp != null)
                {
                    return $" ({comp.Label})";
                }
            }
            return string.Empty;
        }
    }
}
