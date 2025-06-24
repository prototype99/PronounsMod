using System;

namespace PronounsMod
{
    public static class VerbPluralizer
    {
        private static string[] table = new[]
        {
            "is", "are",
            "was", "were",
            "has", "have",
            "does", "do",
            "says", "say",
            "gets", "get",
            "makes", "make",
            "goes", "go",
            "knows", "know",
            "takes", "take",
            "comes", "come",
            "sees", "see",
            "thinks", "think",
            "looks", "look",
            "wants", "want",
            "gives", "give",
            "uses", "use",
            "finds", "find",
            "tells", "tell",
            "asks", "ask",
            "works", "work"
        };

        public static string Pluralize(string s)
        {
            int index = Array.IndexOf(table, s);
            if (index >= 0 && index < table.Length - 1)
            {
                return table[index + 1];
            }
            else
            {
                return s;
            }
        }
    }
}
