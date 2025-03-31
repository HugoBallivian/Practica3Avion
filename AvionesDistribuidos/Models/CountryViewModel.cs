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
        public string State { get; set; } = "libre"; // Estados: "libre", "reserva", "venta", "devolucion", "empty", etc.
    }
}
