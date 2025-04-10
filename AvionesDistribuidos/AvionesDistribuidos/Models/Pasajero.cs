using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvionesDistribuidos.Models
{
    [Table("Pasajero")]
    public class Pasajero
    {
        [Key]
        public int numero_pasaporte { get; set; }

        [Required]
        [MaxLength(255)]
        public string nombre_completo { get; set; }
    }
}
