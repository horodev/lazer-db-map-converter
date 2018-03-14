using System;
using System.Collections.Generic;
using System.Text;

namespace LazerDBMapConverter
{
    public class BeatmapMetadata
    {
        public int ID { get; set; }
        public string Artist { get; set; }
        public string ArtistUnicode { get; set; }
        public string AudioFile { get; set; }
        public string Author { get; set; }
        public string BackgroundFile { get; set; }
        public string PreviewTime { get; set; }
        public string Source { get; set; }
        public string Tags { get; set; }
        public string Title { get; set; }
        public string TitleUnicode { get; set; }
    }
}
