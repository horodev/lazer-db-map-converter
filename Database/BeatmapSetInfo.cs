using System;
using System.Collections.Generic;
using System.Text;

namespace LazerDBMapConverter
{
    public class BeatmapSetInfo
    {
        public int ID { get; set; }
        public bool DeletePending { get; set; }
        public string Hash { get; set; }
        public int? OnlineBeatmapSetID { get; set; }
        public int MetadataID { get; set; }
        public bool Protected { get; set; }
    }
}
