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
        bool isATutorialForUser = false;
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"])))
        {
            
        }
        
        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            if (isATutorialForUser)
            {
                await context.PostAsync("You're still in the tutorial.  Please type \"contribute\".");
            }
            else
            {


                Activity replyToConversation = ((Activity)context.Activity).CreateReply();
                replyToConversation.Attachments = new List<Attachment>();
                HeroCard hcard = new HeroCard
                {
                    Title = "MSFT Concierge",
                                        
                };

                AdaptiveCards.AdaptiveCard card = new AdaptiveCard();
                // Specify speech for the card.
                //card.Speak = "<s>Your  meeting about \"Adaptive Card design session\"<break strength='weak'/> is starting at 12:30pm</s><s>Do you want to snooze <break strength='weak'/> or do you want to send a late notification to the attendees?</s>";
                // Add text to the card.
                card.Body.Add(new TextBlock()
                {
                    Text = "MSFT Concierge",
                    Size = TextSize.Large,
                    Color = TextColor.Attention,
                    Weight = TextWeight.Bolder
                });
                //uncomment this to test greeting
                //context.UserData.SetValue<string>("Name", userName);
                string text;
                context.UserData.TryGetValue<string>("Name", out userName);
                if (userName != null && userName != "")
                {
                    text = string.Format("Welcome Back {0}, What can I help you with?", userName);
                   
                }
                else
                {   // Add text to the card.
                    text = "Hi, I'm MSFT Concierge.  I'm here to make life a little eaiser for my Microsoft colleagues and point you to the resource you're looking for.  But first, what is your Microsoft Alias?";
                    
                }
                card.Body.Add(new TextBlock()
                {
                    Size = TextSize.Medium,
                    Wrap = true,
                    Text = text
                });
                hcard.Subtitle = text;
                // Create the attachment.
                Attachment attachment = new Attachment()
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card
                };
                //replyToConversation.Attachments.Add(attachment);
                replyToConversation.Attachments.Add(hcard.ToAttachment());

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
        }

        [LuisIntent("ITService")]
        public async Task ITService(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            if (isATutorialForUser)
            {
                await context.PostAsync("You're still in the tutorial.  Please type \"contribute\".");
            }
            else
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
                    CardAction plButton4 = new CardAction()
                    {
                        Value = $"I need to reset my password",
                        Title = "Password Reset"
                    };
                    CardAction plButton5 = new CardAction()
                    {
                        Value = $"I need help getting connected",
                        Title = "Get Connected"
                    };

                    cardButtons.Add(plButton);
                    cardButtons.Add(plButton2);
                    cardButtons.Add(plButton3);
                    cardButtons.Add(plButton4);
                    cardButtons.Add(plButton5);

                    HeroCard plCard = new HeroCard()
                    {
                        Title = $"You need IT support.",
                        Subtitle = $"Please click a button",
                        Images = cardImages,
                        Buttons = cardButtons
                    };

                    Attachment plAttachment = plCard.ToAttachment();
                    reply.Attachments.Add(plAttachment);
                }

                await context.PostAsync(reply);
                context.Wait(this.MessageReceivedITSUpport);
            }
        }

        [LuisIntent("Contribute")]
        public async Task Contribute(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)

        {
            await ContributeMessageReceived(context, activity);
            

        }

        [LuisIntent("Tutorial")]
        public async Task Tutorial(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)

        {

            
                await context.PostAsync("Great! Start by asking me a simple question.  Ask me: \"Where can I get more information on training?\".");
                //isATutorialForUser = true;
                context.Wait(TutorialMessageReceived);
            

        }

        [LuisIntent("Tools")]
        public async Task Tools(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)

        {
            //var awaitedResults = await activity;
            //await context.Forward(new BasicQnAMakerDialog("bfd0e1ed083141a1b3343dc3a2bb0015", "dd7117b5-0b27-459c-9565-cce3395f0769"), ConvEndAsync,
            //    awaitedResults, CancellationToken.None);
            if (isATutorialForUser)
            {
                await context.PostAsync("You're still in the tutorial.  Please type \"contribute\".");
            }
            else
            {
                QnAMakerDialogResults cogSvc = new QnAMakerDialogResults();
                var awaitedResults = await activity;
                QnAMakerResult qnaAnswer = await cogSvc.AskChatbotFaqAsync(result.Query, "dd7117b5-0b27-459c-9565-cce3395f0769");
                await HeroCardMessageReceived(context, activity, qnaAnswer);
            }
        }

        [LuisIntent("Training")]
        public async Task Training(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)

        {
            //context.Call(new BasicQnAMakerDialog("bfd0e1ed083141a1b3343dc3a2bb0015", "269f8e3b-09bb-4367-bca3-188c55ec5f57"), this.HeroCardMessageReceived);
            //context.Wait(ConvEndAsync);
            if (isATutorialForUser)
            {
                await context.PostAsync("You're still in the tutorial.  Please type \"contribute\".");
            }
            else
            {
                QnAMakerDialogResults cogSvc = new QnAMakerDialogResults();
                var awaitedResults = await activity;
                QnAMakerResult qnaAnswer = await cogSvc.AskChatbotFaqAsync(result.Query, "269f8e3b-09bb-4367-bca3-188c55ec5f57");
                await HeroCardMessageReceived(context, activity, qnaAnswer);
            }
            
        }

        [LuisIntent("Legal")]
        public async Task Legal(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)

        {
            //var awaitedResults = await activity;
            //await context.Forward(new BasicQnAMakerDialog("bfd0e1ed083141a1b3343dc3a2bb0015", "a78b6ff9-c4fb-4f11-9d47-08c97a951a63"), ConvEndAsync,
            //    awaitedResults, CancellationToken.None);
            if (isATutorialForUser)
            {
                await context.PostAsync("You're still in the tutorial.  Please type \"contribute\".");
            }
            else
            {
                QnAMakerDialogResults cogSvc = new QnAMakerDialogResults();
                var awaitedResults = await activity;
                QnAMakerResult qnaAnswer = await cogSvc.AskChatbotFaqAsync(result.Query, "a78b6ff9-c4fb-4f11-9d47-08c97a951a63");
                await HeroCardMessageReceived(context, activity, qnaAnswer);
            }
        }

        [LuisIntent("HR")]
        public async Task HR(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)

        {
            //var awaitedResults = await activity;
            //await context.Forward(new BasicQnAMakerDialog("bfd0e1ed083141a1b3343dc3a2bb0015", "1c9b56bf-d9c0-4135-83c2-8d97d26c305a"), ConvEndAsync,
            //    awaitedResults, CancellationToken.None);
            if (isATutorialForUser)
            {
                await context.PostAsync("You're still in the tutorial.  Please type \"contribute\".");
            }
            else
            {
                QnAMakerDialogResults cogSvc = new QnAMakerDialogResults();
                var awaitedResults = await activity;
                QnAMakerResult qnaAnswer = await cogSvc.AskChatbotFaqAsync(result.Query, "1c9b56bf-d9c0-4135-83c2-8d97d26c305a");
                await HeroCardMessageReceived(context, activity, qnaAnswer);
            }
        }

        [LuisIntent("None")]
        public async Task None(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)

        {
            //var awaitedResults = await activity;
            //await context.Forward(new BasicQnAMakerDialog("bfd0e1ed083141a1b3343dc3a2bb0015", "c656c827-ba0b-460d-b3b5-e6b5af817a92"), ConvEndAsync,
            //    awaitedResults, CancellationToken.None);
            if (isATutorialForUser)
            {
                await context.PostAsync("You're still in the tutorial.  Please type \"contribute\".");
            }
            else
            {
                QnAMakerDialogResults cogSvc = new QnAMakerDialogResults();
                var awaitedResults = await activity;
                QnAMakerResult qnaAnswer = await cogSvc.AskChatbotFaqAsync(result.Query, "c656c827-ba0b-460d-b3b5-e6b5af817a92");
                await HeroCardMessageReceived(context, activity, qnaAnswer);
            }
        }

        private async Task HeroCardMessageReceived(IDialogContext context, IAwaitable<object> result, QnAMakerResult qnaResult, bool tutorialExample = false)
        {
            
            // answer is a string
            var message = await result;
            Activity reply = ((Activity)context.Activity).CreateReply();
            string answer = qnaResult.Answer.ToString();
            string[] qnaAnswerData = answer.Split(';');
            int dataSize = qnaAnswerData.Length;
            string title = qnaAnswerData[0];
            string description = qnaAnswerData[1];
            string url = qnaAnswerData[2];
            string imageURL = qnaAnswerData[3];
            var userQuestion = (context.Activity as Activity).Text;

            HeroCard card = new HeroCard
            {
                Title = title,
                Subtitle = description,
            };
            if (title == "IT HelpDesk")
            {
                card.Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.OpenUrl, "Call", value: url)
                };
            }
            else
            {
                card.Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.OpenUrl, "Learn More", value: url)
                };
            }

            card.Images = new List<CardImage>
            {
                new CardImage( url = imageURL)
            };
            reply.Attachments.Add(card.ToAttachment());
            await context.PostAsync(reply);
            context.Call(new FeedbackDialog(url, userQuestion, tutorialExample), ResumeAfterFeedback);

        }
                
        private async Task ResumeAfterFeedback(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            //if (await result != null)
            //{
            //    await MessageReceivedAsync(context, result);
            //}
            //else
            //{
                context.Wait(MessageReceived);
            //}
        }
        
        private async Task MessageReceivedITSUpport(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var awaitedResults = await activity;
            if (awaitedResults.Text == "HelpDesk?" || awaitedResults.Text == "I need help with my software." || awaitedResults.Text == "I need help with my hardware."
                || awaitedResults.Text == "I need to reset my password" || awaitedResults.Text == "I need help getting connected")
            {
               // await context.Forward(new BasicQnAMakerDialog("bfd0e1ed083141a1b3343dc3a2bb0015", "12d55edb-80e5-456c-8645-319659b97025"), ConvEndAsync,
               // awaitedResults, CancellationToken.None);

                QnAMakerDialogResults cogSvc = new QnAMakerDialogResults();
                //var awaitedResults = await activity;
                QnAMakerResult qnaAnswer = await cogSvc.AskChatbotFaqAsync(awaitedResults.Text, "12d55edb-80e5-456c-8645-319659b97025");
                await HeroCardMessageReceived(context, activity, qnaAnswer);
            }
            else
            {
                //await context.Forward(new BasicLuisDialog(), ConvEndAsync,
                //awaitedResults, CancellationToken.None);
                context.Wait(MessageReceived);
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
                    await context.PostAsync(string.Format("Hi, {0}.  Great to meet you.  Would you like run through a quick tutorial?", userName));
                    context.Wait(TutorialQuestionMessageReceived);
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
                    await context.PostAsync(string.Format("Hi, {0}.  Great to meet you.  Would you like run through a quick tutorial?", userName));
                    context.Wait(TutorialQuestionMessageReceived);

                }

        private async Task TutorialQuestionMessageReceived(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            context.UserData.TryGetValue<string>("Name", out userName);
            if (message.Text == "yes" || message.Text == "YES" || message.Text == "Yes" || message.Text == "y" || message.Text == "Y")
            {
                await context.PostAsync($"Great! Start by asking me a simple question.  Ask me: 'Where can I get more information on training?'");
                isATutorialForUser = true;
                context.Wait(TutorialMessageReceived);
            }

            else if (message.Text == "no" || message.Text == "NO" || message.Text == "No" || message.Text == "n" || message.Text == "n")
            {
                context.UserData.TryGetValue<string>("Name", out userName);
                await context.PostAsync("No problem.  You can access the tutorial at a time by typing the word \"tutorial\".");
                await context.PostAsync("How may I help you?");
                context.Wait(MessageReceived);
            }
            else
            {
                await context.PostAsync("Please answer \"yes\" or \"no\".");
                context.Wait(TutorialQuestionMessageReceived);
            }
        }

        private async Task TutorialMessageReceived(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var response = await activity;

            //if (response.Text.Contains("Where can I get more information on training?"))
            //{
            //await context.Forward(new BasicQnAMakerDialog("bfd0e1ed083141a1b3343dc3a2bb0015", "269f8e3b-09bb-4367-bca3-188c55ec5f57"), ConvEndAsync,
            //  response, CancellationToken.None);
            isATutorialForUser = true;
            QnAMakerDialogResults cogSvc = new QnAMakerDialogResults();
            var awaitedResults = await activity;
            QnAMakerResult qnaAnswer = await cogSvc.AskChatbotFaqAsync(awaitedResults.Text, "269f8e3b-09bb-4367-bca3-188c55ec5f57");
            await HeroCardMessageReceived(context, activity, qnaAnswer, true);
                //context.Wait(CompletedQnATutorialReceived);                
            //}
        }

        //private async Task CompletedQnATutorialReceived(IDialogContext context, IAwaitable<object> result)
        //{
        //    await context.PostAsync("Great!  Providing feedback in this way will help me to get better and better at servicing you, letting me know what I did right, and what I need to improve on.");
        //    await context.PostAsync("Next, I'm going to show you how you can contribute to our content and provide impact to the community");
        //    await context.PostAsync("Type the word \"contribute\".");
        //    context.Wait(ContributeTutorialMessageReceived);
        //}

        //private async Task ContributeTutorialMessageReceived(IDialogContext context, IAwaitable<object> result)
        //{
        //    try
        //    {
        //        await context.PostAsync("That's awesome. Please fill out as much data as you can.");
        //        var addContent = new FormDialog<ContributeFFlowDialog>(new ContributeFFlowDialog(), ContributeFFlowDialog.BuildForm, FormOptions.PromptInStart);
        //        await context.PostAsync("Thanks for the your contribution!");
        //        await context.PostAsync("These contributions are forwarded up for review, verified, and then added to my knowledgebase within a week. ");
        //        await context.PostAsync("I keep track of the number of contributions {0} makes and in the future I will provide a way to provide that impact back to you managers.", userName);
        //        await context.PostAsync("That concludes my tutorial.  You may type \"tutorial\" at anytime to run through the tutorial again.");
        //    }
        //    catch (Exception ex)
        //    {
        //        await context.PostAsync("Something really bad happened. You can try again later meanwhile I'll check what went wrong.");
               
        //    }
        //}

        private async Task ContributeMessageReceived(IDialogContext context, IAwaitable<object> result)
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
                isATutorialForUser = false;
                context.Wait(MessageReceived);
            }
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
                if (!isATutorialForUser)
                {
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
                else
                {
                    await context.PostAsync("Thanks for the your contribution!");
                    await context.PostAsync("These contributions are forwarded up for review, verified, and then added to my knowledgebase within a week. ");
                    await context.PostAsync(string.Format("I keep track of the number of contributions {0} makes and in the future I will provide a way to provide that impact back to you managers.", userName));
                    await context.PostAsync("That concludes my tutorial.  You may type \"tutorial\" at anytime to run through the tutorial again.");
                    isATutorialForUser = false;
                }

            }
            catch (FormCanceledException)
            {
                await context.PostAsync("Don't want to send Contribute now? That's ok. You can drop a comment below.");
                isATutorialForUser = false;
            }
            catch (Exception)
            {
                await context.PostAsync("Something really bad happened. You can try again later meanwhile I'll check what went wrong.");
                isATutorialForUser = false;
            }
            finally
            {
                context.Wait(MessageReceived);
            }
        }

    }
}
