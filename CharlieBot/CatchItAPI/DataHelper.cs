/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace CatchIt.Data
{
    public enum eServiceStatus { Ok, NoDataNetwork, DataServiceUnavailable, Unknown, NoLocationServices, LocationServicesDisabled, NoNearByStops };
    public class JsonSettings
    {
        public static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.All
        };
    } 

    static public class MBTADataHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stops"></param>
        /// <returns></returns>
        static public List<Stop> BuildStopList(GTFS.StopByRoute stops)
        {
            List<Stop> results = new List<Stop>();
            if (stops.direction != null)
            {
                foreach (var d in stops.direction)
                {
                    foreach (var s in d.stop)
                    {
                        results.Add(ConvertStop(s));
                    }
                }
                return results;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static public Stop ConvertStop(GTFS.Stop s)
        {
            /*
             * public Stop(string id, string code, string name, string desc, double latitude, double longitude, 
                        string zoneId, string url, string parentstation, string locationtype) 
                    {
                        Name = name;
                        Id = id;
                        Code = code;
                        Desc = desc;
                        Latitude = latitude;
                        Longitude = longitude;
                        ZoneId = zoneId;
                        Url = url;
                        ParentStation = parentstation;
                        LocationType = locationtype;
                        TransitType = new List<TransitType>();
                    }

            */
            double lat;
            double lon;

            double.TryParse(s.stop_lat, out lat);
            double.TryParse(s.stop_lon, out lon);
            Stop stop = new Stop(s.stop_id, null, s.stop_name, s.stop_name, lat, lon, null, null, s.parent_station, null);
            stop.ParentStationName = s.parent_station_name;
            return stop;
        }
        public static GTFS.TransitType ConvertTransitType (string transitType)
        {
            GTFS.TransitType result = GTFS.TransitType.All;
            if (transitType == null || transitType == "") return result;

            switch (transitType)
            {
                case "Bus":
                    result = GTFS.TransitType.Bus;
                    break;
                case "Subway":
                    result = GTFS.TransitType.Subway;
                    break;
                case "Commuter Rail":
                    result = GTFS.TransitType.CommuterRail;
                    break;
                case "Street Car":
                    result = GTFS.TransitType.StreetCar;
                    break;
                case "Light Rail":
                    result = GTFS.TransitType.StreetCar;
                    break;
                default:
                    break;
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="routes"></param>
        /// <returns></returns>
        static public List<Route> BuildRouteList(GTFS.Routes routes)
        {
            string backgroundColor;
            string textColor;
            GTFS.TransitType transitType;

            List<Route> results = new List<Route>();
            foreach (var m in routes.mode)
            {
                switch (m.route_type)
                {
                    case "0":
                        backgroundColor = "427B1D";
                        textColor = "FFFFFF";
                        transitType = GTFS.TransitType.StreetCar;
                        break;
                    case "1":
                        backgroundColor = "E12D27";
                        textColor = "FFFFFF";
                        transitType = GTFS.TransitType.Subway;
                        break;
                    case "2":
                        backgroundColor = "8B118F";
                        textColor = "FFFFFF";
                        transitType = GTFS.TransitType.CommuterRail;
                        break;
                    case "3":
                        backgroundColor = "FFFF7C";
                        textColor = "000000";
                        transitType = GTFS.TransitType.Bus;
                        break;
                    case "4":
                        backgroundColor = "000000";
                        textColor = "FFFFFF";
                        transitType = GTFS.TransitType.Ferry;
                        break;
                    default:
                        backgroundColor = "000000";
                        textColor = "FFFFFF";
                        transitType = GTFS.TransitType.All;
                        break;
                }
                foreach (var r in m.route)
                {
                    switch (r.route_name)
                    {
                        case "Red Line":
                            backgroundColor = "E12D27";
                            break;
                        case "Blue Line":
                            backgroundColor = "2F5DA6";
                            break;
                        case "Orange Line":
                            backgroundColor = "E87200";
                            break;
                        default:
                            break;
                    }
                    // Add a route object to the list for each route
                    results.Add(new Route(r.route_id, "", r.route_name, r.route_name, r.route_name, "", backgroundColor, textColor, transitType, null));
                }
            }
            return results;
        }
        public static DateTime GetTimeFromEpochTime(string time)
        {
            DateTime result = DateTime.Now.ToLocalTime();
            int unixTime;

            if (int.TryParse(time, out unixTime))
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                result = epoch.AddSeconds(unixTime);
            }

            return result;
        }
        public static Tuple<bool, DateTime> GetPreditionDateTime(GTFS.Stop stop)
        {
            bool realtime = true;
            int seconds;
            DateTime arrivalTime = DateTime.Now;

            if (!int.TryParse(stop.pre_away, out seconds))
            {
                // Pre_away is invalid, return Schedule arrival.
                realtime = false;
                if (stop.sch_arr_dt != "")
                {
                    arrivalTime = GetTimeFromEpochTime(stop.sch_arr_dt).ToLocalTime();
                    TimeSpan delta = arrivalTime - DateTime.Now;
                 
                    if (delta.TotalMinutes < 0)
                    {
                        // Arrival time has already gone by, do not display this result.
                        return null;
                    }
                }
            }
            else
            {
                if (stop.pre_dt != "")
                {
                    arrivalTime = GetTimeFromEpochTime(stop.pre_dt).ToLocalTime();
                }
            }

            return new Tuple<bool, DateTime>(realtime, arrivalTime);

        }
        public static Tuple<bool, DateTime> GetPreditionDateTime(GTFS.Trip trip)
        {
            bool realtime = true;
            int seconds;
            DateTime arrivalTime = DateTime.Now;

            if (!int.TryParse(trip.pre_away, out seconds))
            {
                // Pre_away is invalid, return Schedule arrival.
                realtime = false;
                if (trip.sch_arr_dt != "")
                {
                    arrivalTime = GetTimeFromEpochTime(trip.sch_arr_dt).ToLocalTime();
                    TimeSpan delta = arrivalTime - DateTime.Now;
                    if (delta.TotalSeconds < 0)
                    {
                        seconds = 60;
                    }
                    else
                        seconds = (int)delta.TotalSeconds;
                }
            }
            else
            {
                if (trip.pre_dt != "")
                {
                    arrivalTime = GetTimeFromEpochTime(trip.pre_dt).ToLocalTime();
                }
            }

            return new Tuple<bool, DateTime>(realtime, arrivalTime);
        }
/*
        public static Tuple<string, string> GetPredition(Trip trip)
        {
            int nMinutes;
            bool realtime = true;
            string Minutes;
            
            Tuple<bool, DateTime> prediction = GetPreditionDateTime(trip);
            DateTime arrivalTime = prediction.Item2;

            nMinutes = (seconds > 60) ? (1 + (seconds / 60)) : 1;

            if (!realtime)
                Minutes = nMinutes.ToString() + " min*";
            else if (seconds == 0)
                Minutes = "BRD";
            else if (seconds < 60)
                Minutes = "ARR";
            else
                Minutes = nMinutes.ToString() + " min";

            return new Tuple<string, string>(Minutes, realtime ? arrivalTime.ToString("t", CultureInfo.CurrentCulture) : arrivalTime.ToString("t", CultureInfo.CurrentCulture) + "*");
        }
  */
    }
}
