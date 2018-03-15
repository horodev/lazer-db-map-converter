using System;
using System.Collections.Generic;
using System.Text;

namespace LazerDBMapConverter
{
    public class BeatmapSetFileInfo
    {
        public int ID { get; set; }
        public int BeatmapSetInfoID { get; set; }
        public int FileInfoID { get; set; }
        public string Filename { get; set; }
    }
}
