using System;
using Verse;

namespace PronounsMod
{
    public enum VerbForm
    {
        Singular,
        Plural
    }

    public static class VerbTenseUtility
    {
        public static string GetLabel(this VerbForm verbForm)
        {
            switch (verbForm)
            {
                case VerbForm.Singular: return "PronounsMod_VerbFormSingular".Translate();
                case VerbForm.Plural: return "PronounsMod_VerbFormPlural".Translate();
                default: throw new ArgumentException("Invalid verb form: " + verbForm);
            }
        }
    }
}
