using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvionesDistribuidos.Models
{
    [Table("Ciudad")]
    public class Ciudad
    {
        [Key]
        [Column("id_ciudad")]
        public int Id { get; set; }

        [Required]
        [Column("id_pais")]
        public int PaisId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; }

        [ForeignKey("PaisId")]
        public Pais Pais { get; set; }

        // Relación uno a muchos: una ciudad tiene varios destinos
        public ICollection<Destino> Destinos { get; set; }
    }
}
