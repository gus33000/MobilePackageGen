namespace ToCBS
{
    internal class TempManager
    {
        private static readonly List<string> tempFiles = [];

        internal static string GetTempFile()
        {
            string file = Path.GetTempFileName();
            File.Delete(file);
            tempFiles.Add(file);
            return file;
        }

        internal static void CleanupTempFiles()
        {
            foreach (string file in tempFiles)
            {
                File.Delete(file);
            }
            tempFiles.Clear();
        }
    }
}
