/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using POka.Diagnostics;

namespace CatchIt.GTFS
{
    // public enum eServiceStatus { Ok, NoDataNetwork, DataServiceUnabailable, Unknown };
    public enum eDirection1 { Outbound = 0, Inbound = 1};
    public enum eDirection2 { Southbound = 0, Northbound = 1};
    /// <summary>
    /// 
    /// </summary>
    public class JsonSettings
    {
        public static JsonSerializerSettings settings = new JsonSerializerSettings { 
                                                        NullValueHandling = NullValueHandling.Ignore, 
                                                        TypeNameHandling = TypeNameHandling.All };
    } 
    public class MBTAManager
    {
        private const string _APP_API_KEY           = "Rqz3iAVK7UqqWB6klQYU6Q"; // Paul Oka's (paul.oka@microsoft.com) MBTA Dev Key 
        private const string _SERVER_TIME           = "http://realtime.mbta.com/developer/api/v2/servertime?api_key={0}&format=json";
        private const string _ROUTE_LIST            = "http://realtime.mbta.com/developer/api/v2/routes?api_key={0}&format=json";
        private const string _ROUTE_LIST_BY_STOP    = "http://realtime.mbta.com/developer/api/v2/routesbystop?api_key={0}&stop={1}&format=json";
        private const string _STOP_LIST_BY_ROUTE    = "http://realtime.mbta.com/developer/api/v2/stopsbyroute?api_key={0}&route={1}&format=json";
        private const string _STOP_LIST_BY_LOCATION = "http://realtime.mbta.com/developer/api/v2/stopsbylocation?api_key={0}&lat={1}&lon={2}&format=json";
        private const string _SCHEDULE_BY_STOP      = "http://realtime.mbta.com/developer/api/v2/schedulebystop?api_key={0}&stop={1}&format=json";
        private const string _SCHEDULE_BY_ROUTE     = "http://realtime.mbta.com/developer/api/v2/schedulebyroute?api_key={0}&route={1}&format=json";
        private const string _PREDICTION_BY_ROUTE   = "http://realtime.mbta.com/developer/api/v2/predictionsbyroute?api_key={0}&route={1}&include_service_alerts=true&format=json";
        private const string _PREDICTION_BY_STOP    = "http://realtime.mbta.com/developer/api/v2/predictionsbystop?api_key={0}&stop={1}&include_service_alerts=true&format=json";
        private const string _ALERTS_LIST           = "http://realtime.mbta.com/developer/api/v2/alerts?api_key={0}&include_service_alerts=true&include_access_alerts=true&format=json";
        private const string _ALERTS_BY_ROUTE       = "http://realtime.mbta.com/developer/api/v2/alertsbyroute?api_key={0}&route={1}&include_service_alerts=true&include_access_alerts=false&format=json";
        private const string _ALERTS_BY_STOP        = "http://realtime.mbta.com/developer/api/v2/alertsbystop?api_key={0}&stop={1}&include_service_alerts=true&include_access_alerts=true&format=json";
        private const string _ALERTS_BY_ID          = "http://realtime.mbta.com/developer/api/v2/alertbyid?api_key={0}&id={1}&include_service_alerts=true&include_access_alerts=true&format=json";
        private const string _ALERT_HEADERS         = "http://realtime.mbta.com/developer/api/v2/alertheaders?api_key={0}&include_service_alerts=true&include_access_alerts=true&format=json";
        private const string _ALERT_HEADERS_BY_ROUTE = "http://realtime.mbta.com/developer/api/v2/alertheadersbyroute?api_key={0}&route={1}&include_service_alerts=true&include_access_alerts=true&format=json";


        static public readonly List<string> hideRouteId = new List<string>() 
        {"01", "02", "03", "04", "05", "06", "07", "08", "09", "946_", "948_", "903_", "913_", "931_", "933_",
        "810_", "813_", "823_", "830_", "831_", "840_", "842_", "851_", "852_", "880_", "882_", "899_"};
        // New subway Route Ids
        // Blue,Green-B, Green-C, Green-D, Green-E, Orange, Red, Mattapan
        // Implemented 2015-03-21.
        protected static Routes _routes = null;
        protected static CatchIt.Data.eServiceStatus _serviceStatus = CatchIt.Data.eServiceStatus.Ok;
        public static CatchIt.Data.eServiceStatus Status { get { return _serviceStatus; } private set { _serviceStatus = value; } }
        public static bool HideRouteId(string routeId) {return hideRouteId.Contains(routeId);}

        /// <summary>
        /// This function will return a alerts for the specified route
        /// </summary>
        /// <param name="routeId"></param>
        /// <returns></returns>
        public static async Task<Alerts> GetAlertsByRoute(string routeId)
        {
            if (routeId == null || routeId == "") return new Alerts();

            try
            {
                string jsonAlerts = await GetWebData(string.Format(_ALERTS_BY_ROUTE, _APP_API_KEY, routeId));
                if (jsonAlerts != null)
                {
                    Alerts alerts = JsonConvert.DeserializeObject<Alerts>(jsonAlerts, JsonSettings.settings);
                    return alerts;
                }
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.GetAlertsByRoute()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return new Alerts();

        }
        public static async Task<AlertHeaders> GetAlertHeadersByRoute(string routeId)
        {
            if (routeId == null || routeId == "") return new AlertHeaders();

            try
            {
                string jsonAlerts = await GetWebData(string.Format(_ALERT_HEADERS_BY_ROUTE, _APP_API_KEY, routeId));
                if (jsonAlerts != null)
                {
                    AlertHeaders alertHeaders = JsonConvert.DeserializeObject<AlertHeaders>(jsonAlerts, JsonSettings.settings);
                    return alertHeaders;
                }
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.GetAlertHeadersByRoute()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return new AlertHeaders();

        }
        /// <summary>
        /// This function will return a alerts for the specified stop
        /// </summary>
        /// <param name="stopId"></param>
        /// <returns></returns>
        public static async Task<Alerts> GetAlertsByStop(string stopId)
        {
            if (stopId == null || stopId == "") return new Alerts();

            try
            {
                string jsonAlerts = await GetWebData(string.Format(_ALERTS_BY_STOP, _APP_API_KEY, stopId));
                if (jsonAlerts != null)
                {
                    Alerts alerts = JsonConvert.DeserializeObject<Alerts>(jsonAlerts, JsonSettings.settings);
                    return alerts;
                }
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.GetAlertsByStop()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return new Alerts();
        }
        /// <summary>
        /// This function will get a alert specific to the an ID. If alert is not found 
        /// it will return null.
        /// </summary>
        /// <param name="alertId"></param>
        /// <returns>Can return null if alertId can not be found</returns>
        public static async Task<Alert> GetAlertById(string alertId)
        {
            if (alertId == null || alertId == "") return null;

            try
            {
                string jsonAlerts = await GetWebData(string.Format(_ALERTS_BY_ID, _APP_API_KEY, alertId));
                if (jsonAlerts != null)
                {
                    Alert alert = JsonConvert.DeserializeObject<Alert>(jsonAlerts, JsonSettings.settings);
                    return alert;
                }
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.GetAlertsById()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return null;
        }
        /// <summary>
        /// This function will return predictions for upcoming trips (including trips already underway) in a direction for a particular route.
        /// </summary>
        /// <param name="routeId"></param>
        /// <returns></returns>
        public static async Task<PredictionByRoute> GetPredictionByRoute(string routeId)
        {
            if (routeId == null || routeId == "") return new PredictionByRoute();

            try
            {
                string jsonPrediction = await GetWebData(string.Format(_PREDICTION_BY_ROUTE, _APP_API_KEY, routeId));
                if (jsonPrediction != null)
                {
                    PredictionByRoute prediction = JsonConvert.DeserializeObject<PredictionByRoute>(jsonPrediction, JsonSettings.settings);
                    //prediction.server_time = await GetServerTime();
                    return prediction;
                }
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.GetPredictionByRoute()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return new PredictionByRoute();
        }
        public static async Task<PredictionByRoute> GetPredictionsScheduleByRoute(string routeId, string stopId, string directionId)
        {
            // Return empty result if invalid parameters are sent
            if (routeId == null || routeId == "" || stopId == null || stopId == "") return new PredictionByRoute();

            try
            {
                // Get Prediction for the route
                PredictionByRoute prediction = await GetPredictionByRoute(routeId);

                if (prediction == null || prediction.direction == null)
                {
                    prediction = new PredictionByRoute();
                    prediction.direction = new List<Direction>();
                    prediction.route_id = routeId;
                }
                
                Direction direction = prediction.direction.Find(d => d.direction_id == directionId);

                // if we have any prediction return. If the route is a Commuter Rail, add the schedule. 
                if (direction != null && direction.trip.Count > 0)
                {
                    return prediction; 
                }
                // Not getting at predictions from MBTA, add scheduled trips.

                // Return schedules for the next 3 hours, but max out at 5 trips
                ScheduleByRoute schedule = await GetSchedByRoute(routeId, directionId, "180", "5");

                if (schedule == null || prediction == null || schedule.direction == null) return new PredictionByRoute();
                
                if (prediction.route_name == null) prediction.route_name = schedule.route_name;

                // Build a merged dataset of scheduled and prediction trips.
                // Go through the schedule and look for matching trips in the prediction and combine them.

                foreach (var preDirection in prediction.direction)
                {
                    if (directionId != null && directionId != preDirection.direction_id) continue;
                    
                    foreach (var trip in preDirection.trip)
                    {
                        //foreach (var stop in trip.stop)
                        Stop stop = trip.stop.Find(s => s.stop_id == stopId);
                        if (stop != null)
                        {
                            // Found the right stop going in the right direction
                            // Merge or add this stop data with the schedule
                            Stop schedStop = FindStop(stop.stop_id, trip.trip_id, directionId, schedule);

                            if (schedStop == null)
                                AddStop(stopId, trip.trip_id, directionId, schedule, trip);
                            else
                                MergeStop(schedStop, stop);
                        }
                    }
                }
                // Set the prediction to merged prediction/schedule data
                prediction.direction = schedule.direction;

                return prediction;
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.GetPredictionsScheduleByRoute()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
            }
            #endregion
            return new PredictionByRoute();
        }
        /// <summary>
        /// This function will return predicted arrivals and departures in the next hour for a direction and 
        /// route for a particular stop.
        /// </summary>
        /// <param name="StopId"></param>
        /// <returns></returns>
        public static async Task<PredictionByStop> GetPredictionByStop(string StopId)
        {
            if (StopId == null || StopId == "") return new PredictionByStop();
            string jsonPrediction = "";
            try
            {
                jsonPrediction = await GetWebData(string.Format(_PREDICTION_BY_STOP, _APP_API_KEY, StopId));
                if (jsonPrediction != null)
                {
                    PredictionByStop prediction = JsonConvert.DeserializeObject<PredictionByStop>(jsonPrediction, JsonSettings.settings);
                    //prediction.server_time = await GetServerTime();
                    return prediction;
                }
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.GetPredictionByStop()";
                Debug.WriteLine("{0}:{1} {2}", Signiture, Log.FormatExceptionMsg(e), jsonPrediction);
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return new PredictionByStop();
        }
        public static async Task<PredictionByStop> GetPredictionsScheduleByStop(string StopId, List<string> RouteIds = null, string DirectionId = null)
        {
            if (StopId == null || StopId == "") return new PredictionByStop();
            string jsonPrediction = "";
            try
            {
                // Get Predictions for the stop
                PredictionByStop prediction = await GetPredictionByStop(StopId);
                // Get schedule for the stop
                string RouteId = null;
                // Assign the routeId only if there is 1, if more than one leave RouteId = null to get all routes for the stop.
                if ((RouteIds != null) && (RouteIds.Count == 1)) RouteId = RouteIds[0];
                
                // Return schedules for the next 3 hours, but max out at 5 trips
                ScheduleByStop schedule = await GetSchedByStop(StopId, RouteId, DirectionId, "180", "5");

                //prediction.server_time = await GetServerTime();

                if (schedule == null || prediction == null || schedule.mode == null) return new PredictionByStop();

                if (prediction.mode == null)
                {
                    // No prediction data, just return the schedule data.
                    prediction.mode = schedule.mode;
                    prediction.stop_id = schedule.stop_id;
                    prediction.stop_name = schedule.stop_name;
                    return prediction;
                }

                // Go through the schedule and look for matching trips in the prediction and combine them.
                foreach (var mode in prediction.mode)
                {
                    string modeName = mode.mode_name;
                    foreach (var route in mode.route)
                    {
                        string routeId = route.route_id;
                        if (RouteIds != null && !RouteIds.Contains(routeId)) continue;

                        foreach (var direction in route.direction)
                        {
                            string directionId = direction.direction_id;
                            if (DirectionId != null && DirectionId != directionId) continue;
                            // Look at each trip in prediction and match it to the schedule.
                            foreach (var trip in direction.trip)
                            {
                                string tripId = trip.trip_id;
                                Trip schedTrip = FindTrip(tripId, modeName, routeId, directionId, schedule.mode);

                                if (schedTrip == null)
                                    AddTrip(schedule.mode, trip, modeName, routeId, directionId);
                                else
                                    MergeTrips(schedTrip, trip);
                            }
                        }
                    }
                }               
                prediction.mode = schedule.mode;
                return prediction;
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.GetPredictionsScheduleByStop()";
                Debug.WriteLine("{0}:{1} {2}", Signiture, Log.FormatExceptionMsg(e), jsonPrediction);
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
            }
            #endregion
            return new PredictionByStop();
        }
        /// <summary>
        /// This function will return a complete list of routes for which data can be requested through the web services.
        /// <returns></returns>
        public static async Task<Routes> GetRoutes()
        {
            if (_routes == null)
            {
                try
                {
                    string jsonRoutes = await GetWebData(string.Format(_ROUTE_LIST, _APP_API_KEY));
                    _routes = JsonConvert.DeserializeObject<Routes>(jsonRoutes, JsonSettings.settings);
                }
                #region Exception Handler
                catch (Exception e)
                {
                    // Log error
                    const string Signiture = "MBTAManager.GetRoutes()";
                    Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));              
                    Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                    Debugger.Break();
#endif
                    return (new Routes());
                }
                #endregion
            }

            return _routes;
        }
        /// <summary>
        /// This function will return a list of routes that serve a particular stop.
        /// </summary>
        /// <param name="stopId"></param>
        /// <returns></returns>
        public static async Task<Routes> GetRouteByStopId(string stopId)
        {
            if (stopId == null || stopId == "") return new Routes();
            Routes routes;

            try
            {
                string jsonRoutes = await GetWebData(string.Format(_ROUTE_LIST_BY_STOP, _APP_API_KEY, stopId));
                routes = JsonConvert.DeserializeObject<Routes>(jsonRoutes, JsonSettings.settings);
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.GetRouteByStopId()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
                return new Routes();
            }
            #endregion
            return routes;
        }
        /// <summary>
        /// This function will return the scheduled arrivals and departures for the next five trips 
        /// (including trips already underway) for a particular route.
        /// </summary>
        /// <param name="routeId"></param>
        /// <returns></returns>
        public static async Task<ScheduleByRoute> GetSchedByRoute(string routeId, string directionId = null, string maxTime = null, string maxTrips = null)
        {
            if (routeId == null || routeId == "") return new ScheduleByRoute();

            try
            {
                string parameters = string.Format("{0}", routeId);
                if (directionId != null) parameters += string.Format("&direction={0}", directionId);
                if (maxTime != null) parameters += string.Format("&max_time={0}", maxTime);
                if (maxTrips != null) parameters += string.Format("&max_trips={0}", maxTrips);

                string jsonSched = await GetWebData(string.Format(_SCHEDULE_BY_ROUTE, _APP_API_KEY, parameters));
                if (jsonSched != null) return JsonConvert.DeserializeObject<ScheduleByRoute>(jsonSched, JsonSettings.settings);
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.GetSchedByRoute()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return new ScheduleByRoute();
        }
        /// <summary>
        /// This function will return up to the next five scheduled arrivals and departures in the next hour 
        /// route for a particular stop. It can be filtered by the routeId and directionId
        /// </summary>
        /// <param name="stopId"></param>
        /// <param name="routeId=null"></param>
        /// <param name="directionId=null"></param>
        /// <returns></returns>
        public static async Task<ScheduleByStop> GetSchedByStop(string stopId, string routeId = null, string directionId = null, string maxTime = null, string maxTrips = null)
        {
            if (stopId == null || stopId == "") return new ScheduleByStop();


            try
            {
                string parameters = string.Format("{0}", stopId);
                if (routeId != null) parameters += string.Format("&route={0}", routeId);
                if (directionId != null) parameters += string.Format("&direction={0}", directionId);
                if (maxTime != null) parameters += string.Format("&max_time={0}", maxTime);
                if (maxTrips != null) parameters += string.Format("&max_trips={0}", maxTrips);

                string jsonSched = await GetWebData(string.Format(_SCHEDULE_BY_STOP, _APP_API_KEY, parameters));
                if (jsonSched != null) return JsonConvert.DeserializeObject<ScheduleByStop>(jsonSched, JsonSettings.settings);
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.GetSchedByStop()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return new ScheduleByStop();
        }
        /// <summary>
        /// This query will return the current server time.
        /// </summary>
        /// <returns>ServerTime object</returns>
        public static async Task<ServerTime> GetServerTime()
        {
            try
            {
                string jsonServertime = await GetWebData(string.Format(_SERVER_TIME, _APP_API_KEY));
                if (jsonServertime != null) return JsonConvert.DeserializeObject<ServerTime>(jsonServertime, JsonSettings.settings);
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.GetServerTime()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return new ServerTime();
        }
        /// <summary>
        /// This function will return a list of the nearest stops from a particular location. Up to 15 are returned, 
        /// within a 1-mile radius.
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>
        public static async Task<StopByLocation> GetStopsByLocation(string lat, string lon)
        {
            if (lat == null || lon == null || lat == "" || lon == "") return new StopByLocation();

            try
            {
                string jsonStops = await GetWebData(string.Format(_STOP_LIST_BY_LOCATION, _APP_API_KEY, lat, lon));
                if (jsonStops != null) return JsonConvert.DeserializeObject<StopByLocation>(jsonStops, JsonSettings.settings);
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.GetStopsByLocation()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return new StopByLocation();
        }
        /// <summary>
        /// This function will return a list of stops for a particular route.
        /// </summary>
        /// <param name="routeId"></param>
        /// <returns></returns>
        public static async Task<StopByRoute> GetStopsByRoute(string routeId)
        {
            if (routeId == null || routeId == "") return new StopByRoute();

            try
            {
                string jsonStops = await GetWebData(string.Format(_STOP_LIST_BY_ROUTE, _APP_API_KEY, routeId));
                if (jsonStops != null) return JsonConvert.DeserializeObject<StopByRoute>(jsonStops, JsonSettings.settings);
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.GetStopsByRoute()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return new StopByRoute();
        }
        /// <summary>
        /// This function make a web call and returns a json response.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        internal static async Task<string> GetWebData(string uri)
        {
            if (uri == null || uri == "") return "";

            // Instantiate HttpClient to set BufferSize
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue();
            httpClient.DefaultRequestHeaders.CacheControl.NoCache = true;
            httpClient.Timeout = new TimeSpan(0, 0, 60);
            httpClient.MaxResponseContentBufferSize = 1024 * 1024; // Read up to 1 MB of data

            HttpResponseMessage response;
            int i = 0;
            try
            {
                do
                {
                    response = await httpClient.GetAsync(uri);
                    if (response.IsSuccessStatusCode)
                    {
                        _serviceStatus = CatchIt.Data.eServiceStatus.Ok;
                        return await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        _serviceStatus = CatchIt.Data.eServiceStatus.DataServiceUnavailable;
                    }
                    i++;

                } while (!response.IsSuccessStatusCode && i < 1);
            }
            #region Exception Handler
            catch (Exception e)
            {
                _serviceStatus = CatchIt.Data.eServiceStatus.NoDataNetwork;
                const string Signiture = "MBTAManager.GetWebData()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return null;
        }
        internal static Stop FindStop(string stopId, string tripId, string directionId, ScheduleByRoute schedule)
        {
            if (stopId == "" || tripId == "" || directionId == "" || schedule == null) return null;

            try
            {
                Direction direction = schedule.direction.Find(d => d.direction_id == directionId);
                if (direction == null) return null;

                Trip trip = direction.trip.Find(t => t.trip_id == tripId);
                if (trip == null) return null;

                Stop stop = trip.stop.Find(s => s.stop_id == stopId);
                if (stop == null) return null;

                // Found the right stop.
                return stop;
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.FindStop()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return null;

        }
        internal static Trip FindTrip(string tripId, string modeName, string routeId, string directionId, List<Mode> modes)
        {
            // Return null if no trip is found
            if (modes == null || tripId == "" || modeName == "" || routeId == "" || directionId == "") return null;
            
            try
            {
                Mode mode = modes.Find(m => m.mode_name == modeName);
                if (mode == null) return null;

                Route route = mode.route.Find(r=>r.route_id == routeId);
                if (route == null) return null;

                Direction direction = route.direction.Find(d=>d.direction_id == directionId);
                if (direction == null) return null;

                Trip trip = direction.trip.Find(t=>t.trip_id == tripId);       
                return trip;
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.FindTrip()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return null;
        }
        internal static bool AddStop(string stopId, string tripId, string directionId, ScheduleByRoute schedule, Trip trip)
        {
            if (stopId == "" || tripId == "" || directionId == "" || schedule == null || trip == null) return false;

            try
            {   // find the right direction & trip to add the stop.
                Direction direction = schedule.direction.Find(d => d.direction_id == directionId);
                if (direction == null) return false;

                Trip schedTrip = direction.trip.Find(t => t.trip_id == tripId);
                if (schedTrip == null)
                {
                    direction.trip.Add(trip);
                    return true;               
                }

                // Found the right trip, add stop to the list of stops.
                Stop stop = trip.stop.Find(s => s.stop_id == stopId);
                if (stop != null) schedTrip.stop.Add(stop);
                return true;
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.AddStop()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return false;
        }
        internal static bool AddTrip(List<Mode> modes, Trip trip, string modeName, string routeId, string directionId)
        {
            if (modes == null || trip == null || modeName == "" || routeId == "" || directionId == "") return false;

            try
            {
                Mode mode = modes.Find(m => m.mode_name == modeName);
                if (mode == null) return false;

                Route route = mode.route.Find(r => r.route_id == routeId);
                if (route == null) return false;

                Direction direction = route.direction.Find(d => d.direction_id == directionId);
                if (direction == null) return false;

                direction.trip.Add(trip);
                // TODO: Sort Trips by pre_dt
                //direction.trip.Sort()
                return true;
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.AddTrip()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return false;
        }
        internal static bool MergeStop(Stop target, Stop source)
        {
            if (target == null || source == null) return false;

            try
            {
                if (source.pre_dt != "") target.pre_dt = source.pre_dt;
                if (source.pre_away != "") target.pre_away = source.pre_away;
                return true;
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.MergeStop()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return false;
        }
        internal static bool MergeTrips(Trip target, Trip source)
        {
            if (target == null || source == null || target.trip_id != source.trip_id) return false;
            try
            {
                if (source.pre_dt != "") target.pre_dt = source.pre_dt;
                if (source.pre_away != "") target.pre_away = source.pre_away;
                if (source.trip_headsign != "") target.trip_headsign = source.trip_headsign;
                return true;
            }
            #region Exception Handler
            catch (Exception e)
            {
                // log error
                const string Signiture = "MBTAManager.MergeTrips()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return false;
        }
    }
}