using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvionesDistribuidos.Models
{
    [Table("Avion")]
    public class Avion
    {
        [Key]
        [Column("id_avion")]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("nombre")]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("matricula")]
        public string Matricula { get; set; }

        [Required]
        [Column("ultimo_digito_1")]
        public int UltimoDigito1 { get; set; }

        [Required]
        [Column("ultimo_digito_2")]
        public int UltimoDigito2 { get; set; }

        [Required]
        [MaxLength(1)]
        [Column("ultima_letra_1")]
        public string UltimaLetra1 { get; set; }

        [Required]
        [MaxLength(1)]
        [Column("ultima_letra_2")]
        public string UltimaLetra2 { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("clase")]
        public string Clase { get; set; }

        [Required]
        [Column("horas_vuelo")]
        public int HorasVuelo { get; set; }

        [Required]
        [Column("ciclos_vuelo")]
        public int CiclosVuelo { get; set; }

        [Required]
        [Column("distancia_recorrida")]
        public decimal DistanciaRecorrida { get; set; }
    }
}
