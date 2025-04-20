namespace MyWebApp.Models
{
    public class FarmerItemLog
    {
        public int LogID { get; set; }
        public int ItemID { get; set; }
        public string Category { get; set; }
        public string ItemName { get; set; }
        public DateTime ActionDate { get; set; }
    }
}