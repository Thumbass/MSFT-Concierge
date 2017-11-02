using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.FormFlow;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class ContributeFFlowDialog
    {
       // [Prompt("Do you wish to Add, Update, or Remove content")]
        public EntryType ContentType { get; set; }
        [Prompt("What's the name of the resource?")]
        public string ContentName { get; set; }
        [Prompt("Give a brief Description of the resource you wish added or edited (Leave Blank if you are request a resource be removed)")]
        public string Description { get; set; }
        [Prompt("What is the URL of the resource (i.e. http:https://shop.ecompanystore.com/MSEPPStore/login.aspx?src=MIC_Login&0")]
        public string Url { get; set; }
        
       [Prompt("What is the URL of the Pic you would like to use for the resource")]
        [Optional]
        public string PicUrl { get; set; }
        //[Prompt("What's category does this resource fall in to?")]
        public CategoryList Category { get; set; }
        
       [Prompt(@"If category was ""Other"" please type the category you wish to use, otherwise type none. ")]
        [Optional]
        public string CategoryOther { get; set; }


        public enum CategoryList
        {
            Admin,ITService,Legal,HR,Tools,Training,Benefits,InternalResources,other
        }
        public enum EntryType
        {
            Admin,Add,Update,Remove
        }
        
        public static IForm<ContributeFFlowDialog> BuildForm()
        {
            return new FormBuilder<ContributeFFlowDialog>()
               .Field("ContentType")
               .Field("ContentName")
               .AddRemainingFields()
               .Build();
        }
    }
}