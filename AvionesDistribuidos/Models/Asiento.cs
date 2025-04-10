using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvionesDistribuidos.Models
{
    [Table("Asiento")]
    public class Asiento
    {
        [Key]
        [Column("id_asiento")]
        public int Id { get; set; }

        [Required]
        [Column("id_vuelo")]
        public int VueloId { get; set; }

        [Required]
        [Column("precio")]
        public decimal Precio { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("numero_asiento")]
        public string NumeroAsiento { get; set; }

        [Required]
        [Column("clase")]
        public int Clase { get; set; }

        [ForeignKey("VueloId")]
        public Vuelo Vuelo { get; set; }

        public virtual EstadoAsientosVuelo EstadoAsiento { get; set; }
    }
}
