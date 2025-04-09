namespace AvionesDistribuidos.Models
{
    public class CountryViewModel
    {
        public string Name { get; set; }
        public string Capital { get; set; }
    }

    public class SeatConfiguration
    {
        public string SectionName { get; set; }
        public List<SeatRow> Rows { get; set; }
    }

    public class SeatRow
    {
        public string RowLabel { get; set; }
        public List<Seat> Seats { get; set; }
    }

    public class Seat
    {
        public string SeatId { get; set; }
        public int? DatabaseId { get; set; } // id_asiento real
        public decimal? Price { get; set; }
        public int State { get; set; } = 0; // Estados: "0=libre", "1=reserva", "2=venta", "3=devolucion", "empty", etc.
    }
}
