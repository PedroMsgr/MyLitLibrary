using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MylitLibrary.Entities
{
    public class UserBooks
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
    }

}
