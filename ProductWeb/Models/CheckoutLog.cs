namespace ProductWeb.Models
{
    using System;
    using System.Collections.Generic;

    public class CheckoutLog
    {
        public string UserId { get; set; }
        public string UserName { get; set; }        
        public string UserEmail { get; set; }      
        public string PhoneNumber { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public List<CheckoutLogItem> Items { get; set; } = new List<CheckoutLogItem>();
    }

}
