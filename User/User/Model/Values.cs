namespace User.Model
{
    public class Values
    {
        public int Id { get; set; }
        public string? BrokerID { get; set; }
        public NewOrder? newOrder { get; set; }
        public int? newOrderId { get; set; }
        public double? Value { get; set; }
        public bool? Accept { get; set; }
        public string? JopID { get; set; }

    }
}
