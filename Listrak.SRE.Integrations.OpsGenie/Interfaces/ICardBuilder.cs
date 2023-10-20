using Listrak.SRE.Integrations.OpsGenie.Models;
using Microsoft.Bot.Schema;

namespace Listrak.SRE.Integrations.OpsGenie.Interfaces;

public interface ICardBuilder
{
    Attachment BuildCard(AlertData myData);
    Attachment AddUnAckButton(Attachment card, AlertData myData);
    Attachment AddAckButton(Attachment card, AlertData myData);
    Attachment AddCloseButton(Attachment card, AlertData myData);
    Attachment AddNoteButton(Attachment card, AlertData myData);
    Attachment AddIncidentButton(Attachment card, AlertData myData);
}