using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using System.Configuration;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class BasicQnAMakerDialog : QnAMakerDialog
    {
        //Parameters to QnAMakerService are:
        //Required: subscriptionKey, knowledgebaseId, 
        //Optional: defaultMessage, scoreThreshold[Range 0.0 – 1.0]

        public BasicQnAMakerDialog(string key, string id) : base(new QnAMakerService(new QnAMakerAttribute(key,
            id, "No good match in FAQ.", 0.1)))
        { }
        protected override async Task RespondFromQnAMakerResultAsync(IDialogContext context, IMessageActivity message, QnAMakerResults result)
        {
            // answer is a string
            var answer2 = message;
            var answer = result.Answers.First().Answer;
            Attachment data = new Attachment(answer);
            answer2.Attachments.Add(data);
            //Activity reply = ((Activity)context.Activity).CreateReply();
            context.Done(answer2);
        //    string[] qnaAnswerData = answer.Split(';');
        //    int dataSize = qnaAnswerData.Length;
        //    string title = qnaAnswerData[0];
        //    string description = qnaAnswerData[1];
        //    string url = qnaAnswerData[2];
        //    string imageURL = qnaAnswerData[3];

        //    HeroCard card = new HeroCard
        //    {
        //        Title = title,
        //        Subtitle = description,
        //    };
        //    if (title == "IT HelpDesk")
        //    {
        //        card.Buttons = new List<CardAction>
        //            {
        //                new CardAction(ActionTypes.OpenUrl, "Call", value: url)
        //            };
        //    }
        //    else
        //    {
        //        card.Buttons = new List<CardAction>
        //            {
        //                new CardAction(ActionTypes.OpenUrl, "Learn More", value: url)
        //            };
        //    }

        //    card.Images = new List<CardImage>
        //        {
        //            new CardImage( url = imageURL)
        //        };
        //    reply.Attachments.Add(card.ToAttachment());
        //    await context.PostAsync(reply);
        //}
        //protected override async Task DefaultWaitNextMessageAsync(IDialogContext context, IMessageActivity message, QnAMakerResults result)
        //{
        //    // get the URL
        //    var answer = result.Answers.First().Answer;
        //    string[] qnaAnswerData = answer.Split(';');
        //    string qnaURL = qnaAnswerData[2];

        //    // pass user's question
        //    var userQuestion = (context.Activity as Activity).Text;
        //    context.Call(new FeedbackDialog(qnaURL, userQuestion), ResumeAfterFeedback);
        //}

        //private async Task ResumeAfterFeedback(IDialogContext context, IAwaitable<IMessageActivity> result)
        //{
        //    if (await result != null)
        //    {
        //        await MessageReceivedAsync(context, result);
        //    }
        //    else
        //    {
        //        context.Done<IMessageActivity>(await result);
        //    }
        }
    }
}
