namespace MyWebApp.Models
{
    public class FarmerItemDeleteLog
    {
        public int LogID { get; set; }
        public int ItemID { get; set; }
        public string Category { get; set; }
        public string ItemName { get; set; }
        public DateTime DeletedDate { get; set; }
        public string DeletedBy { get; set; }
    }
}