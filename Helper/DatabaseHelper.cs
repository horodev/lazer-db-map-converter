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
        public DirectoryHelper DirectoryHelper { get; private set; }
        private string _path;

        public List<int> FailedMapConversions { get; private set; }

        public DatabaseHelper(string path, ConversionType type)
        {
            _path = path;
            DirectoryHelper = new DirectoryHelper(_path, type);
            FailedMapConversions = new List<int>();
            StartConverting();
            Console.WriteLine("All maps have been converted!");
        }

        private void StartConverting()
        {
            Console.WriteLine("Fetching Maps from Database...");

            var conn = new SQLiteConnection(Path.Combine(_path,"client.db"));
            var beatmapSetInfo = conn.Query<BeatmapSetInfo>("Select * from BeatmapSetInfo");
            beatmapSetInfo = beatmapSetInfo.Where(s => s.OnlineBeatmapSetID != null).ToList();

            var maxCount = beatmapSetInfo.Count;
            var currentCount = 1;

            Console.WriteLine($"I've found {maxCount} Maps!");

            foreach (var bsi in beatmapSetInfo)
            {
                Console.WriteLine($"({currentCount} / {maxCount})");

                var metadata = conn.Query<BeatmapMetadata>("Select * from BeatmapMetadata WHERE ID = ?", bsi.MetadataID);
                var beatmapSetFileInfo = conn.Query<BeatmapSetFileInfo>("Select * from BeatmapSetFileInfo WHERE BeatmapSetInfoID = ?", bsi.ID);

                DirectoryHelper.Reset(); // IMPORTANT
                DirectoryHelper.SetMapRootDirectory(bsi, metadata[0]);
                bool canBeCreated = true;

                foreach (var setFile in beatmapSetFileInfo)
                {
                    // Every BeatmapSetFileInfo Entriy has a single Foreignkey, pointing to exactly one FileInfo, so the
                    // following Query will always return one object.
                    var fileInfo = conn.Query<FileInfo>("Select * from FileInfo where ID = ?", setFile.FileInfoID)[0];
                    canBeCreated = DirectoryHelper.AddContent(setFile, fileInfo);

                    if (!canBeCreated) // A file isn't eligible to be created
                        break;
                }
                if (canBeCreated)
                {
                    DirectoryHelper.Create();
                    Console.WriteLine($"\t > Success!");
                }
                else
                {
                    FailedMapConversions.Add((int)bsi.OnlineBeatmapSetID);
                    Console.WriteLine($"\t > {bsi.OnlineBeatmapSetID} ({metadata[0].Artist} - {metadata[0].Title}) added to failed conversion list.");
                }
                currentCount++;
            }
            conn.Dispose();
        }

    }
}
