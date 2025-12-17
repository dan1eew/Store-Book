using System;

namespace Store_Book
{
    public class Order
    {
        public int OrderID { get; set; }
        public int OrderNumber { get; set; }
        public string OrderCode { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int HouseNumber { get; set; }
        public string FullNameCustomer { get; set; }
        public int CodeToReceive { get; set; }
        public string OrderStatus { get; set; }
        public int BookID { get; set; }
        public int PickUpPointID { get; set; }
        public int UserID { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string BookTitle { get; set; }
        public string PickUpAddress { get; set; }
    }
}