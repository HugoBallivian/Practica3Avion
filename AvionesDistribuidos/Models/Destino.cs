using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvionesDistribuidos.Models
{
    public class Destino
    {
        [Key]
        public int Id_Destino { get; set; }

        [Required]
        public int Id_Ciudad { get; set; }

        [Required]
        [MaxLength(150)]
        public string Aeropuerto { get; set; }

        [Required]
        [MaxLength(50)]
        public string Descripcion_Corta { get; set; }

        // Propiedad de navegación
        [ForeignKey("Id_Ciudad")]
        public Ciudad Ciudad { get; set; }
    }
}
