using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvionesDistribuidos.Models
{
    [Table("Pais")]
    public class Pais
    {
        [Key]
        [Column("id_pais")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(3)]
        [Column("codigo_iso")]
        public string CodigoIso { get; set; }

        public ICollection<Ciudad> Ciudades { get; set; }
    }
}
