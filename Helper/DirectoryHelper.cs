using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazerDBMapConverter
{
    public enum ConversionType
    {
        Osz,
        Directory
    }

    public class DirectoryHelper
    {
        private List<string> currentFiles;
        private List<string> currentFilesHashPath;

        public ConversionType Type { get; set; }

        private string osuPath;

        public DirectoryHelper(string osuPath, ConversionType type)
        {
            this.osuPath = osuPath;
            currentFiles = new List<string>();
            currentFilesHashPath = new List<string>();

            Type = type;
        }

        private bool IsPathTooLong(string relativePath)
        {
            try
            {
                Path.GetFullPath(relativePath);
            }
            catch(PathTooLongException)
            {
                return true;
            }
            return false;
        }

        public void SetMapRootDirectory(BeatmapSetInfo beatmapSetInfo, BeatmapMetadata metadata, string rootPath = "") 
        {
            var beatmapName = $"{beatmapSetInfo.OnlineBeatmapSetID} {metadata.Artist} - {metadata.Title}";
            beatmapName = ContainsAnyInvalidCharacters(beatmapName) ?
                ReplaceInvalidCharacter(beatmapName) :
                beatmapName;

            var path = Path.Combine("Maps", beatmapName);
            currentFiles.Add(path);
        }

        public void Create()
        {
            if (Type == ConversionType.Osz)
                ConvertToOszAlt();
            else if (Type == ConversionType.Directory)
                ConvertToDirectory();
        }

        [Obsolete("use ConvertToOszAlt() instead. It should be faster and save more ressources")]
        private void ConvertToOsz()
        {
            ConvertToDirectory();

            var dir = currentFiles[0];

            Console.WriteLine("\t > Creating osz file from directory...");

            ZipFile.CreateFromDirectory(dir, dir + ".osz");
            foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
                File.Delete(file);
            Directory.Delete(dir, true);
        }

        private void ConvertToOszAlt()
        {
            var hashRoot = currentFilesHashPath[0];
            var mapRoot = currentFiles[0];

            Console.WriteLine($"\t > Creating osz {mapRoot}...");

            using (var fs = new FileStream(mapRoot + ".osz", FileMode.Create))
            {
                using (var zip = new ZipArchive(fs, ZipArchiveMode.Create))
                {
                    for (int i = 1; i < currentFiles.Count(); i++)
                    {
                        zip.CreateEntryFromFile(Path.Combine(hashRoot, currentFilesHashPath[i]), currentFiles[i]);
                    }
                    zip.Dispose();
                }
                fs.Dispose();
            }
        }

        private void ConvertToDirectory()
        {
            var hashRoot = currentFilesHashPath[0];
            var mapRoot = currentFiles[0];

            CreateRootDirectory();

            for(int i = 1; i < currentFiles.Count(); i++)
            {
                if(currentFiles[i].Contains('/'))
                {
                    // Some files are stored in subdirectories according to the db
                    // We could create this directory earlier, but doing it here
                    // enables to do all File and Directory creation in this method.

                    // Also in case of a PathTooLongException with a certain file we don't have to delete the created folder
                    // as we can be sure that the length of the path is fine.
                    Directory.CreateDirectory(Path.Combine(mapRoot, currentFiles[i].Substring(0, currentFiles[i].LastIndexOf('/'))));
                }
                File.Copy(Path.Combine(hashRoot, currentFilesHashPath[i]), Path.Combine(mapRoot, currentFiles[i]));
            }
        }

        public void Reset()
        {
            currentFiles.Clear();
            currentFilesHashPath.Clear();
            currentFilesHashPath.Add(Path.Combine(osuPath, "files"));
        }

        public bool AddContent(BeatmapSetFileInfo beatmapSetFileInfo, FileInfo fileInfo)
        {
            if(!IsPathTooLong(currentFiles[0] + @"\" + beatmapSetFileInfo.Filename))
            {
                currentFiles.Add(beatmapSetFileInfo.Filename);

                var firstLetter = fileInfo.Hash[0].ToString();
                var secondLetter = fileInfo.Hash[1].ToString();
                var hashPath = Path.Combine(firstLetter, firstLetter + secondLetter, fileInfo.Hash);

                // This path could potentially produce a PathTooLongException, however it is ignored for now until a proper Workaround is found
                currentFilesHashPath.Add(hashPath);
                return true;
            }
            return false;
        }

        private void CreateRootDirectory()
        {
            Console.WriteLine("\t > Trying to create directory..");

            var rootPath = CreateDirectory(currentFiles[0]);

            if(rootPath == null)
            {
                Console.WriteLine($"\t > [Error] Failed to create the directory (PathTooLong)");
                return;
            }
            Console.WriteLine($"\t > Successfully created directory - {rootPath}");
        }

        private string CreateDirectory(string directoryName, string rootPath = "")
        {
            if(!IsPathTooLong(rootPath + @"\" + directoryName))
            {
                var path = Path.Combine(Path.Combine(rootPath, directoryName));
                path = ContainsAnyInvalidCharacters(path) ?
                    ReplaceInvalidCharacter(path) :
                    path;

                Directory.CreateDirectory(path);
                return path;
            }
            return null;
        }

        public bool ContainsAnyInvalidCharacters(string path)
        {
            for (var i = 0; i < path.Length; i++)
            {
                int c = path[i];

                if (c == '\"' || c == '<' || c == '>' || c == '|' || c == '*' || c == '?' || c < 32 || c == ':')
                    return true;
            }

            return false;
        }
        public string ReplaceInvalidCharacter(string path)
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
