using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace shreddie
{
    class Program
    {
        static Random rnd = new Random();
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                for (int x = 0; x < args.Length; x++)
                {
                    Console.Title = x + " / " + args.Length;
                    try
                    {
                        // get the file attributes for file or directory
                        FileAttributes attr = File.GetAttributes(args[x]);

                        //detect whether its a directory or file
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            Console.WriteLine("Processing directory... " + args[x]);
                            string[] Files = Directory.GetFiles(args[x], "*.*", SearchOption.AllDirectories);
                            int index = 0;
                            foreach (string FilePath in Files)
                            {
                                index++;
                                ShredFile(FilePath);
                                Console.Title = x + " / " + args.Length + "  (" + index + " / " + Files.Length + ")";
                            }
                            Directory.Delete(args[x]);
                        }
                        else
                        {
                            ShredFile(args[x]);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        static string GetRandomString(int Length)
        {
            const string CHARS = "QAZWSXEDCRFVTGBYHNUJMIKOLP01234567890";
            string ret = "";

            for (int i = 0; i < Length; i++)
            {
                ret += CHARS[rnd.Next(0, CHARS.Length)];
            }

            return ret;
        }

        static DateTime GetRandomDateTime()
        {
            return new DateTime(rnd.Next(1975, 2050), rnd.Next(1, 12), rnd.Next(1, 25), rnd.Next(1, 24), rnd.Next(1, 59), rnd.Next(1, 59));
        }

        static void ShredFile(string FilePath)
        {
            Console.WriteLine("Processing " + FilePath);

            //overwrite the content(data)
            using (FileStream stream = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                byte[] Data = new byte[1000000];

                int passes = rnd.Next(4, 9);

                if (stream.Length > 1000000000)
                    passes = 3;

                for (int i = 0; i < passes; i++)
                {
                    stream.Position = 0;

                    int Percentage = 0;
                    for (long j = 0; j < stream.Length || stream.Length == 0; j += Data.Length)
                    {
                        rnd.NextBytes(Data);
                        stream.Write(Data, 0, Data.Length);
                        stream.Flush();

                        int tempPercentage = (int)(((double)stream.Position / (double)stream.Length) * 100D);
                        if (Percentage != tempPercentage)
                        {
                            Percentage = tempPercentage;
                            Console.Write(".");
                        }
                    }
                    Console.WriteLine("\r\nPass " + (i + 1) + " / " + passes);
                }
            }

            //destroying/overwriting existing file information
            //even though the content(data) might be gone, the file name might give the contents away
            //overwrite the information a couple times to be sure it's not recoverable

            //1. File Renaming
            FileInfo fileInf = new FileInfo(FilePath);

            for (int i = 0; i < rnd.Next(9, 16); i++)
            {
                string newFileName = "";
                string TargetFile = "";

                do
                {
                    newFileName = GetRandomString(fileInf.Name.Length + fileInf.Extension.Length + 1);
                    TargetFile = fileInf.Directory.FullName + "\\" + newFileName;
                } while (File.Exists(TargetFile));

                File.Move(fileInf.FullName, TargetFile);
                fileInf = new FileInfo(TargetFile);
            }


            //2. Set random file Dates
            for (int i = 0; i < rnd.Next(9, 16); i++)
            {
                //try to apply the Dates if errors occur it's mostly Access Denied etc...
                try
                {
                    File.SetCreationTime(fileInf.FullName, GetRandomDateTime());
                    File.SetCreationTimeUtc(fileInf.FullName, GetRandomDateTime());
                    File.SetLastAccessTime(fileInf.FullName, GetRandomDateTime());
                    File.SetLastAccessTimeUtc(fileInf.FullName, GetRandomDateTime());
                    File.SetLastWriteTime(fileInf.FullName, GetRandomDateTime());
                    File.SetLastWriteTimeUtc(fileInf.FullName, GetRandomDateTime());
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(500);
                }
            }

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    File.Delete(fileInf.FullName);
                    break;
                }
                catch { Thread.Sleep(250); }
            }

            if (File.Exists(fileInf.FullName))
            {
                Console.WriteLine("Unable to delete file " + fileInf.FullName);
            }
        }
    }
}