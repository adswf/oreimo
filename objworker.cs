
using System.Diagnostics;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Oreimo/Toradora Automatic Script Finder");
                Console.WriteLine(" -Drag and drop the RES.DAT_ folder onto this .exe");
            }

            else
            {
                Directory.CreateDirectory("Scripts");
                foreach (string arg in args)
                {
                    string lst = arg;
                    if (!arg.EndsWith("\\"))
                    { lst += "\\"; }
                    lst += "List.lst";

                    if (File.Exists(lst))
                    { RestoreScripts(File.ReadAllLines(lst)); }
                    else
                    { DumpScripts(arg); }
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void RestoreScripts(string[] Strings)
        {
            foreach (string entry in Strings)
            {
                string Output = entry.Split('\t')[0];
                string File = entry.Split('\t')[1];
                string Dir = AppDomain.CurrentDomain.BaseDirectory + "Scripts\\";

                Console.WriteLine($"Compressing: {File}");
                CompressScript(Dir + File);
                if (System.IO.File.Exists(Output))
                { System.IO.File.Delete(Output); }

                System.IO.File.Move(Dir + File + ".gz", Output);
                ExtractScript(Output);
            }
        }

        const string ScriptDir = ".\\Scripts\\";
        static void DumpScripts(string Dir)
        {
            string[] Scripts = Directory.GetFiles(Dir, "*.obj.gz", SearchOption.AllDirectories);
            List<string> Lines = new List<string>();
            foreach (string Script in Scripts)
            {
                Console.WriteLine($"Extracing: {Path.GetFileName(Script)}");
                ExtractScript(Script);
                Lines.Add(Script + "\t" + Path.GetFileNameWithoutExtension(Script));
            }

            File.WriteAllLines(ScriptDir + "List.lst", Lines.ToArray());
        }
        static void ExtractScript(string Script)
        {
            string MovedPath = ScriptDir + Path.GetFileName(Script);
            File.Copy(Script, MovedPath);
            Process.Start(".\\Workspace\\gzip.exe", $"-d \"{MovedPath}\"").WaitForExit();
            File.Delete(MovedPath);
        }

        static void CompressScript(string Script)
        { Process.Start(".\\Workspace\\gzip.exe", $"-n9 \"{Script}\"").WaitForExit(); }
    }
}
