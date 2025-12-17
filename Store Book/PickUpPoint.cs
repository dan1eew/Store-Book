namespace Store_Book
{
    public class PickUpPoint
    {
        public int PickUpPointID { get; set; }
        public int IndexAdrees { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public int HouseNumber { get; set; }

        public string FullAddress => $"г. {City}, ул. {Street}, д. {HouseNumber}, индекс: {IndexAdrees}";
    }
}