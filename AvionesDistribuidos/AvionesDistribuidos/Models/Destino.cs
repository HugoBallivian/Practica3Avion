using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvionesDistribuidos.Models
{
    [Table("Destino")]
    public class Destino
    {
        [Key]
        [Column("id_destino")]
        public int Id { get; set; }

        [Required]
        [Column("id_ciudad")]
        public int CiudadId { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("aeropuerto")]
        public string Aeropuerto { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("descripcion_corta")]
        public string descripcion_corta { get; set; }

        [ForeignKey("CiudadId")]
        public Ciudad Ciudad { get; set; }
    }
}
