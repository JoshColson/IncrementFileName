using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace FileRenamer
{
    enum UserInput
    {
        Increment,
        Decrement,
        Yes,
        No,
        Default
    }


    class Program
    {
        private static string CapitalCase(string input)
        {
            if (input.Length > 0)
            {
                return input.ToString().Substring(0, 1).ToUpper() + input.ToString().Substring(1).ToLower();
            }
            return input;
        }

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("\nEnter the directory path:");
                string dirPath = "";

                while (dirPath == "")
                {
                    dirPath = Console.ReadLine();

                    if (!Directory.Exists(dirPath))
                    {
                        Console.WriteLine("Invalid path. Please enter an existing directory path");
                        dirPath = "";
                    }
                    else if (!File.GetAttributes(dirPath).HasFlag(FileAttributes.Directory))
                    {
                        Console.WriteLine("Invalid path. Please enter an existing directory path, not a file path.");
                        dirPath = "";
                    }
                }






                Console.WriteLine("\nEnter 'increment' or 'decrement':");
                UserInput operation = UserInput.Default;
                while (operation == UserInput.Default)
                {
                    string input = CapitalCase(Console.ReadLine());

                    if (Enum.TryParse<UserInput>(input, true, out operation))
                    {
                        if (operation != UserInput.Increment && operation != UserInput.Decrement)
                        {
                            Console.WriteLine("Invalid input. Please enter 'increment' or 'decrement'");
                            operation = UserInput.Default;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter 'increment' or 'decrement'");
                    }
                }

                int incrementBy = 0;
                while (incrementBy == 0)
                {
                    Console.WriteLine("\nEnter the number to increment or decrement by:");
                    if (!int.TryParse(Console.ReadLine(), out incrementBy))
                    {
                        Console.WriteLine("Invalid input. Please enter an integer.");
                    }
                }

                int startNum = int.MinValue;
                while (startNum == int.MinValue)
                {
                    Console.WriteLine("\nEnter the start number:");
                    if (!int.TryParse(Console.ReadLine(), out startNum))
                    {
                        Console.WriteLine("Invalid input. Please enter an integer.");
                    }
                }

                int endNum = int.MaxValue;
                while (endNum == int.MaxValue)
                {
                    Console.WriteLine("\nEnter the end number:");
                    if (!int.TryParse(Console.ReadLine(), out endNum))
                    {
                        Console.WriteLine("Invalid input. Please enter an integer.");
                    }
                }
                Console.WriteLine("\n");
                string[] oldFiles = Directory.GetFiles(dirPath);
                Dictionary<string, string> oldNewPathPairings = new Dictionary<string, string>();

                for (int i = 0; i < oldFiles.Length; i++)
                {
                    string oldFileName = Path.GetFileNameWithoutExtension(oldFiles[i]);
                    string extension = Path.GetExtension(oldFiles[i]);
                    int oldNum;
                    int num;

                    Match match = Regex.Match(oldFileName, @"\d+");


                    if (match.Success)
                    {
                        bool success = int.TryParse(match.Value, out oldNum);
                        if (success)
                        {
                            if (!(oldNum <= endNum && oldNum >= startNum || oldNum >= endNum && oldNum <= startNum))
                            {

                                continue;
                            }

                            if (operation == UserInput.Increment)
                            {
                                num = oldNum + incrementBy;
                            }
                            else
                            {
                                num = oldNum - incrementBy;
                            }
                            string newFileName = oldFileName.Replace(oldNum.ToString(), num.ToString());



                            string tempFileName = Guid.NewGuid().ToString();
                            string tempFilePath = Path.Combine(Path.GetDirectoryName(oldFiles[i]), tempFileName + extension);
                            Console.WriteLine($"Planning to rename {oldFileName + extension} to {newFileName + extension}");
                            Console.WriteLine($"Changing file {oldFileName + extension} to a temporary name.");
                            oldNewPathPairings.Add(tempFilePath, Path.Combine(Path.GetDirectoryName(oldFiles[i]), newFileName + extension));
                            File.Move(oldFiles[i], tempFilePath);
                            File.Delete(oldFiles[i]);
                        }
                        

                    }
                }
                if (oldNewPathPairings.Count>0 && operation == UserInput.Increment)
                {
                    oldNewPathPairings = oldNewPathPairings.OrderByDescending(pair => int.Parse(Regex.Match(pair.Value, @"\d+").Value))
                                         .ToDictionary(pair => pair.Key, pair => pair.Value);
                }
                else if (oldNewPathPairings.Count > 0)
                {
                    oldNewPathPairings = oldNewPathPairings.OrderBy(pair => int.Parse(Regex.Match(pair.Value, @"\d+").Value))
                                                                 .ToDictionary(pair => pair.Key, pair => pair.Value);
                }

                if (oldNewPathPairings.Count > 0)
                {
                    foreach (var pathPairing in oldNewPathPairings)
                    {
                        if (!Directory.Exists(pathPairing.Value))
                        {
                            Console.WriteLine($"Changing file {Path.GetFileName(pathPairing.Key)} to new name: {pathPairing.Value}.");
                            File.Move(pathPairing.Key, pathPairing.Value);
                        }
                    }
                }

                UserInput answer = UserInput.Default;
                Console.WriteLine("\nWould you like to do it again? (Yes, No)");
                while (answer==UserInput.Default)
                {
                    string input = CapitalCase(Console.ReadLine().ToLower());
                    if (Enum.TryParse<UserInput>(input, true, out answer))
                    {
                        if (answer!=UserInput.Yes && answer != UserInput.No)
                        {
                            Console.WriteLine("Invalid response, answer 'Yes' or 'No'");
                            answer = UserInput.Default;
                        }
                        else if (answer == UserInput.No)
                        {
                            return;
                        }
                        else
                        {
                            break;
                        }
                    }

                }
                
            }
        }
    }
}
