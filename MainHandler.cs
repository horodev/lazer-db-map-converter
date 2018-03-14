using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazerDBMapConverter
{
    public class MainHandler
    {
        public static void Main(string[] args)
        {
            InputHandler handler = new InputHandler(args);
        }

        public static void Convert(bool oszConversion)
        {
            MainHandler.Convert();
            if(oszConversion)
            {
                foreach(var dir in Directory.EnumerateDirectories("Maps"))
                {
                    ZipFile.CreateFromDirectory(dir, dir+".osz");
                    foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
                        File.Delete(file);
                    Directory.Delete(dir, true);
                }
            }
        }

        public static void Convert(string path = "")
        {
            if(path == "")
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu");

            Directory.CreateDirectory("Maps");
            DatabaseHelper dh = new DatabaseHelper(path);
        }
    }
}
