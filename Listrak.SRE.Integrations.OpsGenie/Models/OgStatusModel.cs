using System.Collections.Generic;
using System;

namespace Listrak.SRE.Integrations.OpsGenie.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Data
    {
        public bool Seen { get; set; }
        public string Id { get; set; }
        public string TinyId { get; set; }
        public string Alias { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public bool Acknowledged { get; set; }
        public bool IsSeen { get; set; }
        public List<object> Tags { get; set; }
        public bool Snoozed { get; set; }
        public int Count { get; set; }
        public DateTime LastOccurredAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Source { get; set; }
        public string Owner { get; set; }
        public string Priority { get; set; }
        public List<object> Teams { get; set; }
        public List<object> Responders { get; set; }
        public Integration Integration { get; set; }
        public Report Report { get; set; }
        public string OwnerTeamId { get; set; }
        public List<object> Actions { get; set; }
        public string Entity { get; set; }
        public string Description { get; set; }
        public Details Details { get; set; }
    }


    public class Integration
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class Report
    {
        public int AckTime { get; set; }
        public string AcknowledgedBy { get; set; }
    }

    public class OpsGenieStatus
    {
        public Data Data { get; set; }
        public double Took { get; set; }
        public string RequestId { get; set; }
    }

}
