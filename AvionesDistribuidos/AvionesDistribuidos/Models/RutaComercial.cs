using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvionesDistribuidos.Models
{
    [Table("RutaComercial")]
    public class RutaComercial
    {
        [Key]
        [Column("id_ruta")]
        public int Id { get; set; }

        [Required]
        [Column("id_ciudad_origen")]
        public int CiudadOrigenId { get; set; }

        [Required]
        [Column("id_ciudad_destino")]
        public int CiudadDestinoId { get; set; }

        [Required]
        [Column("precio")]
        public decimal Precio { get; set; }

        [Required]
        [Column("duracion_promedio_minutos")]
        public int DuracionPromedioMinutos { get; set; }

        [Column("activa")]
        public int Activa { get; set; }

        [Required]
        [Column("distancia_km")]
        public int DistanciaKm { get; set; }

        [ForeignKey("CiudadOrigenId")]
        public Ciudad CiudadOrigen { get; set; }

        [ForeignKey("CiudadDestinoId")]
        public Ciudad CiudadDestino { get; set; }
    }
}
