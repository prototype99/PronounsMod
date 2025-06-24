using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace PronounsMod
{
    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("TryGenerateNewPawnInternal")]
    public static class Patch_PawnGenerator
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionsList = instructions.ToList();
            int index = instructionsList.FirstIndexOf(i => i.operand is MethodInfo info && info == typeof(PawnGenerator).Method("GenerateRandomAge"));
            instructionsList.InsertRange(index - 2, new[]
            {
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Call, typeof(Comp_Pronouns).Method(nameof(Comp_Pronouns.GeneratePronouns)))
            });
            return instructionsList;
        }
    }
}
