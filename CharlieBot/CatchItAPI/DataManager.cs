/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Text;
using System.Threading.Tasks;
//using System.Linq;
//using System.IO;
//using Windows.Storage;
//using SQLite;
//using Windows.Foundation;
//using System.Net.Http;
//using Windows.Devices.Geolocation;
using CatchIt.GTFS;
using POka.Diagnostics;

namespace CatchIt.Data
{
    #region Data Models
    public class AlertDataModel
    {
        public string AlertId { get; set; }
        public string Text { get; set; }
        public string EffectName { get; set; }
        public AlertDataModel(AlertHeader alert)
        {
            AlertId = alert.alert_id;
            Text = alert.header_text;
            EffectName = alert.effect_name;
        }
        public AlertDataModel()
        { 
        }
        public new string ToString()
        {
            return string.Format("alert_id({0}), header_text({1}), effect_name({2})", AlertId, Text, EffectName);
        }
    }

    public class PredictionDataModel
    {
        private string _directionName;
        private string _directionId;
        private string _destination;
        public string Destination
        {
            get
            {
                if (_destination == "") return _directionName;
                return _destination;
            }
        }
        public string DirectionId { get { return _directionId; } }
        public List<Tuple<bool, DateTime>> Predictions { get; private set; }
        public Dictionary<String, Vehicle> Vehicles { get; private set; } // Dictionary of Trip Name & Vehicle data.
        public PredictionDataModel (string destination, string direction_id, string direction_name, List<Tuple<bool, DateTime>> predictions = null, Dictionary<String, Vehicle> vehicles = null)
        {
            _destination = destination;
            _directionName = direction_name;
            _directionId = direction_id;
            Predictions = predictions == null ? new List<Tuple<bool, DateTime>>() : predictions;;
            Vehicles = vehicles == null ? new Dictionary<String, Vehicle>() : vehicles;
        }
    }
    public class RouteDataModel
    {
        public string RouteId { get; private set; }
        public string RouteName { get; private set; }
        public string ModeName { get; private set; }
        public string StopName { get; set; }       
        public string StopId { get;  set; }             // Could be null
        public string DirectionId { get; set; }         // Could be null
        public string ToStopId { get; set; }            // Could be null
        public List<AlertDataModel> Alerts { get; private set; }
        public List<PredictionDataModel> Predictions { get; private set; }
        public RouteDataModel(string route_id, string route_name, string mode_name, List<PredictionDataModel> predictions = null, List<AlertDataModel> alerts = null)
        {
            ModeName = mode_name;
            RouteId = route_id;
            if (mode_name == "Bus") 
                RouteName = "Bus Route " + route_name;
            else
                RouteName = route_name;
            Predictions = predictions == null ? new List<PredictionDataModel>() : predictions;
            Alerts = alerts == null ? new List<AlertDataModel>() : alerts;
        }
    }
    public class StopDataModel
    {
        /*
         *  Stops view needs the following data
         *  - Stop Name
         *  - Stop Id
         *  - List of Alerts for the stop
         *  - List of Routes
         *      - Route Id - (_931, 1, CR-Fitchburgh)
         *      - Route Name - (Red Line, Bus Route 1, Fitchburg/Acton Line
         *      - Mode Name - (Subway, Bus, Communter Rail)
         *      - List of Alerts for the route
         *      - Dictionary - Destination, List of Predictions 
         */
        public string StopName { get; private set; }
        public string StopId { get; private set; }
        public List<AlertDataModel> Alerts { get; private set; }
        public List<RouteDataModel> Routes { get; private set; }
        public eServiceStatus Status { get { return DataManager.Status; } }
        public StopDataModel(string stop_name, string stop_id, List<RouteDataModel> routes = null, List<AlertDataModel> alerts = null)
        {
            StopName = stop_name;
            StopId = stop_id;
            Routes = routes == null ? new List<RouteDataModel>() : routes;
            Alerts = alerts == null ? new List<AlertDataModel>() : alerts;
        }
    }
    public class FavoriteDataModel
    {
        public string StopName { get; set; }
        public string StopId { get; set; }
        public string RouteId { get; set; }
        public string Destination { get; set; }
        public string DirectionId { get; set; }
        public string FavoriteId { get; set; }
        public string ToStopName { get; set; }
        public List<RouteDataModel> Routes { get; private set; }
        public eServiceStatus Status { get { return DataManager.Status; } }
        public FavoriteDataModel(string stop_name, string stop_id, string destination, string direction_id, List<RouteDataModel> routes = null)
        {
            StopName = stop_name;
            StopId = stop_id;
            Destination = destination;
            DirectionId = direction_id;
            Routes = routes == null ? new List<RouteDataModel>() : routes;
        }
    }
    #endregion

    /// <summary>
    /// The DataManager class manages the data layer of the application. It contains all of the transport
    /// managers and maintains the connection with the database. Hence, it is responsible for gathering
    /// both real-time data and static data for the application. In the case that the application is abstracted
    /// such that the GTFS database layer needs to be swapped out with another database, it makes sense to
    /// implement a GTFSManager which encapsulates the database connection. Paul Oka has already drafted an
    /// implementation and should be consulted for more details.
    /// </summary>
    public class DataManager
    {
        #region Protected Members Variables
        protected static Dictionary<string, Dictionary<string, List<string>>> _allLines;
        protected static eServiceStatus _status = eServiceStatus.Ok;
        #endregion

        public static eServiceStatus Status
        {
            get 
            {
                eServiceStatus status = MBTAManager.Status;
                if (status != eServiceStatus.Ok) return status;
                return _status; 
            }
        }

        public static async Task<FavoriteDataModel> Favorites(string stopId, string stopName, List<string> routeIds = null, string directionId = null, string modeName = null, string toStopId = null, string toStopName = null)
        {
            FavoriteDataModel result = new FavoriteDataModel("", "", "", "");
            List<RouteDataModel> routes = null;

            if (string.IsNullOrEmpty(stopId)) return result;
            try
            {
                if (routeIds != null && routeIds.Count == 1 && directionId != null)
                {
                    // Get prediction By Routes
                    PredictionByRoute prediction = await MBTAManager.GetPredictionsScheduleByRoute(routeIds[0], stopId, directionId);
                    if (prediction == null || prediction.direction == null) return result;
                    if (prediction.mode_name == null || prediction.mode_name == "") prediction.mode_name = modeName;

                    routes = BuildRouteDataModel(prediction, stopId, stopName, toStopId, toStopName, directionId);
                }
                else
                {
                    // Get prediction data by Stop ID
                    PredictionByStop prediction = await MBTAManager.GetPredictionsScheduleByStop(stopId, routeIds, directionId);
                    // Need to find all of the trip objects and convert them into Predictions.
                    // Prediction->Mode->Route->Direction->Trip

                    if (prediction == null || prediction.mode == null) return result;
                    stopName = prediction.stop_name;

                    AlertHeaders alertHeaders = null;
                    if (routeIds != null && routeIds.Count >= 1)
                    {
                        // Get alerts for the routeId
                        alertHeaders = GetAlertHeaders(await MBTAManager.GetAlertsByRoute(routeIds[0]));
                    }
                    routes = BuildRouteDataModel(prediction, alertHeaders);
                }

                if (routes == null) return result;

                result = new FavoriteDataModel(stopName, stopId, "", directionId, routes);
                if (string.IsNullOrEmpty(result.ToStopName)) result.ToStopName = toStopName;
            }
            #region Exception Handling
            catch (Exception e)
            {
                Debug.WriteLine("{0}:{1}", "DataManager.FavoriteByStopId", Log.FormatExceptionMsg(e));
                Log.WriteLine("DataManager.FavoriteByStopId", Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            #endregion
            return result;
        }
        public static async Task<List<StopDataModel>> StopsNearby(string lat = null, string lon = null)
        {
            List<StopDataModel> result = new List<StopDataModel>();
            // StopName = ""Kendall/MIT" 
            // RouteName = "Red Line", "Bus Route 1"
            // ModeName = "Subway" 
            // Direction = "Southbound"
            // Destination = "Ashmount"
            // 
            try
            {
                // Get List of Nearby Stops
                // Get Predictions for each of the stops.
                // Build Stop Data model
                IEnumerable<string> stopIds = await DataManager.GetStopsByLocation(lat, lon);
                if (stopIds == null) return result;

                foreach (var stopId in stopIds)
                {

                    PredictionByStop predictions = await MBTAManager.GetPredictionsScheduleByStop(stopId);

                    string StopName = predictions.stop_name;
                    string StopId = predictions.stop_id;

                    if (predictions.mode == null) continue;

                    List<RouteDataModel> routeList = BuildRouteDataModel(predictions);
                    result.Add(new StopDataModel(StopName, StopId, routeList, null));

                }
                return result;
            }
            #region Exception Handling
            catch (Exception e)
            {
                // Some stops will not have data...
                string Signiture = "DataManager.StopsNearby()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
            }

            #endregion
            return new List<StopDataModel>();
        }
        public static async Task<Dictionary<string, Dictionary<string, List<string>>>> AllLines()
        {
            if (_allLines != null) return _allLines;
            Dictionary<string, Dictionary<string, List<string>>> result = new Dictionary<string, Dictionary<string, List<string>>>();

            try
            {
                Routes routes = await MBTAManager.GetRoutes();

                if (routes != null)
                {
                    // for each Mode, look at the route type
                    foreach (var m in routes.mode)
                    {
                        if (!result.ContainsKey(m.route_type))
                        {
                            // Add Route Type Key (e.g. Subway, Rail ...)
                            result.Add(m.route_type, (new Dictionary<string, List<string>>()));
                        }

                        // for each Route 
                        foreach (var r in m.route)
                        {
                            // if route should be hidden skip to the next one
                            if (r.route_hide == "true") continue;
                            // TODO: Remove this hack due to MBTA RouteId changes
                            if (MBTAManager.HideRouteId(r.route_id)) continue;

                            if (!result[m.route_type].ContainsKey(r.route_name))
                            {
                                // Add new line with current routeId
                                List<string> lst = new List<string>();
                                result[m.route_type].Add(r.route_name, lst);
                            }

                            // Add routeId to line list 
                            result[m.route_type][r.route_name].Add(r.route_id);
                        }
                    }
                    _allLines = result;
                }
                _status = eServiceStatus.Ok;
            }
            catch (Exception e)
            {
                _status = eServiceStatus.Unknown;
                string Signiture = "DataManager.AllLines()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Debugger.Break();
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
            }

            return result;
        }
        public static async Task<Dictionary<string, Tuple<string, string>>> DestinationByLine(string modeName, string routeName)
        {
            Dictionary<string, Tuple<string, string>> result = new Dictionary<string, Tuple<string, string>>();
            if (string.IsNullOrEmpty(modeName) || string.IsNullOrEmpty(routeName)) return result;

            try
            {
                List<string> routeIds = await GetRouteIds(modeName, routeName);
                // RouteIds have changed... Now subway are Red, Orange, Blue ...
                // Get the first and last stop for each of the routes
                foreach (var routeId in routeIds)
                {
                    if (routeId == "948_" || routeId == "913_") continue; // Do not add destiniation for these routes
  
                    foreach (var direction in await GetFirstLastStopByRouteId(routeId))
                    {
                        List<GTFS.Stop> stops = direction.Value;

                        foreach (var stop in stops)
                        {
                            string stopName;
                            string directionId;

                            // Set the stop name
                            if (modeName == "Subway" || modeName == "Light Rail") // Subway
                            {
                                string tempName = stop.parent_station_name;
                                stopName = string.Equals(tempName, "Braintree") ? "Braintree/Ashmont" : tempName;
                            }
                                
                            else if (modeName == "Bus")
                                if (stop.stop_order == "1")
                                    stopName = "Inbound";
                                else
                                    stopName = "Outbound";
                            else 
                                stopName = stop.stop_name; // Commuter Rail 
                            
                            // Set the direction Id
                            if (direction.Key == "0")
                                if (stop.stop_order == "1")
                                    directionId = "1"; // stop is first stop on outbound route, so set the direction to inbound
                                else
                                    directionId = "0";
                            else
                                if (stop.stop_order == "1")
                                    directionId = "0"; // stop is first stop on inbound route, so set it outbound
                                else
                                    directionId = "1"; // Set the direction to inbound
                            // If stop does not exist in result add it
                            if (!result.ContainsKey(stopName))
                            {
                                Tuple<string, string> directionIdrouteId = new Tuple<string, string>(directionId, routeId);
                                result.Add(stopName, directionIdrouteId);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string Signiture = "DataManager.DestinationByLine()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }

#if DEBUG
            string d = "";
            foreach (var r in result)
            {
                d += ", " + r;
            }
            Debug.WriteLine("modeName({0}), routeName({1}) Destination = {2}", modeName, routeName, d);
#endif
            // Return only unique stops
            return result;
        }
        public static async Task<IEnumerable<string>> LinesNames(string modeName)
        {
            List<string> result = new List<string>();
            if (string.IsNullOrEmpty(modeName)) return result;
            try
            {
                Routes routes = await MBTAManager.GetRoutes();
                int modeType;

                switch (modeName)
                {
                    case "Subway":
                        modeType = 1;
                        break;
                    case "Commuter Rail":
                        modeType = 2;
                        break;
                    case "Bus":
                        modeType = 3;
                        break;
                    case "Light Rail":
                        modeType = 0;
                        break;
                    default:
                        modeType = 1;
                        break;
                }

                foreach (var r in routes.mode[modeType].route)
                {
                    if (r.route_hide == "true") continue;

                    string lineName = r.route_name;
                    // If stop does not exist in result add it
                    if (!result.Exists(s1 => s1 == lineName))
                    {
                        result.Add(lineName);
                    }
                }


            }
            catch (Exception e)
            {
                string Signiture = "DataManager.LinesNames()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }

            return result;
        }
        public static async Task<Dictionary<string, Stop>> StopsByRouteIds(List<string> routeIds, string directionId = null)
        {
            Dictionary<string, Stop> tempResult = new Dictionary<string, Stop>();
            Dictionary<string, Stop> result = new Dictionary<string, Stop>();

            if (routeIds == null || routeIds.Count == 0) return result;
            try
            {
                foreach (var routeId in routeIds)
                {
                    var stops = await MBTAManager.GetStopsByRoute(routeId);

                    foreach (var direction in stops.direction)
                    {
                        if (directionId == null || directionId == direction.direction_id)       // if we have a direction Id, only get the stops associated with it.
                        {
                            foreach (var stop in direction.stop)
                            {
                                // Get Parent Stop info if exisits
                                string stopName = (stop.parent_station_name == "") ?  stop.stop_name : stop.parent_station_name  ;
                                string stopId = (stop.parent_station == "") ? stop.stop_id : stop.parent_station;

                                if (!result.ContainsKey(stopName))
                                {
                                    // Add Stop
                                    if (routeId == "933_")    // Major hack to get the Red Line stop order correctly for northbound trains
                                        tempResult.Add(stopName, MBTADataHelper.ConvertStop(stop));
                                    else
                                        result.Add(stopName, MBTADataHelper.ConvertStop(stop));
                                }
                            }
                        }
                    }
                }
                //
                // Major hack to get the Red Line stop order correctly for northbound trains
                // #HACK
                if (tempResult.Count > 0)
                {
                    foreach (var r in result)
                    {
                        tempResult.Add(r.Key, r.Value);
                    }

                    result = tempResult;
                }

            }
            #region Exception Handling
            catch (Exception e)
            {
                string Signiture = "DataManager.StopsByRouteIds()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }

            #endregion
            return result;
        }
        public static string GetDestinationLabel(string modeName, string directionName, string destination)
        {
            string destinationLabel = "";

            switch (modeName)
            {
                case "Bus":
                    destinationLabel = "Next " + directionName + " Bus";
                    break;
                case "Light Rail":
                case "Subway":
                case "Commuter Rail":
                    if (destination != "")
                        destinationLabel = "Next Train to " + destination;
                    else
                        destinationLabel = "Next " + directionName + " Train";
                    break;
                default:
                    destinationLabel = "Next " + directionName;
                    break;

            }

            return destinationLabel;
        }
        
        #region Internal Membmer Function
        /// <summary>
        /// Return the destination stop for trip.
        /// </summary>
        /// <param name="trip"></param>
        /// <param name="direction"></param>
        /// <param name="mode"></param>
        /// <returns>
        /// Name of the destination stop for the bus
        /// </returns>
        internal static string GetDestination(Trip trip, Direction direction, string modeName)
        {
            if (trip == null || direction == null) return "";

            if (modeName == "Bus") 
                return direction.direction_name;
            else if (modeName == "Subway" && trip.trip_headsign != null && trip.trip_headsign != "") 
                return trip.trip_headsign;
            else if (trip.trip_headsign == null || trip.trip_headsign == "" )
                return GetDestinationFromTripName(trip.trip_name);
            else if (modeName == "Commuter Rail" && trip.trip_headsign != "") 
                return trip.trip_headsign;

            return direction.direction_name;
        }
        /// <summary>
        /// The function extracts the trip destination from the trip name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static string GetDestinationFromTripName(string name)
        {
            // Scan the trip name for a "to" destination, use the starting index 
            // of the "to" and return the rest of the string.
            // If scan fails, return the direction the bus is heading (e.g. Outbound, Inbound)

            // "11:32 pm from Dudley Station to Massachusetts Ave @ Holyoke St";
            const string searchString1 = " to ";
            const string searchString2 = " - ";

            int indx = name.IndexOf(searchString1);
            if (indx > 0)
            {
                string result = name.Substring(indx + searchString1.Length);

                int indx2 = result.IndexOf(searchString2);
                if (indx2 > 0)
                {
                    return (result.Substring(0, indx2));
                }
                
                return (result);
            }
            return "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeId"></param>
        /// <returns></returns>
        internal static async Task<Dictionary<string, List<GTFS.Stop>>> GetFirstLastStopByRouteId(string routeId)
        {
            Dictionary<string, List<GTFS.Stop>> result = new Dictionary<string, List<GTFS.Stop>>();
            if (string.IsNullOrEmpty(routeId)) return result;

            StopByRoute stops = await MBTAManager.GetStopsByRoute(routeId);
            List<Direction> direction = stops.direction;
            
            foreach (var d in direction)
            {
                if (!result.ContainsKey(d.direction_id))
                {
                    result.Add(d.direction_id, new List<GTFS.Stop>());
                }
                List<GTFS.Stop> stop = d.stop;

                if (stop.Count > 0)
                {
                    result[d.direction_id].Add(stop[0]);              // Get First element in the list
                    result[d.direction_id].Add(stop[stop.Count - 1]); // Get the last element in the list
                }
                
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="modeName"></param>
        /// <param name="routeName"></param>
        /// <returns></returns>
        internal static async Task<List<string>> GetRouteIds(string modeName, string routeName)
        {
            List<string> result = new List<string>();
            // Get a list of all the line in the transit systems
            if (_allLines == null)
            {
                await AllLines();
            }
            int modeType = (int) MBTADataHelper.ConvertTransitType(modeName);

            // Get a list of routes Id for the line
            result = _allLines[modeType.ToString()][routeName]; // "Subway", "Red Line"


            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="predictions"></param>
        /// <param name="fromStopId"></param>
        /// <returns></returns>
        internal static List<RouteDataModel> BuildRouteDataModel(PredictionByRoute predictions, string fromStopId, string fromStopName, 
            string toStopId, string toStopName, string directionId)
        {
            // Create empty result
            List<RouteDataModel> result = new List<RouteDataModel>();
            // Check for alerts 
            List<AlertDataModel> alerts = new List<AlertDataModel>();
 
            if (predictions.alert_headers != null && predictions.alert_headers.Count > 0)
            {
                foreach (var alert in predictions.alert_headers)
                {
                    alerts.Add(new AlertDataModel(alert));
                }
                // Reverse the order of alert headers. To put newer alerts at the begining of the list.
                alerts.Reverse();
            }

            List<PredictionDataModel> predictionList = new List<PredictionDataModel>();
            Dictionary<string, Vehicle> vehicleDict = new Dictionary<string, Vehicle>(); // List of active vehicles on the route. 

            // Need to find the right direction, 
            // then for each Trip find the right stop. . 
            foreach (var direction in predictions.direction)
            { // Loop through each direction of the route

                // Only process direction specifed by Direction ID
                if ((directionId != null) && (direction.direction_id != directionId)) continue;
                
                string destination = "";
                Dictionary<string, List<Tuple<bool, DateTime>>> routeToPredictionsDict = new Dictionary<string, List<Tuple<bool, DateTime>>>();
                //List<Tuple<bool, DateTime>> predictionTime = new List<Tuple<bool, DateTime>>();

                foreach (var trip in direction.trip)
                { // Loop through each trip running on the route on the specific direction
                    if (trip.trip_headsign != "") 
                    {
                        destination = toStopName;
                        if (destination == null)
                        {
                            // Find Destination from Trip object
                            destination = GetDestination(trip, direction, predictions.mode_name);
                            if (destination == fromStopName) continue;
                        }
                    }

                    // Find from stop by stopID
                    GTFS.Stop fromStop = trip.stop.Find(s => s.stop_id == fromStopId);

                    // Find to Stop by stopID
                    GTFS.Stop toStop = trip.stop.Find(s => s.stop_id == toStopId);

                    // No Stop yet, try finding stop by stop Name
                    if (fromStop == null) fromStop = trip.stop.Find(s => s.stop_name == fromStopName);

                    // No Stop yet, try finding stop by stop Name
                    if (toStop == null) toStop = trip.stop.Find(s => s.stop_name == toStopName);

                    // Add Active Vehicle information for this trip.
                    if (trip.vehicle != null)
                    {
                        if (!vehicleDict.ContainsKey(trip.trip_name))
                        { // If destination does not exisit in dictionary create one
                            vehicleDict.Add(trip.trip_name, trip.vehicle);
                        }
                        else
                        { // updated it
                            vehicleDict[trip.trip_name] = trip.vehicle;
                        }
                        
                    }
                    
                    
                    if ((fromStop != null) && (toStopId == null || toStop != null)) 
                    { // Find the right stop to track
                        Tuple<bool, DateTime> pTime =  MBTADataHelper.GetPreditionDateTime(fromStop);
                        // only add prediction time that is valid.
                        if (pTime != null)
                        {   
                            // If destination does not exisit in dictionary create one
                            if (!routeToPredictionsDict.ContainsKey(destination)) 
                                routeToPredictionsDict.Add(destination, new List<Tuple<bool, DateTime>>());

                            // Add Prediction to destination in dictionary
                            routeToPredictionsDict[destination].Add(pTime);
                        }
                    }
                }

                foreach (var entry in routeToPredictionsDict)
                {
                    List<Tuple<bool, DateTime>> predictionTime = entry.Value;

                    // Save all the stops predictions found for this route on the specific direction
                    if (predictionTime.Count > 0)
                    {
                        string destinationLabel = GetDestinationLabel(predictions.mode_name, direction.direction_name, entry.Key);
                        // Sort Prediction Time to be soonest to latest
                        predictionTime.Sort(
                            delegate(Tuple<bool, DateTime> p1, Tuple<bool, DateTime> p2) {return p1.Item2.CompareTo(p2.Item2);});
                     
                        predictionList.Add(new PredictionDataModel(destinationLabel, direction.direction_id, direction.direction_name, predictionTime, vehicleDict));
                    }
                }
            }

            if (predictionList.Count > 0 || alerts.Count > 0)
            { // If we found predictions, create a route data model for the view.

                result.Add(new
                    RouteDataModel(predictions.route_id, predictions.route_name, predictions.mode_name, predictionList, alerts) 
                    { StopName = fromStopName, StopId = fromStopId, ToStopId = toStopId, DirectionId = directionId });
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="predictions"></param>
        /// <returns></returns>
        internal static List<RouteDataModel> BuildRouteDataModel(PredictionByStop predictions, AlertHeaders alerts = null)
        {
            List<RouteDataModel> result = new List<RouteDataModel>();
            List<AlertDataModel> alertsDM = null;
            Dictionary<string, Vehicle> vehicleDict = new Dictionary<string, Vehicle>(); // List of active vehicles on the route. 

            if (alerts == null)
            {
                if (predictions.alert_headers != null && predictions.alert_headers.Count > 0)
                {
                    alertsDM = new List<AlertDataModel>();
                    foreach (var alert in predictions.alert_headers)
                    {
                        alertsDM.Add(new AlertDataModel(alert));
                    }
                    // Reverse the order of alert headers. To put newer alerts at the begining of the list.
                    alertsDM.Reverse();
                }
            }
            else
            {
                alertsDM = new List<AlertDataModel>();
                foreach (var alert in alerts.alert_headers)
                {
                    alertsDM.Add(new AlertDataModel(alert));
                }
                // Reverse the order of alert headers. To put newer alerts at the begining of the list.
                alertsDM.Reverse();
            }
            foreach (var mode in predictions.mode)
            {
                List<PredictionDataModel> predictionList = null;
                foreach (var route in mode.route)
                {
                    if (!result.Exists(r => r.RouteName == route.route_name))
                    {
                        predictionList = new List<PredictionDataModel>();
                        var rdm = new RouteDataModel(route.route_id, route.route_name, mode.mode_name, predictionList, alertsDM);
                        rdm.StopId = predictions.stop_id;
                        rdm.StopName = predictions.stop_name;
                        result.Add(rdm);
                    }

                    foreach (var direction in route.direction)
                    {
                        List<Tuple<bool, DateTime>> preditionTime = new List<Tuple<bool, DateTime>>();
                        string destination = "";
                        // Sort trips by schedule arrival time
                        direction.trip.Sort(
                            delegate(Trip x, Trip y)
                            {
                                if (x.sch_arr_dt == null && y.sch_arr_dt == null) return 0;
                                else if (x.sch_arr_dt == null) return -1;
                                else if (y.sch_arr_dt == null) return 1;

                                return x.sch_arr_dt.CompareTo(y.sch_arr_dt);
                            });

                        foreach (var trip in direction.trip)
                        {
                            if (trip.trip_headsign != "" && destination == "") destination = GetDestination(trip, direction, mode.mode_name);
                            preditionTime.Add(MBTADataHelper.GetPreditionDateTime(trip));

                            // Add vechicle data for each trip
                            if (trip.vehicle != null) vehicleDict.Add(trip.trip_name, trip.vehicle);
                        }
                        string destinationLabel = GetDestinationLabel(mode.mode_name, direction.direction_name, destination);
 
                        predictionList.Add(new PredictionDataModel(destinationLabel, direction.direction_id, direction.direction_name, preditionTime, vehicleDict));
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// This query will return a list of the nearest stops from a particular location. Up to 15 are returned, within a 1-mile radius
        /// </summary>
        /// <param name="lat">GPS Latitude as a string</param>
        /// <param name="lon">GPS Logitude as a string</param>
        /// <returns>List of Stop Ids</returns>
        internal static async Task<IEnumerable<string>> GetStopsByLocation(string lat = null, string lon = null)
        {
            List<string> result = new List<string>();
            // Redmond GPS Location!
            //      lat = 47.6785619
            //      lon = -122.1311156
            // Cambridge, MA
#if false
            try
            {
                if (lat == null || lon == null)
                {
                    // Get cancellation token
                    Geolocator geolocator = new Geolocator();

                    // Carry out the operation
                    Geoposition pos = await geolocator.GetGeopositionAsync();

                    lat = pos.Coordinate.Point.Position.Latitude.ToString();
                    lon = pos.Coordinate.Point.Position.Longitude.ToString();
                    // ScenarioOutput_Accuracy.Text = pos.Coordinate.Accuracy.ToString();
                    // ScenarioOutput_Source.Text = pos.Coordinate.PositionSource.ToString();
                    // force a release
                    geolocator = null;
                }
                if (lat == "" || lon == "")
                {
                    _status = eServiceStatus.NoLocationServices;
                    return result;
                }
                StopByLocation localStops = await MBTAManager.GetStopsByLocation(lat, lon);
                if (localStops.stop == null) return result;

                foreach (var s in localStops.stop)
                {
                    string stopId = s.parent_station == "" ? s.stop_id : s.parent_station;
                    if (!result.Contains(stopId)) result.Add(stopId);
                }

                _status = eServiceStatus.Ok;

                return result;
            }
#region Exception Handling
            catch (Exception e)
            {
                _status = eServiceStatus.NoLocationServices;
                string Signiture = "DataManager.GetStopsByLocation()";
                Debug.WriteLine("{0}:{1}", Signiture, Log.FormatExceptionMsg(e));
                Log.WriteLine(Signiture, Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }

#endregion
#endif 
            return result;
        }
        internal static AlertHeaders GetAlertHeaders(Alerts alerts)
        {
            if (alerts == null) return null;
            AlertHeaders headers = new AlertHeaders();

            foreach (var item in alerts.alerts)
            {
                AlertHeader ah = new AlertHeader();
                ah.alert_id = item.alert_id.ToString();
                ah.effect_name = item.effect_name;
                ah.header_text = item.header_text;
                headers.alert_headers.Add(ah);
            }

            return headers;
        }
#endregion
#region Unused Methods
#if false
        public enum eSubwayLines { eRED = 0, eORANGE, eBLUE };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeName"></param>
        /// <returns></returns>
        public async Task<Route> GetRouteByName(string routeName)
        {
            Route result = new Route();

            if (routeName == null || routeName == "") return result;

            // Lookup all routes 
            // findname 
            Routes allRoutes = await MBTAManager.GetRoutes();

            Predicate<Route> match1 = i => i.LongName == routeName;
            Predicate<Route> match2 = i => i.LongName == routeName;

            //Route route = allRoutes.mode.Find(match1);

            ///if (route == null) route = allRoutes.Find(match2);

            //if (route.ShortName == routeName || route.LongName == routeName) result = route;

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stopId"></param>
        /// <param name="withShapes"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Route>> GetRoutesByStopId(string stopId, bool withShapes = false)
        {
            if (stopId == null || stopId == "") return null;

            return MBTADataHelper.BuildRouteList(await MBTAManager.GetRouteByStopId(stopId));
        }
       

        public static async Task<Dictionary<String, List<Trip>>> GetScheduleByRouteId(string routeId)
        {
            Dictionary<String, List<Trip>> result = new Dictionary<string, List<Trip>>();
            return result;
        }
        public static async Task<Dictionary<string, List<GTFS.Stop>>> GetPredictionsByRouteId(string route_id, string stop_id = null, string direction_id = null)
        {
            Dictionary<string, List<GTFS.Stop>> result = new Dictionary<string, List<GTFS.Stop>>();
            if (route_id == null || route_id == "") return result;

            // Get prediction data from Web API
            PredictionByRoute prediction = await MBTAManager.GetPredictionByRoute(route_id);

            // Need to find all of the trip objects and convert them into Predictions.
            // Prediction->Direction->Trip->GTFS.Stop
            if (prediction == null || prediction.direction == null) return result;

            try
            {
                foreach (var d in prediction.direction)
                {
                    // Get the trip for the specific route in a specific direction (e.g. "Braintree", "Ashmont", "Alewife")
                    if (d.trip != null)
                    {
                        string destination = d.direction_name;
                        foreach (var t in d.trip)
                        {
                            // Now have a trip object with predictions 
                            if (t.stop != null)
                            {
                                // Check no predictions list. 
                                if (!result.ContainsKey(destination))
                                {
                                    result.Add(destination, t.stop);
                                    continue;
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("{0}:{1}", "DataManager.GetPredictionsByRouteId", Log.FormatExceptionMsg(e));
                Log.WriteLine("DataManager.GetPredictionsByRouteId", Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif

            }
            return result;
        }

        /// <summary>
        /// This Queary returns a dictionary of destinations and trips.
        /// </summary>
        /// <param name="stop_id"></param>
        /// <param name="route_id"></param>
        /// <returns>Dictionary Key = Destination, Value = List of Trips </returns>
        public static async Task<Dictionary<String, List<String>>> GetPredictionDestinationsByStopId(string stop_id, string route_id = null, string direction_id = null)
        {
            Dictionary<String, List<String>> result = new Dictionary<String, List<String>>();
            if (stop_id == null || stop_id == "") return result;
            
            try
            {
                // Key = routeId, Value = list of trips
                Dictionary<String, List<Trip>> predicationTrips = await GetPredictionTripsByStopId(stop_id, route_id);

                foreach (var routeId in predicationTrips)
                {
                    foreach (var trip in routeId.Value)
                    {
                        if (!result.ContainsKey(trip.Destination)) result.Add(trip.Destination, new List<string>());
                        result[trip.Destination].Add(trip.Prediction);
                    }
                }

            }
#region Exception Handling
            catch (Exception e)
            {
                Debug.WriteLine("{0}:{1}", "DataManager.GetDestinationPredictionByStopId", Log.FormatExceptionMsg(e));
                Log.WriteLine("DataManager.GetDestinationPredictionByStopId", Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }

#endregion
            return result;
        }
        /// <summary>
        /// Gets a list of Trips grouped by Route Id
        /// </summary>
        /// <param name="stopId">Stop Id as string</param>
        /// <param name="routeId = null">Route Id as string</param>
        /// <param name="directionId = null">Direction Id as string</param>
        /// <returns>
        /// Dictionary (Key = Route Id, Value = List of Trips)  
        /// </returns>
        public static async Task<Dictionary<String, List<Trip>>> GetPredictionTripsByStopId(string stopId, string routeId = null, string directionId = null)
        {
            // Key = routeId, Value = list of trips
            Dictionary<String, List<Trip>> result = new Dictionary<String, List<Trip>>();

            // Dictionary<String, List<Trip>> tempResult = new Dictionary<string,List<Trip>>();;
            
            if (stopId == null || stopId == "") return result;

            // Get prediction data from Web API
            PredictionByStop prediction = await MBTAManager.GetPredictionsScheduleByStop(stopId, routeId, directionId);
            //ScheduleByStop schedule = await MBTAManager.GetSchedByStop(stopId);
         

            // Need to find all of the trip objects and convert them into Predictions.
            // Prediction->Mode->Route->Direction->Trip
            if (prediction == null || prediction.mode == null) return result;

            try
            {
                string stopName = prediction.stop_name;
                List<Mode> modes = prediction.mode;

                foreach (var mode in modes)
                {
                    result = BuildTrips(mode, stopName, stopId, prediction.server_time.server_dt, routeId);
                }
            }
#region Exception Handling
            catch (Exception e)
            {
                Debug.WriteLine("{0}:{1}", "DataManager.GetPredictionsByStopId", Log.FormatExceptionMsg(e));
                Log.WriteLine("DataManager.GetPredictionsByStopId", Log.FormatExceptionMsg(e));
#if DEBUG
                Debugger.Break();
#endif
            }
            
#endregion
#if DEBUG
            foreach (var route in result)
            {
                Debug.WriteLine("\tRoute Id {1}, Stop Id = {0}", stopId, route.Key);
                foreach (var t in route.Value)
                {
                    Debug.WriteLine("\t\t{0}", t.ToString());
                }
            }
#endif
            // Key = routeId, Value = list of Trips)
            return result;
        }
        /// <summary>
        /// Given a stop id, returns a list of strings represent route ids
        /// </summary>
        /// <param name="stopId"></param>
        /// <returns></returns>
        internal static async Task<IEnumerable<string>> GetRouteIdsByStopId(string stopId)
        {
            List<string> result = new List<string>();

            if (stopId == null || stopId == "") return result;

            Routes routes = await MBTAManager.GetRouteByStopId(stopId);

            foreach (var m in routes.mode)
            {
                foreach (var r in m.route)
                {
                    result.Add(r.route_id);
                }
            }

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="stopName"></param>
        /// <param name="stopId"></param>
        /// <param name="serverTime"></param>
        /// <param name="routeId"></param>
        /// <returns>Dictionary Key = RouteId, Value = List of Trips</returns>
        internal static Dictionary<String, List<Trip>> BuildTrips(Mode mode, string stopName, string stopId, string serverTime, string routeId = null)
        {
            Dictionary<String, List<Trip>> result = new Dictionary<string, List<Trip>>();
            // Dictionary<String, List<String>> result = new Dictionary<String, List<String>>();

            if (mode == null) return new Dictionary<String, List<Trip>>();

            // Get a transit mode object for stop (e.g. "Subway", "Bus", "Commuter Rail")
            if (mode.route != null)
            {
                foreach (var route in mode.route) // Get a route object from stop
                {
                    string routeName = route.route_name; // e.g. Red Line, 64

                    // if route id is included only return predictions with matching route id
                    if (routeId == null || routeId == route.route_id)
                    {
                        // if (!result.ContainsKey(routeName)) result.Add(routeName, new Dictionary<string,List<Prediction>>());

                        // Get the direction for the route (e.g "Southbound", "Northbound", "Inbound", "Outbound")
                        if (route.direction != null)
                        {                           
                            foreach (var direction in route.direction)
                            {
                                string directionName = direction.direction_name;

                                // Get the trip for the specific route in a specific direction (e.g. "Braintree", "Ashmont", "Alewife")
                                if (direction.trip != null)
                                {
                                    foreach (var jsontrip in direction.trip)
                                    {
                                        // Now have a trip object with predictions 
                                        int seconds;
                                        if (int.TryParse(jsontrip.pre_away, out seconds))
                                        {
                                            Trip trip = new Trip(jsontrip, direction, route, mode, stopName, stopId, serverTime);

                                            // Save the destination.
                                            trip.Destination = GetDestination(jsontrip, direction, mode);
                                            /*
                                             * Build Result
                                             */
                                            string key = route.route_id + "." + direction.direction_id;

                                            // Check no predictions list. 
                                            if (!result.ContainsKey(key))
                                            {
                                                result.Add(key, new List<Trip>());
                                                result[key].Add(trip);
                                                continue;
                                            }
                                            int lstCnt = result[key].Count;

                                            // Keep a sorted list by the predicted away time in sec
                                            for (int i = 0; i < lstCnt; i++)
                                            {
                                                var existingPrediction = result[key][i];
                                                if (trip.PredictedSeconds < existingPrediction.PredictedSeconds)
                                                {
                                                    // Insert front of current predictions
                                                    result[key].Insert(i, trip);
                                                    break;
                                                }
                                                else if (i == lstCnt - 1)
                                                {
                                                    // add to end of the list
                                                    result[key].Insert(lstCnt, trip);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
#endif
#endregion
    }
}
