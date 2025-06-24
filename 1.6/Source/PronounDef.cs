using Verse;

namespace PronounsMod
{
    public class PronounDef : Def
    {
        public string subjective;
        public string objective;
        public string possessive;
        public VerbForm verbForm = VerbForm.Singular;
        public float commonalityFemale;
        public float commonalityMale;
    }
}
