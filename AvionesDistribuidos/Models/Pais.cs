namespace AvionesDistribuidos.Models
{
    public class Pais
    {
        public int Id { get; set; }
        public string PaisNombre { get; set; }
        public ICollection<Estado> Estados { get; set; }
    }
}
