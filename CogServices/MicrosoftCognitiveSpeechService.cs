using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace LuisBot.CogServices
{
    public class MicrosoftCognitiveSpeechService
    {
        private readonly string subscriptionKey;
        private readonly string speechRecognitionUri;

        public MicrosoftCognitiveSpeechService()
        {
            this.DefaultLocale = "en-US";
            this.subscriptionKey = WebConfigurationManager.AppSettings["MicrosoftSpeechApiKey"];
            this.speechRecognitionUri = Uri.UnescapeDataString(WebConfigurationManager.AppSettings["MicrosoftSpeechRecognitionUri"]);
        }

        public string DefaultLocale { get; set; }

        /// <summary>
        /// Gets text from an audio stream.
        /// </summary>
        /// <param name="audiostream"></param>
        /// <returns>Transcribed text. </returns>
        public async Task<string> GetTextFromAudioAsync(Stream audiostream)
        {
            var requestUri = this.speechRecognitionUri + Guid.NewGuid();

            using (var client = new HttpClient())
            {
                var token = Authentication.Instance.GetAccessToken();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                try
                {
                    using (var binaryContent = new ByteArrayContent(StreamToBytes(audiostream)))
                    {
                        binaryContent.Headers.TryAddWithoutValidation("content-type", "audio/wav; codec=\"audio/pcm\"; samplerate=16000");
                        var response = await client.PostAsync(requestUri, binaryContent);
                        var responseString = await response.Content.ReadAsStringAsync();
                        dynamic data = JsonConvert.DeserializeObject(responseString);

                        if (data != null)
                        {
                            return data.header.name;
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var properties2 = new Dictionary<string, string>
                         {{"ReasonFailed", "failed"},
                        {"token", token },
                        {"FailedSpeech", requestUri }
                        };
                    TelemetryClient _telemetryClient = new TelemetryClient();
                    _telemetryClient.TrackEvent("Failed-JSON-Data", properties2);
                    _telemetryClient.TrackException(ex, properties2);
                    //throw new Exception(ex);
                    return string.Empty;
                }
            }
        }
        
        /// <summary>
        /// Converts Stream into byte[].
        /// </summary>
        /// <param name="input">Input stream</param>
        /// <returns>Output byte[]</returns>
        private static byte[] StreamToBytes(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }

    public sealed class Authentication
    {
        // The token has an expiry time of 10 minutes https://www.microsoft.com/cognitive-services/en-us/Speech-api/documentation/API-Reference-REST/BingVoiceRecognition
        private const int TokenExpiryInSeconds = 600;
        private static readonly object LockObject;
        private static readonly string ApiKey;
        private string token;
        private Timer timer;

        static Authentication()
        {
            LockObject = new object();
            ApiKey = WebConfigurationManager.AppSettings["MicrosoftSpeechApiKey"];
        }

        private Authentication()
        {

        }

        public static Authentication Instance { get; } = new Authentication();

        /// <summary>
        /// Gets the current access token.
        /// </summary>
        /// <returns>Current access token</returns>
        public string GetAccessToken()
        {
            // Token will be null first time the function is called.
            if (this.token == null)
            {
                lock (LockObject)
                {
                    // This condition will be true only once in the lifetime of the application
                    if (this.token == null)
                    {
                        this.RefreshToken();
                    }
                }
            }
            return this.token;
        }

        /// <summary>
        /// Issues a new AccessToken from the Speech Api
        /// </summary>
        /// This method couldn't be async because we are calling it inside of a lock.
        /// <returns>AccessToken</returns>
        private static string GetNewToken()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ApiKey);
                var response = client.PostAsync("https://api.cognitive.microsoft.com/sts/v1.0/issueToken", null).Result;
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                   
                    TelemetryClient _telemetryClient = new TelemetryClient();
                    var properties3 = new Dictionary<string, string>
                         {{"StatusCode", response.StatusCode.ToString()},
                        {"ReasonPhrase", response.ReasonPhrase },
                        {"RequestMessage", response.RequestMessage.ToString() }
                        };
                    _telemetryClient.TrackEvent("Failed-Token", properties3);
                    throw new HttpException((int)response.StatusCode, $"({response.StatusCode}) {response.ReasonPhrase}");
                }
                else
                {
                    TelemetryClient _telemetryClient = new TelemetryClient();
                    var properties3 = new Dictionary<string, string>
                         {{"StatusCode", response.StatusCode.ToString()},
                        {"ReasonPhrase", response.ReasonPhrase },
                        {"RequestMessage", response.RequestMessage.ToString() }
                        };
                    _telemetryClient.TrackEvent("Succeeded-Token", properties3);
                }
                return response.Content.ReadAsStringAsync().Result;
            }
        }

        /// <summary>
        /// Refreshes the current token before it expires. This method will refresh the current access token.
        /// It will also schedule itself to run again before the newly acquired token's expiry by one minute.
        /// </summary>
        private void RefreshToken()
        {
            this.token = GetNewToken();
            this.timer?.Dispose();
            this.timer = new Timer(
                x => this.RefreshToken(),
                null,
                TimeSpan.FromSeconds(TokenExpiryInSeconds).Subtract(TimeSpan.FromMinutes(1)), // Specifies the delay before RefreshToken is invoked.
                TimeSpan.FromMilliseconds(-1)); // Indicates that this function will only run once
        }
    }
}