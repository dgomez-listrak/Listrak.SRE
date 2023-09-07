using System;
using System.Collections.Generic;

namespace Listrak.SRE.Integrations.OpsGenie.Models
{
    public class Source
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class Alert
    {
        public long UpdatedAt { get; set; }
        public List<string> Tags { get; set; }
        public List<string> Teams { get; set; }
        public List<string> Recipients { get; set; }
        public string Message { get; set; }
        public string Username { get; set; }
        public string AlertId { get; set; }

        public string
            Source
        {
            get;
            set;
        } // Note: This might be confusing given that there's also a 'source' property outside the 'alert'. Consider renaming if possible.

        public string
            Alias
        { get; set; } // 'Alias' is a reserved keyword in C#, but it's okay to use it as a property name.

        public string TinyId { get; set; }
        public long CreatedAt { get; set; }
        public string UserId { get; set; }
        public string Entity { get; set; }
    }

    public class WebhookPayload
    {
        public Source Source { get; set; }
        public Alert Alert { get; set; }
        public string Action { get; set; }
        public string IntegrationId { get; set; }
        public string IntegrationName { get; set; }
    }
}