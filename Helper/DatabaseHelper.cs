using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace LazerDBMapConverter
{
    public class DatabaseHelper
    {
        private List<BeatmapMetadata> Metadata { get; set; }
        private string _path;

        public DatabaseHelper(string path)
        {
            _path = path;
            var list = StartConverting();
            Console.WriteLine("All maps have been converted!");

            if(list.Count > 0)
            {
                if(!File.Exists("failed.txt"))
                    File.CreateText("failed.txt");
                var writer = File.AppendText("failed.txt");
                foreach (var failed in list)
                    writer.WriteLine(failed.Value);
                writer.Dispose();
                Console.WriteLine("Some maps couldn't be converted, check the failed.txt file");
            }
        }

        private Dictionary<int, string> StartConverting()
        {
            var invalidMaps = new Dictionary<int, string>();

            Console.WriteLine("Fetching Maps from Database...");
            var conn = new SQLiteConnection(Path.Combine(_path,"client.db"));

            var beatmapSetInfo = conn.Query<BeatmapSetInfo>("Select * from BeatmapSetInfo");

            beatmapSetInfo = beatmapSetInfo.Where(s => !string.IsNullOrEmpty(s.OnlineBeatmapSetID)).ToList();


            var maxCount = beatmapSetInfo.Count;
            var currentCount = 1;

            Console.WriteLine($"I've found {maxCount} Maps!");

            foreach (var bsi in beatmapSetInfo)
            {
                Console.WriteLine($"({currentCount} / {maxCount})");
                Console.WriteLine("\t > Trying to create directory..");

                var rootPath = CreateMapDirectory(bsi);
                if (rootPath != null)
                {
                    Console.WriteLine($"\t > Directory created at '{rootPath}'!");
                    Console.WriteLine("");

                    var beatmapSetFileInfo = conn.Query<BeatmapSetFileInfo>("Select * from BeatmapSetFileInfo WHERE BeatmapSetInfoID = ?", bsi.ID);

                    Console.WriteLine($"\t > Converting...");

                    foreach (var setFile in beatmapSetFileInfo)
                    {
                        var fileInfo = conn.Query<FileInfo>("Select * from FileInfo where ID = ?", setFile.FileInfoID);
                        AddMapFile(rootPath, setFile, fileInfo);
                    }

                    Console.WriteLine($"\t > Success!");

                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\t > The conversion of the Beatmap with Set ID {bsi.OnlineBeatmapSetID} failed, as it's path was invalid");

                    Metadata = conn.Query<BeatmapMetadata>("Select * from BeatmapMetadata WHERE ID = ?", bsi.MetadataID);

                    invalidMaps.Add(int.Parse(bsi.OnlineBeatmapSetID), $"{bsi.OnlineBeatmapSetID} {Metadata[0].Artist} - {Metadata[0].Title}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                currentCount++;
            }
            conn.Dispose();
            return invalidMaps;
        }
        private void AddMapFile(string rootPath, BeatmapSetFileInfo setFile, List<FileInfo> fileInfos)
        {
            foreach(var fInfo in fileInfos)
            {
                var firstLetter = fInfo.Hash[0].ToString();
                var secondLetter = fInfo.Hash[1].ToString();
                var currentPath = Path.Combine(_path, "files", firstLetter, firstLetter + secondLetter, fInfo.Hash);

                var newFilePath = Path.Combine(rootPath, setFile.Filename);

                if(newFilePath.Contains('/'))
                    Directory.CreateDirectory(newFilePath.Substring(0, newFilePath.LastIndexOf('/')));


                if (File.Exists(currentPath))
                {
                    //Workaround for PathToLong Exception thrown by File.Copy
                    //currentPath = @"\\?\" + currentPath;
                    //newFilePath = @"\\?\" + newFilePath;
                    File.Copy(currentPath, newFilePath);
                }
            }
        }
        private string CreateMapDirectory(BeatmapSetInfo bsi)
        {
            var conn = new SQLiteConnection(Path.Combine(_path, "client.db"));
            Metadata = conn.Query<BeatmapMetadata>("Select * from BeatmapMetadata WHERE ID = ?", bsi.MetadataID);
            conn.Dispose();

            var dirPath = $"{bsi.OnlineBeatmapSetID} {Metadata[0].Artist} - {Metadata[0].Title}";

            dirPath = DatabaseHelper.ContainsAnyInvalidCharacters(dirPath) ?
                DatabaseHelper.ReplaceInvalidCharacter(dirPath) :
                dirPath;

            dirPath = Path.Combine(Path.Combine("Maps", dirPath));

            Directory.CreateDirectory(dirPath);
            return dirPath;
        }

        public static bool ContainsAnyInvalidCharacters(string path)
        {
            for (var i = 0; i < path.Length; i++)
            {
                int c = path[i];

                if (c == '\"' || c == '<' || c == '>' || c == '|' || c == '*' || c == '?' || c < 32 || c == ':')
                    return true;
            }

            return false;
        }
        public static string ReplaceInvalidCharacter(string path)
        {
            for (var i = 0; i < path.Length; i++)
            {
                char c = path[i];

                if (c == '\"' || c == '<' || c == '>' || c == '|' || c == '*' || c == '?' || c < 32 || c == ':')
                    path = path.Replace(c, '-');
            }

            return path;
        }
    }
}
