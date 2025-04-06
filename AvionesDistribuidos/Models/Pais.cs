using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvionesDistribuidos.Models
{
    public class Pais
    {
        [Key]
        public int Id_Pais { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(3)]
        public string Codigo_Iso { get; set; }

        public ICollection<Ciudad> Ciudades { get; set; }
    }
}
