using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvionesDistribuidos.Models
{
    public class Ciudad
    {
        [Key]
        public int Id_Ciudad { get; set; }

        [Required]
        public int Id_Pais { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [ForeignKey("Id_Pais")]
        public Pais Pais { get; set; }

        public ICollection<Destino> Destinos { get; set; }
    }
}
