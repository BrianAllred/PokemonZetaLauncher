/* Pokemon Zeta Omicron Launcher
 *      - Copies files from shared folder
 *        to local folder on launch and
 *        back again on close
 *        
 *  The MIT License (MIT)
 *
 *  Copyright (c) 2014 Brian Allred
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *  
 *  The above copyright notice and this permission notice shall be included in
 *  all copies or substantial portions of the Software.
 *  
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *  THE SOFTWARE.
 */



using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PokemonZetaLauncher
{
    class Program
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public Int32 dwTimeout;
        }

        public const UInt32 FLASHW_ALL = 3;

        //flag to catch errors
        static bool _error;
        static void Main(string[] args)
        {
            //only need two args, the local save location and the shared save location
            if (args.Length != 2)
            {
                Console.WriteLine("usage: PokemonZetaLauncher.exe <local save location> <shared save location>");
                _error = true;
            }
            else
            {
                var local = args[0];
                var shared = args[1];
                //make sure folders exist
                Directory.CreateDirectory(shared);
                Directory.CreateDirectory(local);
                //grab the saves from the shared location first
                //no worries, if there aren't any, you won't lose existing local saves
                CopySaves(shared, local);
                //run the game
                Game();
                //copy local back to shared
                CopySaves(local, shared);
            }
            //only do this if there's an error
            //so the user can see problems
            if (_error)
            {
                FlashWindow(Process.GetCurrentProcess().MainWindowHandle);
                Console.WriteLine("\nError occurred! Check output!\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Copies all files that end in ".rxdata" from "fromDir" to "toDir",
        /// checking modified time first
        /// </summary>
        /// <param name="fromDir"></param>
        /// <param name="toDir"></param>
        private static void CopySaves(string fromDir, string toDir)
        {
            foreach (var file in Directory.GetFiles(fromDir))
            {
                if (file.Contains(".rxdata"))
                {
                    string to = Path.Combine(toDir, file.Substring(file.LastIndexOf('\\') + 1));
                    DateTime fromTime = GetFileDate(file);
                    DateTime toTime = GetFileDate(to);
                    if (DateTime.Compare(fromTime, toTime) < 0)
                    {
                        Console.WriteLine("\nWARNING! Destination has a newer file of the same name: " + file.Substring(file.LastIndexOf('\\') + 1));
                        string answer = "z";
                        while (!(answer.ToLower().Equals("y") || answer.ToLower().Equals("n") || answer.Equals("")))
                        {
                            FlashWindow(Process.GetCurrentProcess().MainWindowHandle);
                            Console.Write("Do you want to overwrite anyway (y/N)? ");
                            answer = Console.ReadLine();
                        }

                        if (answer.Equals("n") || answer.Equals(""))
                        {
                            Console.WriteLine(file.Substring(file.LastIndexOf('\\') + 1) + " skipped.");
                            continue;
                        }
                    }
                    try
                    {
                        Console.WriteLine("Copying " + file + " to " + to);
                        File.Copy(file, to);
                        Console.WriteLine("Success!");
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine("File exists, attempting overwrite...");
                        File.Copy(file, to, true);
                        Console.WriteLine("Success!");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine();
                        Console.WriteLine(e.ToString());
                        Console.WriteLine();
                        _error = true;
                        
                    }
                }
            }
        }

        /// <summary>
        /// Runs the game and waits for user to exit it.
        /// </summary>
        private static void Game()
        {
            try
            {
                var externalProcess = new Process {StartInfo = {FileName = "Game.exe"}};
                externalProcess.Start();
                externalProcess.WaitForExit();
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine(e.ToString());
                Console.WriteLine();
                _error = true;
            }
        }

        /// <summary>
        /// Returns correct local DateTime of filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        static DateTime GetFileDate(string filename)
        {
            DateTime now = DateTime.Now;
            TimeSpan localOffset = now - now.ToUniversalTime();
            return File.GetLastWriteTimeUtc(filename) + localOffset;
        }

        /// <summary>
        /// Used to flash console window
        /// </summary>
        /// <param name="hWnd"></param>
        private static void FlashWindow(IntPtr hWnd)
        {
            var fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = hWnd;
            fInfo.dwFlags = FLASHW_ALL;
            fInfo.uCount = UInt32.MaxValue;
            fInfo.dwTimeout = 0;

            FlashWindowEx(ref fInfo);
        }
    }
}
