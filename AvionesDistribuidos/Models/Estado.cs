namespace AvionesDistribuidos.Models
{
    public class Estado
    {
        public int Id { get; set; }
        public int? UbicacionPaisId { get; set; }
        public string EstadoNombre { get; set; }
        public Pais Pais { get; set; }
    }
}
