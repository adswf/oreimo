using System.Diagnostics;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Toradora/Oreimo .dat Extraction Tool");
                Console.WriteLine("Drag the .dat files onto this .exe");
            }

            foreach (string arg in args)
            {
                if (arg.ToLower().EndsWith("-LstOrder.lst".ToLower()))
                {
                    string[] LSTS = File.ReadAllLines(arg);
                    SaveDat(LSTS);
                }

                else
                {
                    LSTOrder = new List<string>();
                    ResetWorkspace();
                    OpenDat(arg);
                    LSTOrder.Reverse();
                    File.WriteAllLines(arg + "-LstOrder.lst", LSTOrder.ToArray());
                }
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void ResetWorkspace()
        {
            Console.WriteLine("Starting...");
            string[] Files = Directory.GetFiles(".\\Workspace", "*.dat", SearchOption.TopDirectoryOnly);
            foreach (string DAT in Files)
            {
                if (File.Exists(".\\" + Path.GetFileName(DAT)))
                    File.Delete(DAT);
                else
                    File.Move(DAT, ".\\" + Path.GetFileName(DAT));
            }
        }

        static List<string> LSTOrder = new List<string>();
        const string TMP = ".\\Workspace\\{0}";

        #region EXTRACT
        static bool OpenDat(string DAT)
        {
            try
            {
                Console.WriteLine($"Extracing: {Path.GetFileName(DAT)}");
                string[] Files = ExtractDatContent(DAT);
                foreach (string File in Files)
                {
                    string ext = Path.GetExtension(File).ToLower().Trim(' ', '.');
                    if (ext == "dat" || ext == string.Empty)
                    {
                        OpenDat(File);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        static string[] ExtractDatContent(string DAT)
        {
            string TMPPath = string.Format(TMP, Path.GetFileName(DAT));
            if (!TMPPath.EndsWith(".dat"))
                TMPPath += ".dat";

            if (File.Exists(TMPPath))
                File.Delete(TMPPath);

            File.Move(DAT, TMPPath);


            string Filename;

            if (Path.GetFileName(DAT).ToLower() == "res.dat")
            {
                Filename = Path.GetFileName(DAT);
            }

            else
            {
                Filename = Path.GetFileNameWithoutExtension(DAT);
            }

            string NewDir = Path.GetDirectoryName(DAT) + "\\" + Filename + "\\";
            if (NewDir.StartsWith("\\"))
            {
                NewDir = "." + NewDir;
            }

            var startinfo = new ProcessStartInfo("cmd.exe", "/C \".\\Workspace\\!XP.BAT\"")
            {
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            List<string> FLIST = new List<string>();
            var process = new Process();
            process.StartInfo = startinfo;
            process.ErrorDataReceived += (sender, args) => {
                if (args.Data == null)
                    return;

                string Line = args.Data.Trim();
                if (!Line.StartsWith("^"))
                {
                    if (!Line.Contains("#") || !Line.Contains("@"))
                    {
                        return;
                    }
                    string DFN = GetDatFN(Line.Split('#')[1].Split('@')[0].Trim('\t', ' ', ','));
                    FLIST.Add(DFN);
                    Console.Title = "Processing: " + DFN;
                    return;
                }

                FLIST[FLIST.Count - 1] = ".\\" + Line.Trim('^', ' ', '\t') + '\t' + FLIST.Last();
            };
            process.Start();
            process.BeginErrorReadLine();
            process.WaitForExit();


            File.Move(TMPPath, DAT);


            if (Directory.Exists(".\\Workspace\\" + Filename))
            {

                if (File.Exists(NewDir.TrimEnd('\\')))
                { NewDir = NewDir.TrimEnd('\\') + "_\\"; }

                if (Directory.Exists(NewDir))
                { Directory.Delete(NewDir, true); }

                Directory.Move(".\\Workspace\\" + Filename, NewDir);

                string TXT = "Y";
                foreach (string File in FLIST.ToArray())
                    TXT += "\r\n" + File;

                if (FLIST.Count == 0)
                {
                    Console.WriteLine("Something went wrong... Retrying...");
                    return ExtractDatContent(DAT);

                }

                string lst = GetDatLFN(DAT);
                if (lst.StartsWith("\\"))
                    lst = "." + lst;

                if (File.Exists(lst))
                    File.Delete(lst);

                File.WriteAllText(lst, TXT);
                LSTOrder.Add(lst);

                return Directory.GetFiles(NewDir, "*", SearchOption.AllDirectories);
            }

            return new string[0];
        }

        private static string GetDatFN(string file)
        {
            if (Path.GetExtension(file) == string.Empty && !file.EndsWith("."))
                file += '.';

            string[] Splits = Path.GetFileName(file).Split('.');
            if (Splits.Length >= 3 && int.TryParse(Splits[Splits.Length - 2], out int tmp))
            {
                string FN = string.Empty;
                for (int i = 0; i < Splits.Length; i++)
                {
                    if (int.TryParse(Splits[i], out int TMP) && Splits[i].StartsWith("0"))
                        continue;
                    FN += Splits[i] + ".";
                }
                return FN.TrimEnd('.', ' ');
            }

            return Path.GetFileName(file).TrimEnd('.', ' ');
        }
        private static string GetDatLFN(string file)
        {
            string Filename;
            if (Path.GetFileName(file).ToLower() == "res.dat")
            { Filename = Path.GetFileName(file); }
            else
            { Filename = Path.GetFileNameWithoutExtension(file); }

            return Path.GetDirectoryName(file) + "\\" + Filename + ".lst";
        }
        #endregion

        #region REPACK
        public static bool SaveDat(string[] Files)
        {
            int i = 0;
            foreach (string File in Files)
            {
                if (!System.IO.File.Exists(File))
                { continue; }

                Console.WriteLine("Repacking {0}/{1}: {2}", ++i, Files.Length, Path.GetFileName(File));
                RepackDat(File);
            }

            return true;
        }
        public static bool RepackDat(string DAT, bool Retry = true)
        {
            try
            {
                string DatDir = Path.GetDirectoryName(DAT) + "\\";
                string DirName;

                if (Path.GetFileName(DAT).ToLower() == "res.dat")
                { DirName = Path.GetFileName(DAT); }
                else
                { DirName = Path.GetFileNameWithoutExtension(DAT); }

                if (DatDir.StartsWith("\\"))
                { DatDir = '.' + DatDir; }

                if (DatDir.StartsWith(".\\"))
                {
                    DatDir = AppDomain.CurrentDomain.BaseDirectory + DatDir.Substring(3, DatDir.Length - 3);
                }

                bool Escape = Directory.Exists(DatDir + DirName + "_");

                if (!Escape && !Directory.Exists(DatDir + DirName))
                { return false; }


                if (Escape)
                {
                    int Tries = 0;
                    while (Directory.Exists(DatDir + DirName + "_"))
                    {
                        Tries++;
                        if (File.Exists(DatDir + DirName))
                        { File.Delete(DatDir + DirName); }

                        Directory.Move(DatDir + DirName + "_", DatDir + DirName);
                        if (Tries > 1)
                        { Console.WriteLine($"Trying to rename folder... Attempts: {Tries}"); }
                    }
                }

                Process Proc = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = AppDomain.CurrentDomain.BaseDirectory + "Workspace\\makeGDP.exe",
                        Arguments = "\"" + DatDir + DirName + "\"",
                        WorkingDirectory = DatDir,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                Proc.Start();
                Proc.WaitForExit();

                if (!File.Exists(DatDir + DirName + ".dat"))
                {
                    if (Retry)
                    {
                        Console.WriteLine("Something went wrong... Retrying");
                        Debugger.Launch();
                        Debugger.Break();
                        return RepackDat(DAT, false);
                    }
                }

                if (Escape)
                {
                    Directory.Move(DatDir + DirName, DatDir + DirName + "_");
                    File.Move(DatDir + DirName + ".dat", DatDir + DirName);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

    }
}
