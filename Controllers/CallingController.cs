﻿using Microsoft.Bot.Builder.Calling;
using Microsoft.Bot.Connector;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using LuisBot.Voice;

namespace LuisBot.Controllers
{
    [BotAuthentication]
    [RoutePrefix("api/calling")]
    public class CallingController : ApiController
    {
        public CallingController() : base()
        {
            CallingConversation.RegisterCallingBot(callingBotService => new CallingBot(callingBotService));
        }

        [Route("callback")]
        public async Task<HttpResponseMessage> ProcessCallingEventAsync()
        {

            return await CallingConversation.SendAsync(this.Request, CallRequestType.CallingEvent);
        }

        [Route("call")]
        public async Task<HttpResponseMessage> ProcessIncomingCallAsync()
        {
            return await CallingConversation.SendAsync(this.Request, CallRequestType.IncomingCall);
        }
    }
}