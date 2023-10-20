using AdaptiveCards;
using Listrak.SRE.Integrations.OpsGenie.Models;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Listrak.SRE.Integrations.OpsGenie.Interfaces;

namespace Listrak.SRE.Integrations.OpsGenie.Implementations
{
    public class CardBuilder : ICardBuilder
    {
        public Attachment BuildCard(AlertData myData)
        {
            AdaptiveCard card = new AdaptiveCard()
            {
                Body = new List<AdaptiveElement>()
        {
            new AdaptiveColumnSet()
            {
                Speak = "OpsGenie Alert",
                Columns = new List<AdaptiveColumn>()
                {
                    new AdaptiveColumn()
                    {
                        Width = "auto",
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveImage()
                            {
                                UrlString = "https://play-lh.googleusercontent.com/Gg8C7Pam7AWPzD2JJMMqo5VSixKzEFcXD78P0_ibyeyjKC3-pLTlOtieuCmpBDo2-w",
                                Size = AdaptiveImageSize.Small,
                                Style = AdaptiveImageStyle.Person
                            }
                        }
                    },
                    new AdaptiveColumn()
                    {
                        Width = "2",
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Text = $"[{myData.Message}](https://opsg.in/a/i/lstrk/${myData.UnifiedAlertId})",
                                Weight = AdaptiveTextWeight.Bolder,
                                Spacing = AdaptiveSpacing.None
                            }
                        }
                    }
                }
            },
            new AdaptiveTextBlock()
            {
                Text = $"{myData.Description}",
                Wrap = true
            },
            new AdaptiveFactSet()
            {
                Facts = new List<AdaptiveFact>()
                {
                    new AdaptiveFact() { Title = "Priority: ", Value = $"{myData.Priority}" },
                    new AdaptiveFact() { Title = "Status: ", Value = $"{myData.Status}" },
                    new AdaptiveFact() { Title = "Source: ", Value = $"{myData.Source}" }
                }
            }
        },
                Actions = new List<AdaptiveAction>()
            };

            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
            return attachment;
        }

        public Attachment AddAckButton(Attachment card, AlertData myData)
        {
            var cardContent = card.Content as AdaptiveCard;
            cardContent.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = "Acknowledge",
                DataJson = $"{{\"type\": \"Ack\", \"alertId\": \"{myData.UnifiedAlertId}\"}}"

            });
            card.Content = cardContent;
            return card;
        }

        public Attachment AddUnAckButton(Attachment card, AlertData myData)
        {
            var cardContent = card.Content as AdaptiveCard;
            cardContent.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = "Unacknowledge",
                DataJson = $"{{\"type\": \"Unack\", \"alertId\": \"{myData.UnifiedAlertId}\"}}"

            });
            card.Content = cardContent;
            return card;
        }

        public Attachment AddCloseButton(Attachment card, AlertData myData)
        {
            var cardContent = card.Content as AdaptiveCard;
            cardContent.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = "Close",
                DataJson = $"{{\"type\": \"Close`\", \"alertId\": \"{myData.UnifiedAlertId}\"}}"

            });
            card.Content = cardContent;
            return card;
        }
        public Attachment AddNoteButton(Attachment card, AlertData myData)
        {
            var cardContent = card.Content as AdaptiveCard;
            cardContent.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = "Add Note",
                DataJson = $"{{\"type\": \"AddNote`\", \"alertId\": \"{myData.UnifiedAlertId}\"}}"

            });
            card.Content = cardContent;
            return card;
        }

        public Attachment AddIncidentButton(Attachment card, AlertData myData)
        {
            var cardContent = card.Content as AdaptiveCard;
            cardContent.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = "Create Incident",
                DataJson = $"{{\"type\": \"Incident`\", \"alertId\": \"{myData.UnifiedAlertId}\"}}"

            });
            card.Content = cardContent;
            return card;
        }
    }
}
