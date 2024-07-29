using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace fproc
{
    public class fproc_Main
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(System.IntPtr hWnd, int cmdShow);

        public const string ProcessFile = "ProcessList.txt";
        public string FakeProcessDir = "procs" + Path.DirectorySeparatorChar;

        public Dictionary<string, Process> FakeProcesses = new Dictionary<string, Process>();
        public bool exit = false;
        int mpnl;
        int fpc;

        public void run(string[] args)
        {
            Console.Title = "fproc";

            if (!Directory.Exists(FakeProcessDir))
                Directory.CreateDirectory(FakeProcessDir);

            if (!File.Exists(ProcessFile))
                File.WriteAllLines(ProcessFile, new string[] { "vmtoolsd.exe", "vmwaretrat.exe", "vmwareuser.exe", "vmacthlp.exe", "vboxservice.exe", "vboxtray.exe", "vbox.exe" });

            foreach (string line in File.ReadAllLines(ProcessFile))
            {
                fpc++;

                if(line.Length > mpnl)
                    mpnl = line.Length;

                foreach (Process p in Process.GetProcessesByName(line))
                {
                    try {
                        string f = p.StartInfo.FileName;
                        p.Kill();
                        File.Delete(ProcessFile);
                    }
                    catch (Exception) {}
                }

                runProc(line);
                Console.WriteLine();
            }

            Thread t = new Thread(() => checkLoop());
            t.Start();

            Console.CancelKeyPress += (object _, ConsoleCancelEventArgs ev) => {
                exit = true;
                t.Abort();
                foreach (Process p in FakeProcesses.Values)
                    p.Kill();
                Thread.Sleep(100);
                Directory.Delete(FakeProcessDir, true);
                Console.CursorVisible = true;
                Environment.Exit(0);
            };

            while (true) {
                try
                {
                    Console.CursorVisible = false;
                    ShowWindow(GetConsoleWindow(), 9);
                    Console.SetWindowSize(mpnl * 2, fpc + 2);
                    Console.BufferHeight = Console.WindowHeight;
                    Console.BufferWidth = Console.WindowWidth;
                }catch (Exception) {}
            }
        }

        void checkLoop()
        {
            while (!exit)
            {
                Thread.Sleep(500);
                for (int i = 0; i < FakeProcesses.Count; i++)
                {
                    int cy = Console.CursorTop;
                    string name = FakeProcesses.Keys.ElementAt(i);
                    Process proc = FakeProcesses.Values.ElementAt(i);
                    Console.SetCursorPosition(0, FakeProcesses.Count - i);
                    if (proc == null || proc.HasExited || !proc.Responding)
                    {
                        Utils.PrintColor("[-] " + name, true, ConsoleColor.Red);
                        runProc(name);
                    }
                    else
                        Utils.PrintColor("[+] " + name, true, ConsoleColor.Green);
                    Console.SetCursorPosition(0, cy);
                }
            }
        }

        public void downProc(string proc)
        {
            for(int i = 0; i < FakeProcesses.Count; i++)
            {
                if (FakeProcesses.Keys.ElementAt(i) == proc)
                {
                    try
                    {
                        Process prc = FakeProcesses.Values.ElementAt(i);
                        if (prc != null || !prc.HasExited || prc.Responding)
                            prc.Kill();
                    }
                    catch (Exception) { }
                    FakeProcesses.Remove(proc);
                    break;
                }
            }
        }

        public void runProc(string proc)
        {
            try
            {
                if (File.Exists(FakeProcessDir + proc))
                    File.Delete(FakeProcessDir + proc);

                File.WriteAllBytes(FakeProcessDir + proc, File.ReadAllBytes("fproc-client.bin"));

                while (FakeProcesses.ContainsKey(proc) && !exit)
                    downProc(proc);
                FakeProcesses.Add(proc, Process.Start(FakeProcessDir + proc));
            }catch (Exception) { }
        }
    }
}
