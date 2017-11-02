using Microsoft.Bot.Builder.Calling;
using Microsoft.Bot.Builder.Calling.Events;
using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LuisBot.CogServices;
using Microsoft.ApplicationInsights;

namespace LuisBot.Voice
{
    // The bot
    public class CallingBot : IDisposable, ICallingBot
    {
        private readonly MicrosoftCognitiveSpeechService speechService = new MicrosoftCognitiveSpeechService();

        public CallingBot(ICallingBotService callingBotService)
        {
            this.CallingBotService = callingBotService;
            this.CallingBotService.OnIncomingCallReceived += this.OnIncomingCallReceived;
            this.CallingBotService.OnRecordCompleted += this.OnRecordCompleted;
            this.CallingBotService.OnHangupCompleted += OnHangupCompleted;
        }

        public ICallingBotService CallingBotService { get; }

        public void Dispose()
        {
            if (this.CallingBotService != null)
            {
                this.CallingBotService.OnIncomingCallReceived -= this.OnIncomingCallReceived;
                this.CallingBotService.OnRecordCompleted -= this.OnRecordCompleted;
                this.CallingBotService.OnHangupCompleted -= OnHangupCompleted;
            }
        }

        private static Task OnHangupCompleted(HangupOutcomeEvent hangupOutcomeEvent)
        {
            hangupOutcomeEvent.ResultingWorkflow = null;
            return Task.FromResult(true);
        }

        private static PlayPrompt GetPromptForText(string text)
        {
            var prompt = new Prompt { Value = text, Voice = VoiceGender.Male, Culture = Culture.EnUs};
            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
        }

        private Task OnIncomingCallReceived(IncomingCallEvent incomingCallEvent)
        {
            var record = new Record
            {
                OperationId = Guid.NewGuid().ToString(),
                InitialSilenceTimeoutInSeconds = 3,
                MaxSilenceTimeoutInSeconds = 2,
                PlayPrompt = new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { new Prompt { Value = "Please leave a 10 second or less message about the issue you are having.", Voice = VoiceGender.Male } } },
                RecordingFormat = RecordingFormat.Wav
            };
            try
            {
                incomingCallEvent.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    new Answer { OperationId = Guid.NewGuid().ToString() },
                    record
                };
                
            }
            catch (Exception ex)
            {
                TelemetryClient telemetry = new TelemetryClient();
                var properties = new Dictionary<string, string>
                    {
                        {"OperationID", record.OperationId },
                        {"Error", ex.ToString() },
                        {"speech", "fail" },
                        // add properties relevant to your bot 
                    };
                
                telemetry.TrackException(ex, properties);
            }
            
            return Task.FromResult(true);
        }

        private async Task OnRecordCompleted(RecordOutcomeEvent recordOutcomeEvent)
        {
            List<ActionBase> actions = new List<ActionBase>();

            var spokenText = string.Empty;
            if (recordOutcomeEvent.RecordOutcome.Outcome == Outcome.Success)
            {
                var record = await recordOutcomeEvent.RecordedContent;
                spokenText = await this.speechService.GetTextFromAudioAsync(record);
                TelemetryClient telemetry = new TelemetryClient();
                var properties = new Dictionary<string, string>
                    {
                        {"SpokenText", spokenText },
                        {"MessageType", "Voice-Feedback" },
                        // add properties relevant to your bot 
                    };
                telemetry.TrackEvent("RecordedMessage", properties);
                actions.Add(new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { new Prompt { Value = "Thanks for leaving the message." }, new Prompt { Value = "You said... " + spokenText }, new Prompt { Value = "We will use your feedback to improve your experience in the future." } } });
            }
            else
            {
                actions.Add(new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { new Prompt { Value = "Sorry, there was an issue. " } } });
            }

            actions.Add(new Hangup { OperationId = Guid.NewGuid().ToString() }); // hang up the call

            recordOutcomeEvent.ResultingWorkflow.Actions = actions;
            recordOutcomeEvent.ResultingWorkflow.Links = null;
        }
    }
}