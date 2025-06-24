namespace PronounsMod
{
    public static class Debug
    {
        public static void Log(object message)
        {
#if DEBUG
            Verse.Log.Message($"[{PronounsMod.PACKAGE_NAME}] {message}");
#endif
        }
    }
}
