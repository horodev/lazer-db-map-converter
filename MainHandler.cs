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

        public static void Convert(ConversionType type, string path = "")
        {
            if (path == "")
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu");

            Directory.CreateDirectory("Maps");
            DatabaseHelper dh = new DatabaseHelper(path, type);
        }
    }
}
