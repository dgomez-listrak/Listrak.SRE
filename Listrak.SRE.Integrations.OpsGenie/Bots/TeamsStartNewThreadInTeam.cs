// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
////https://teams.microsoft.com/l/channel/19:24d638f4c79941298611e751c92277c4@thread.tacv2/On-Call%2520Alert%2520Log?groupId=ad11c04b-a12d-48f3-b300-a640fac9726c&tenantId=a0479c74-9820-417d-8763-a7b609250f00
namespace Listrak.SRE.Integrations.OpsGenie.Implementations
{
    public class TeamsStartNewThreadInTeam : ActivityHandler
    {
        private readonly string _appId;
        private readonly IBotFrameworkHttpAdapter Adapter;

        public TeamsStartNewThreadInTeam(IConfiguration configuration, IBotFrameworkHttpAdapter adapter)
        {
            Adapter = adapter;
            _appId = configuration["MicrosoftAppId"];
            Adapter = adapter;

        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var teamsChannelId = "19:24d638f4c79941298611e751c92277c4@thread.tacv2";//turnContext.Activity.TeamsGetChannelId();
            var activity = MessageFactory.Text("This will start a new thread in a channel.");

            var details = await TeamsInfo.SendMessageToTeamsChannelAsync(turnContext, activity, teamsChannelId, _appId, cancellationToken);
            await ((CloudAdapter)turnContext.Adapter).ContinueConversationAsync(
                botAppId: _appId,
                reference: details.Item1,
                callback: async (t, ct) =>
                {
                    await t.SendActivityAsync(MessageFactory.Text("This will be the first response to the new thread"), ct);
                },
                cancellationToken: cancellationToken);
        }

        protected async Task SendMsg(string msg, CancellationToken cancellationToken)
        {
            /*var turnContext = new TurnContext(Adapter, new Activity(type: ActivityTypes.Message, text: msg));
            var teamsChannelId = "19:24d638f4c79941298611e751c92277c4@thread.tacv2";//turnContext.Activity.TeamsGetChannelId();
            var activity = MessageFactory.Text("This will start a new thread in a channel.");

            var details = await TeamsInfo.SendMessageToTeamsChannelAsync(turnContext, activity, teamsChannelId, _appId, cancellationToken);
            await ((CloudAdapter)turnContext.Adapter).ContinueConversationAsync(
                botAppId: _appId,
                reference: details.Item1,
                callback: async (t, ct) =>
                {
                    await t.SendActivityAsync(MessageFactory.Text("This will be the first response to the new thread"), ct);
                },
                cancellationToken: cancellationToken);
            */
        }
    }
}
