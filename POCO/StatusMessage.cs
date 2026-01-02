using System.Collections.Generic;

namespace TestProject.POCO
{
    public class StatusMessage
    {
        public string? Status { get; set; }
        public string? Code { get; set; }
        public string? Message { get; set; }
        public dynamic? Metadata { get; set; }
        public dynamic? data { get; set; }
        public List<string>? ErrorMessages { get; set; }
    }

}
