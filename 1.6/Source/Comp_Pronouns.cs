using System;
using Verse;

namespace PronounsMod
{
    public class Comp_Pronouns : ThingComp
    {
        private PronounDef pronouns = DefDatabase<PronounDef>.GetRandom();
        private string customSubjective;
        private string customObjective;
        private string customPossessive;
        private VerbForm customVerbForm;

        public static void GeneratePronouns(Pawn pawn)
        {
            Comp_Pronouns comp = pawn.TryGetComp<Comp_Pronouns>();
            if (comp != null)
            {
                DefDatabase<PronounDef>.AllDefsListForReading.TryRandomElementByWeight(p => pawn.gender == Gender.Female ? p.commonalityFemale : p.commonalityMale, out PronounDef result);
                if (result != null)
                {
                    comp.SetPronouns(result);
                }
            }
        }

        public string Subjective => pronouns?.subjective ?? customSubjective ?? "they";

        public string Objective => pronouns?.objective ?? customObjective ?? "them";

        public string Possessive => pronouns?.possessive ?? customPossessive ?? "their";

        // Default to plural verbs when no pronouns are set to avoid "is/are" mismatches
        public VerbForm VerbForm => pronouns?.verbForm ?? (customVerbForm == 0 ? VerbForm.Plural : customVerbForm);

        public string Label => pronouns?.label ?? $"{customSubjective ?? "they"}/{customObjective ?? "them"}/{customPossessive ?? "their"}";

        public bool IsCustom => pronouns == null;

        public PronounDef Pronouns => pronouns;

        public void SetPronouns(PronounDef def)
        {
            if (def == null)
            {
                throw new ArgumentNullException("PronounDef def must not be null.");
            }
            pronouns = def;
            customSubjective = customObjective = customPossessive = null;
        }

        public void SetPronouns(string subjective, string objective, string possessive, VerbForm verbForm)
        {
            if (subjective.NullOrEmpty() || objective.NullOrEmpty() || possessive.NullOrEmpty())
            {
                throw new ArgumentException("strings subjective, objective, and possessive must all have values.");
            }
            customSubjective = subjective;
            customObjective = objective;
            customPossessive = possessive;
            customVerbForm = verbForm;
            pronouns = null;
        }

        public override void PostExposeData()
        {
            Scribe_Defs.Look(ref pronouns, "pronouns");
            Scribe_Values.Look(ref customSubjective, "customSubjective");
            Scribe_Values.Look(ref customObjective, "customObjective");
            Scribe_Values.Look(ref customPossessive, "customPossessive");
            Scribe_Values.Look(ref customVerbForm, "customVerbForm");
        }
    }
}
