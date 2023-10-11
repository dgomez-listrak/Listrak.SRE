using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Listrak.SRE.Integrations.OpsGenie.Interfaces;
using Listrak.SRE.Integrations.OpsGenie.Models;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Listrak.SRE.Integrations.OpsGenie.Implementations
{

    public class MySqlAdapter : IMySqlAdapter
    {
        string connectionString = "Server=listraksre.mysql.database.azure.com;Database=listrak_sre;User ID=sreadmin;Password=*aDL?J]9id;SslMode=Preferred;";
        private readonly ILogger _logger;
        public MySqlAdapter(ILogger<OpsGenieHandler> logger)
        {
            _logger = logger;
        }

        public void LogToMysql(OpsGenieNotification payload)
        {
            try
            {
                var message = payload.Action != null ? payload.Alert : payload.Data;
                string upsertSQL = @"
                    INSERT INTO og_alerts (alertId, conversationId, dateCreated, dateModified, status, priority, cardData, alertPayload) 
                    VALUES (@alertId, @conversationId, @dateCreated, @dateModified, @status, @priority, @cardData, @alertPayload) 
                    ON DUPLICATE KEY UPDATE 
                        dateModified = @dateModified,
                        status = @status,
                        priority = @priority,
                        cardData = @cardData,
                        alertPayload = @alertPayload,
                        conversationId = IF(conversationId IS NULL OR conversationId = '', @conversationId, conversationId);
                    ";


                using MySqlConnection connection = new MySqlConnection(connectionString);
                connection.Open();


                using MySqlCommand cmd = new MySqlCommand(upsertSQL, connection);
                //cmd.Parameters.AddWithValue("@alertIndex", /* Assuming you have an alertIndex in payloadToSend */);
                cmd.Parameters.AddWithValue("@alertId", message.UnifiedAlertId);
                cmd.Parameters.AddWithValue("@conversationId", message.ConversationId /* Assuming you have a conversationId in payloadToSend */);
                cmd.Parameters.AddWithValue("@dateCreated", DateTime.Now /* Assuming you have a dateCreated in payloadToSend */);
                cmd.Parameters.AddWithValue("@dateModified", DateTime.Now /* Assuming you have a dateModified in payloadToSend */);
                cmd.Parameters.AddWithValue("@status", message.Status);
                cmd.Parameters.AddWithValue("@priority", message.Priority);
                cmd.Parameters.AddWithValue("@cardData", string.Empty /* Assuming you have a cardData in payloadToSend */);
                cmd.Parameters.AddWithValue("@alertPayload", string.Empty /* Assuming you have a serialized form of payloadToSend you want to store */);

                cmd.ExecuteNonQuery();

                connection.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} - {ex.InnerException} -{ex.StackTrace}");
            }
        }

        public string GetConverationId(string alertId)
        {
            string checkQuery = "SELECT conversationId FROM og_alerts WHERE alertId = @alertId";
            string existingConversationId = null;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using MySqlCommand cmdCheck = new MySqlCommand(checkQuery, connection);
                cmdCheck.Parameters.AddWithValue("@alertId", alertId);

                // Execute the query and get the conversationId
                var reader = cmdCheck.ExecuteReader();

                if (reader.Read()) // Means alertId exists in the database
                {
                    existingConversationId = reader.GetString("conversationId");
                }

                connection.Close();
            }

            return existingConversationId;
        }

        public bool CheckExistingAlert(string alertId)
        {
            string checkQuery = "SELECT alertId FROM og_alerts WHERE alertId = @alertId";
            string existingAlertId = null;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using MySqlCommand cmdCheck = new MySqlCommand(checkQuery, connection);
                cmdCheck.Parameters.AddWithValue("@alertId", alertId);

                // Execute the query and get the conversationId
                var reader = cmdCheck.ExecuteReader();

                if (reader.Read()) // Means alertId exists in the database
                {
                    existingAlertId = reader.GetString("conversationId");
                }

                connection.Close();
            }
            return !string.IsNullOrEmpty(existingAlertId);
        }
    }
}