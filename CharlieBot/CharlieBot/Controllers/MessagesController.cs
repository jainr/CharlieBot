using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using CatchIt.Data;
using CatchIt.GTFS;
using System.Collections.Generic;
using System.Diagnostics;

namespace CharlieBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
#if false
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // calculate something for us to return
                int length = (activity.Text ?? string.Empty).Length;

                // return our reply to the user
                Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
#endif

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

#if true
        // ---------------------------------------------- Paul's Code -----------------------------------------
        // <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // Get Users specific state data
                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                string routeId = GetId(activity.Text, "route");
                if (string.IsNullOrEmpty(routeId))
                {
                    // set local routeId from user state
                    routeId = userData.GetProperty<string>("RouteId");
                }
                string stopName = GetId(activity.Text, "stop");
                if (string.IsNullOrEmpty(stopName))
                {
                    // set local routeId from user state
                    stopName = userData.GetProperty<string>("StopId");
                }

                string stopId = await GetStopId(routeId, stopName);

                if (!string.IsNullOrEmpty(stopId))
                {
                    // Set StopID State
                    userData.SetProperty<string>("StopId", stopId);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                }

                if (!string.IsNullOrEmpty(routeId))
                {
                    // Set StopID State
                    userData.SetProperty<string>("RouteId", routeId);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                }


#if false
                // Set StopID State
                userData.SetProperty<string>("StopId", "place-knncl");
                await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

                // Set RouteID State
                userData.SetProperty<string>("RouteId", "64");
                await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
#endif

                if (userData.GetProperty<string>("StopId") == null || userData.GetProperty<string>("RouteId") == null)
                {
                    string errReply = "";
                    if (userData.GetProperty<string>("StopId") == null)
                    {
                        errReply = "What stop do you want to leave from?";
                    }
                    else if (userData.GetProperty<string>("RouteId") == null)
                    {
                        errReply = "What route do you want to travel?";
                    }
                    Activity reply = activity.CreateReply(errReply);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else
                {
                    // Call MBTA API for realtime data
                    string stopID = userData.GetProperty<string>("StopId");
                    List<string> routeIds = new List<string>();
                    routeIds.Add(userData.GetProperty<string>("RouteId"));
                    // Delete save state 
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);


                    FavoriteDataModel fdm = null;
                    fdm = await DataManager.Favorites(stopID, stopName, routeIds);


                    // Format the replies to users.
                    if (fdm != null && fdm.Routes != null && fdm.Routes[0].Predictions != null)
                    {
                        foreach (RouteDataModel rdm in fdm.Routes)
                        {
                            foreach (PredictionDataModel pdm in rdm.Predictions)
                            {
                                int cnt = 0;
                                foreach (Tuple<bool, DateTime> p in pdm.Predictions)
                                {

                                    TimeSpan diff = p.Item2 - DateTime.Now;
                                    string mbtaReply = string.Format("The {0} arrives at {1} in {2} mins", pdm.Destination, rdm.StopName, diff.Minutes.ToString()); // "The next train to Braintree arrives in 9 min"

                                    Activity reply = activity.CreateReply(mbtaReply);
                                    await connector.Conversations.ReplyToActivityAsync(reply);

                                    // Only show 3 predition max.
                                    if (cnt++ >= 2) break;

                                }
                            }
                        }
                    }
                    // return our reply to the user

                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
        private string GetId(string msgText, string searchText)
        {
            int i = msgText.IndexOf(searchText);

            if (i < 0) return "";
            i += searchText.Length + 1;

            string retVal = "";
            while ((i < msgText.Length) && (msgText[i] != ' '))
            {
                retVal += msgText[i];
                i++;
            }

            return retVal;
        }

        private async Task<string> GetStopId(string routeId, string stopName)
        {
            if (string.IsNullOrEmpty(routeId) || string.IsNullOrEmpty(stopName)) return "";
            StopByRoute stops = await MBTAManager.GetStopsByRoute(routeId);
            try
            {
                foreach (var d in stops.direction)
                {
                    CatchIt.GTFS.Stop stop = d.stop.Find(s => s.stop_id == stopName);
                    if (stop != null) return stop.stop_id;

                    // Keep looking for stop Id
                    foreach (var s in d.stop)
                    {
                        if (s.parent_station_name.ToLower().Contains(stopName.ToLower()) || (s.stop_name.ToLower().Contains(stopName.ToLower())))
                        {
                            if (!string.IsNullOrEmpty(s.parent_station)) return s.parent_station;
                            return s.stop_id;
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message.ToString());
                Debugger.Break();                
            }
            return "";
        }
    }
#endif
}