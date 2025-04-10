using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AvionesDistribuidos.Models
{
    public class EstadoAsientosVuelo
    {
        [Key]
        [Column("id_estado")]
        public int Id { get; set; }

        [Required]
        [Column("id_vuelo")]
        public int? VueloId { get; set; }

        [ForeignKey("VueloId")]
        public Vuelo? Vuelo { get; set; }

        [Required]
        [Column("id_asiento")]
        public int? AsientoId { get; set; }

        [ForeignKey("AsientoId")]
        public Asiento? Asiento { get; set; }

        [Column("id_pasajero")]
        public int? PasajeroId { get; set; }

        [ForeignKey("PasajeroId")]
        public Pasajero? Pasajero { get; set; }

        [Required]
        [Column("estado")]
        public string? Estado { get; set; }

        [Required]
        [Column("fecha_hora_actualizacion")]
        public long? FechaHoraActualizacion { get; set; }

        [MaxLength(100)]
        [Column("servidor_origen")]
        public string? ServidorOrigen { get; set; }

        [Column("vector_clock")]
        public string? VectorClock { get; set; }
    }
}
