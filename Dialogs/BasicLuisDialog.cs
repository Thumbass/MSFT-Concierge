using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using LuisBot.Dialogs;
using Newtonsoft.Json;
using System.Net;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using System.Runtime.CompilerServices;
using AdaptiveCards;
using Microsoft.Bot.Builder.FormFlow;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        public bool IsStayInQnA { get; set; } = true;
        string userName = string.Empty;
        int caseSwitch = 1;
        int count = 0;
        bool GetName = false;
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"])))
        {
            
        }
        
        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            Activity replyToConversation = ((Activity)context.Activity).CreateReply();

            replyToConversation.Attachments = new List<Attachment>();

            AdaptiveCards.AdaptiveCard card = new AdaptiveCard();

            // Specify speech for the card.
            card.Speak = "<s>Your  meeting about \"Adaptive Card design session\"<break strength='weak'/> is starting at 12:30pm</s><s>Do you want to snooze <break strength='weak'/> or do you want to send a late notification to the attendees?</s>";

            // Add text to the card.
            card.Body.Add(new TextBlock()
            {
                Text = "MSFT Concierge",
                Size = TextSize.Large,
                Color = TextColor.Attention,
                Weight = TextWeight.Bolder
            });
            //uncomment this to test greeting
            context.UserData.SetValue<string>("Name", userName);
            context.UserData.TryGetValue<string>("Name", out userName);
            if (userName != null && userName != "")
            {
                card.Body.Add(new TextBlock()
                {
                    Size = TextSize.Medium,
                    Wrap = true,
                    Text = string.Format("Welcome Back {0}, What can I help you with?", userName)
                });
            }
            else
            {   // Add text to the card.
                card.Body.Add(new TextBlock()
                {
                    Size = TextSize.Medium,
                    Wrap = true,
                    Text = "Hi, I'm MSFT Concierge.  I'm here to make life a little eaiser for my Microsoft colleagues and point you to the resource you're looking for.  But first, what is Microsoft Alias?"
                });
            }
            // Create the attachment.
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
            replyToConversation.Attachments.Add(attachment);

            await context.PostAsync(replyToConversation);
            if (userName != null && userName != "")
            {
                context.Wait(MessageReceived);
            }
            else
            {
                context.Wait(GreetingMessageReceived);
            }
        }

        [LuisIntent("ITService")]
        public async Task ITService(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var awaitedResults = await activity;
            var reply = ((Activity)context.Activity).CreateReply("");
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Markdown;
            reply.AttachmentLayout = AttachmentLayoutTypes.List;
            reply.Attachments = new List<Attachment>();

            Dictionary<string, string> cardContentList = new Dictionary<string, string>();
            cardContentList.Add("Select one:", "http://365login.xyz/wp-content/uploads/2016/12/microsoft-support.jpg");
            foreach (KeyValuePair<string, string> cardContent in cardContentList)
            {
                List<CardImage> cardImages = new List<CardImage>();
                cardImages.Add(new CardImage(url: cardContent.Value));

                List<CardAction> cardButtons = new List<CardAction>();

                CardAction plButton = new CardAction()
                {
                    Value = $"HelpDesk?",
                    Title = "HelpDesk"
                };
                CardAction plButton2 = new CardAction()
                {
                    Value = $"I need help with my software.",
                    Title = "Software Resources"
                };
                CardAction plButton3 = new CardAction()
                {
                    Value = $"I need help with my hardware.",
                    Title = "Hardware Resources"
                };
                cardButtons.Add(plButton);
                cardButtons.Add(plButton2);
                cardButtons.Add(plButton3);

                ThumbnailCard plCard = new ThumbnailCard()
                {
                    Title = $"You need IT support.",
                    //Subtitle = $"{cardContent.Key} Wikipedia Page",
                    Images = cardImages,
                    Buttons = cardButtons
                };

                Attachment plAttachment = plCard.ToAttachment();
                reply.Attachments.Add(plAttachment);
            }
            
            await context.PostAsync(reply);
            context.Wait(this.MessageReceivedITSUpport);
        }

        [LuisIntent("Contribute")]
        public async Task Contribute(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)

        {
            
            try
            {
                await context.PostAsync("That's awesome. Please fill out as much data as you can.");
                var addContent = new FormDialog<ContributeFFlowDialog>(new ContributeFFlowDialog(), ContributeFFlowDialog.BuildForm, FormOptions.PromptInStart);
                context.Call(addContent, ContributeFormComplete);
            }
            catch (Exception ex)
            {
                await context.PostAsync("Something really bad happened. You can try again later meanwhile I'll check what went wrong.");
                context.Wait(MessageReceived);
            }

        }
        [LuisIntent("None")]
        public async Task None(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)

        {
            var awaitedResults = await activity;
            await context.Forward(new BasicQnAMakerDialog("bfd0e1ed083141a1b3343dc3a2bb0015", "c656c827-ba0b-460d-b3b5-e6b5af817a92"), ConvEndAsync,
                awaitedResults, CancellationToken.None);
        }

        private async Task MessageReceivedITSUpport(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var awaitedResults = await result;
            if (awaitedResults.Text == "HelpDesk?" || awaitedResults.Text == "I need help with my software." || awaitedResults.Text == "I need help with my hardware.")
            {
                await context.Forward(new BasicQnAMakerDialog("bfd0e1ed083141a1b3343dc3a2bb0015", "12d55edb-80e5-456c-8645-319659b97025"), ConvEndAsync,
                awaitedResults, CancellationToken.None);
            }
            else
            {
                await context.Forward(new BasicLuisDialog(), ConvEndAsync,
                awaitedResults, CancellationToken.None);
            }

        }

        private async Task GreetingMessageReceived(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            
            if (count != 0)
            {

                if (message.Text == "yes" || message.Text == "YES" || message.Text == "Yes")
                {
                    caseSwitch = 2;
                }
                else
                {
                    caseSwitch = 3; 
                }
            }
            else { count++; }
            switch (caseSwitch)
            {
                case 1:
                    userName = message.Text;
                    await context.PostAsync(string.Format("Are you sure you are {0}?", userName));
                    context.Wait(GreetingMessageReceived);
                    break;
                case 2:
                    context.UserData.SetValue<bool>("GetName", true);
                    context.UserData.SetValue<string>("Name", userName);
                    await context.PostAsync(string.Format("Hi, {0}.  How can I help you?", userName));
                    context.Wait(MessageReceived);
                    break;
                default:
                    await context.PostAsync("What's your Microsoft alias?");
                    context.Wait(GreetingConfimedMessageReceived);
                    break;
            }             
           // context.Wait(MessageReceived);

        }

        private async Task GreetingConfimedMessageReceived(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            userName = message.Text;
            context.UserData.SetValue<bool>("GetName", true);
            context.UserData.SetValue<string>("Name", userName);
            await context.PostAsync(string.Format("Hi, {0}.  How can I help you?", userName));
            context.Wait(MessageReceived);

        }       
        
        public async Task ConvEndAsync(IDialogContext context, IAwaitable<object> argument )
        {
            try
            {
                var message = await argument;
            }
            catch (Exception ex)
            {

            }
            context.Wait(MessageReceived);
        }
        private async Task ContributeFormComplete(IDialogContext context, IAwaitable<ContributeFFlowDialog> result)
        {
            try
            {
                var message = await result;
                var properties = new Dictionary<string, string>
                    {
                        {"Alias", userName },
                        {"ResourceName", message.ContentName },
                        {"ResourceDescription", message.Description },
                        {"ResourceURL", message.Url },
                        {"ResourcePicUrl", message.PicUrl },
                        {"ResourceContentType", message.ContentType.ToString() },
                        {"ResourceCategory", message.Category.ToString() },
                        {"ResourceOtherCatagory", message.CategoryOther },

                        {"MessageType", "Contribution" },
                        {"Vote", "Yes" }
                        // add properties relevant to your bot 
                    };
                TelemetryClient telemetry = new TelemetryClient();
                telemetry.TrackEvent("User-Contribution", properties);
                await context.PostAsync("Thanks for the your contribution!");
                await context.PostAsync("How else may I assist you?");
                

            }
            catch (FormCanceledException)
            {
                await context.PostAsync("Don't want to send Contribute now? That's ok. You can drop a comment below.");
            }
            catch (Exception)
            {
                await context.PostAsync("Something really bad happened. You can try again later meanwhile I'll check what went wrong.");
            }
            finally
            {
                context.Wait(MessageReceived);
            }
        }

    }
}
