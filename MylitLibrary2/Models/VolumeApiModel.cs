using System;

namespace MylitLibrary.Models
{
    public class VolumeApiModel
    {
        public int Id { get; set; }
        public int IdUser { get; set; }
        public string IdBook { get; set; }
        public string TitleBook { get; set; }
        public string Author { get; set; }
        public string DatePublicBook { get; set; }
        public string CoverUrl { get; set; }
        public string StatusBook { get; set; }
        public bool Favorite { get; set; }
        public string Coments { get; set; }
        public int? NoteOfBook { get; set; }
        public string Editorial { get; set; }
        public string Description { get; set; }
        public string ISBN { get; set; }
        public int Pages { get; set; }
        public string ImpressionTipe { get; set; }
        public string Categories { get; set; }
        public string Language { get; set; }

        public override string ToString()
        {
            return $"{TitleBook} - {string.Join(", ", Author)} ({DatePublicBook})";
        }
    }

}
