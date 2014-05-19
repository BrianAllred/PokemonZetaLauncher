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

namespace PokemonZetaLauncher
{
    class Program
    {
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
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Copies all files that end in ".rxdata" from "fromDir" to "toDir"
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
                        Console.WriteLine(e.ToString());
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
                Console.WriteLine(e.ToString());
                _error = true;
            }
        }
    }
}
