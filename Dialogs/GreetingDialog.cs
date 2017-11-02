using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class GreetingDialog : IDialog
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Hi, I'm MSFT Concierge.  I'm here to make life a little eaiser for my Microsoft colleagues and point you to the resource you're looking for.");
            context.Wait(GreetingMessageReceived);
        }

        private async Task GreetingMessageReceived(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var userName = string.Empty;
            var isNameKnown = false;

            context.UserData.TryGetValue<string>("Name", out userName);
            context.UserData.TryGetValue<bool>("isNameKnown", out isNameKnown);
            if (isNameKnown)
            {
                userName = message.Text;
                context.UserData.SetValue<string>("Name", userName);
                context.UserData.SetValue<bool>("isNameKnown", false);
            }

            if (string.IsNullOrEmpty(userName))
            {
                await context.PostAsync("What's your name?");
                context.UserData.SetValue<bool>("isNameKnown", true);                
            }
            else
            {
                await context.PostAsync(string.Format("Hi, {0}.  How can I help you?", userName));
            }
            context.Wait(GreetingMessageReceived);
            
        }
    }
}