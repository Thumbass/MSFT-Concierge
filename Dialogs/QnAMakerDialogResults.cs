using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Microsoft.Bot.Connector;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class QnAMakerDialogResults
    {
        const string AccessKey = "Ocp-Apim-Subscription-Key";  
        public async Task<QnAMakerResult> AskChatbotFaqAsync(string question, string id)
        {
            const string BaseUrl = "https://westus.api.cognitive.microsoft.com/qnamaker/v2.0";           
            string knowledgeBaseID = id;
            string url = $"{BaseUrl}/knowledgebases/{knowledgeBaseID}/generateAnswer"; 

            var client = new HttpClient();
            string accessKey = ConfigurationManager.AppSettings["QnASubscriptionKey"]; 
            client.DefaultRequestHeaders.Add(AccessKey, accessKey);

            var content = new StringContent($"{{\"question\": \"{question}\"}}", Encoding.UTF8, "application/json");
            
            HttpResponseMessage response = await client.PostAsync(url, content);
            string jsonResult = await response.Content.ReadAsStringAsync();

            QnAMakerResults qnaResponse = JsonConvert.DeserializeObject<QnAMakerResults>(jsonResult);
            return qnaResponse.Answers.FirstOrDefault();         }
    }
}
