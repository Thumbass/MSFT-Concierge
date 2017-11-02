using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.ApplicationInsights;
using Microsoft.Bot.Builder.ConnectorEx;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class FeedbackDialog : IDialog<IMessageActivity>
    {
        private string qnaURL;
        private string userQuestion;

        public FeedbackDialog(string url, string question)
        {
            // keep track of data associated with feedback
            qnaURL = url;
            userQuestion = question;
        }

        public async Task StartAsync(IDialogContext context)
        {
            var feedback = ((Activity)context.Activity).CreateReply("Did you find what you need?");
            feedback.Text = "Did you find what you need?"; // This cannot be null, or skype won't work.
            feedback.TextFormat = TextFormatTypes.Plain;


            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" }, //(y)
                    new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" } //(n)
                }
            };
            //feedback.Attachments = new List<Attachment>
            //{
            //    new KeyboardCard(text: "Options:", buttons: feedback.SuggestedActions.Actions).ToAttachment()
            //};
            await context.PostAsync(feedback);
            context.Wait(this.MessageReceivedAsync);
        }

        //Task IDialog<IMessageActivity>.StartAsync(IDialogContext context)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var userFeedback = await result;

            if (userFeedback.Text.Contains("yes-positive-feedback") || userFeedback.Text.Contains("no-negative-feedback"))
            {
                // create telemetry client to post to Application Insights 
                TelemetryClient telemetry = new TelemetryClient();

                if (userFeedback.Text.Contains("yes-positive-feedback"))
                {
                    // post feedback to App Insights
                    var properties = new Dictionary<string, string>
                    {
                        {"Question", userQuestion },
                        {"URL", qnaURL },
                        {"Vote", "Yes" }
                        // add properties relevant to your bot 
                    };

                    telemetry.TrackEvent("Yes-Vote", properties);
                    await context.PostAsync("Thanks for your feedback!");
                }
                else
                {
                    var properties = new Dictionary<string, string>
                    {
                        {"Question", userQuestion },
                        {"URL", qnaURL },
                        {"Vote", "No" }

                        // add properties relevant to your bot 
                    };

                    telemetry.TrackEvent("No-Vote", properties);
                    await context.PostAsync(@"I'm sorry to hear that."); 
                    await context.PostAsync(@"We have captured your question and our response which you were not satisfied with. We will use this data to correct our responses.");
                    await context.PostAsync(@"If you would like to expand on the issue you can click the call button and a leave a 10 second message or you can type ""reference"" to make suggested content changes.");

                }
                
                context.Done<IMessageActivity>(null);
            }
            else

            {
                // no feedback, return to QnA dialog
                //userFeedback.Summary = "LUIS";
                //context.Done<IMessageActivity>(userFeedback);
               await StartAsync(context);
            }
        }
    }
}