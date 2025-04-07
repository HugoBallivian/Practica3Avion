using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvionesDistribuidos.Models
{
    [Table("Vuelo")]
    public class Vuelo
    {
        [Key]
        [Column("id_vuelo")]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("codigo_vuelo")]
        public string CodigoVuelo { get; set; }

        [Required]
        [Column("id_ruta")]
        public int RutaId { get; set; }

        [Required]
        [Column("fecha_salida")]
        public DateTime FechaSalida { get; set; }

        [Required]
        [Column("fecha_llegada")]
        public DateTime FechaLlegada { get; set; }

        [Required]
        [Column("id_avion")]
        public int AvionId { get; set; }

        [Required]
        [Column("estado")]
        public int Estado { get; set; }  // 1: Abierto, 2: Volando, 3: Cerrado

        [Required]
        [MaxLength(30)]
        [Column("gate")]
        public string Gate { get; set; }

        // Propiedades de navegación
        [ForeignKey("AvionId")]
        public Avion Avion { get; set; }

        [ForeignKey("RutaId")]
        public RutaComercial Ruta { get; set; }
    }
}
