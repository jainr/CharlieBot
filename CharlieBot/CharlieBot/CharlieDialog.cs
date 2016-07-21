using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Resource;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Dialogs;

namespace CharlieBot
{
    [LuisModel("5b0818bf-1a55-4b04-b1bd-3e49a277f7e5", "2b8c08aa30234d77a0b868503207c24b")]
    [Serializable]
    public class CharlieDialog :LuisDialog<object>
    {
        private string route;
        private string stopName;
        private string direction;

        public const string Entity_Stop_Id = "stopId";

        [LuisIntent("bus")]
        public async Task getBusInfo(IDialogContext context, LuisResult result)
        {
            EntityRecommendation stopId;

            if (result.TryFindEntity(Entity_Stop_Id, out stopId))
            {
                context.UserData.SetValue<string>(Entity_Stop_Id, stopId.Entity);
                await context.PostAsync($"Mode is detected as bus, Stop Name is, {context.UserData.Get<string>(Entity_Stop_Id)}.");
            }
            else
            {
   
                PromptDialog.Text(context, findStopName, "I didn't get the stop name, can you please enter the stop name?", null, 3);
                //stopName = context.ConversationData.ToString();
                //await context.PostAsync("Stop Name is ::" +stopName);


                //context.UserData.SetValue<string>(Entity_Stop_Id, new PromptDialog.PromptString(stopName, "I didn't get the stop name, can you please enter the stop name?", 1));
            }
            //_mode = "bus";
            //await context.PostAsync($"Mode is deteced as bus");
            context.Wait(MessageReceived);
        }

        public async Task findStopName(IDialogContext context, IAwaitable<string> stopName)
        {
            await stopName;
            await context.PostAsync("Ok! Your stopName is :" + stopName);
            //context.Wait(MessageReceived);
        }

        [LuisIntent("train")]
        public async Task gettrainInfo(IDialogContext context, LuisResult result)
        {
            //_mode = "train";
            await context.PostAsync($"Mode is deteced as train");
            context.Wait(MessageReceived);
        }

        [LuisIntent("commuter rail")]
        public async Task getCommuterRailInfo(IDialogContext context, LuisResult result)
        {
            //_mode = "commuter rail";
            await context.PostAsync($"Mode is deteced as commuter rail");
            context.Wait(MessageReceived);
        }



        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Sorry I didn't understand that");
            context.Wait(MessageReceived);
        }


    }
}