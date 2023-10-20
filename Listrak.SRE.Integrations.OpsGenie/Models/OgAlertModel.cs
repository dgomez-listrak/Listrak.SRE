using Newtonsoft.Json;

namespace Listrak.SRE.Integrations.OpsGenie.Models
{

    public class OpsGenieNotification
    {
        public string Action { get; set; }
        public AlertData Alert { get; set; }
        public AlertData Data { get; set; }
    }

    public class AlertData
    {
        [JsonProperty("message")] public string Message { get; set; }
        public string Description { get; set; }

        [JsonProperty("alertId")] public string AlertIdFromAlert { get; set; }

        [JsonProperty("id")] public string IdFromData { get; set; }

        public string Priority { get; set; }
        public string Status { get; set; }
        public string Source { get; set; }
        public string ConversationId { get; set; }
        public bool Ackwnowledged { get; set; }

        // This is a convenience property to unify alertId and id
        public string UnifiedAlertId => AlertIdFromAlert ?? IdFromData;
    }
}