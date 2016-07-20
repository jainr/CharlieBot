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

namespace CatchIt.GTFS
{
    public enum TransitType { Bus = 3, CommuterRail = 2, Subway = 1, All = 8, StreetCar = 0, Ferry = 4 }

    public class Mode
    {
        public string route_type { get; set; }
        public string mode_name { get; set; }
        public List<Route> route { get; set; }
        public new string ToString()
        {
            return string.Format("route_type({0}), mode_name({1}), route({2})", 
                route_type, mode_name, route == null ?"null":route.Count.ToString());
        }
    }
    public class Direction
    {
        public string direction_id { get; set; }
        public string direction_name { get; set; }
        public List<Stop> stop { get; set; } // could be null
        public List<Trip> trip { get; set; } // could be null
        public new string ToString()
        {
            return string.Format("direction_id({0}), direction_name({1}), stop({2}), trip({3})", 
                direction_id, direction_name, stop == null?"null":stop.Count.ToString(), trip== null ?"null":trip.Count.ToString());
        }

    }
    public class Stop
    {
        public string stop_id { get; set; }
        public string stop_name { get; set; }
        public string parent_station { get; set; }
        public string parent_station_name { get; set; }
        public string stop_lat { get; set; }
        public string stop_lon { get; set; }
        public string distance { get; set; }
        public string stop_order { get; set; }
        public string stop_sequence { get; set; }
        public string sch_arr_dt { get; set; }
        public string sch_dep_dt { get; set; }
        public string pre_dt { get; set; }
        public string pre_away { get; set; }
        public new string ToString()
        {
            return string.Format("stop_name({0}), stop_id({1}), parent_station({2}), parent_station_name({3}), stop_lat({4}), stop_lon({5}, distance({6}), stop_order({7}), stop_sequence({8}), sch_arr_dt({9}), sch_dep_dt({10}), pre_dt({11}), pre_away({12})",
                stop_name, stop_id, parent_station, parent_station_name, stop_lat, stop_lon, distance, stop_order, stop_sequence, sch_arr_dt, sch_dep_dt, pre_dt, pre_away);
        }

    }
    public class Route
    {
        public string route_id { get; set; }
        public string route_name { get; set; }
        public string route_hide { get; set; }
        public List<Direction> direction { get; set; }
        public new string ToString()
        {
            return string.Format("route_id({0}), route_name({1}), route_hide({2}), direction({3})", 
                route_id, route_name, route_hide, direction == null?"null":direction.Count.ToString());
        }
    }
    public class Trip
    {
        public string trip_id { get; set; }
        public string trip_name { get; set; }
        public string trip_headsign { get; set; }
        public string sch_arr_dt { get; set; }
        public string sch_dep_dt { get; set; }
        public string pre_dt { get; set; }
        public string pre_away { get; set; }
        public Vehicle vehicle { get; set; }
        public List<Stop> stop { get; set; }
        public new string ToString()
        {
            return string.Format("trip_id({0}), trip_name({1}), trip_headsign({2}), sch_arr_dt({3}), sch_dep_dt({4}), pre_dt({5}), pre_away({6}), vehicle({7}), stop({8})", 
                trip_id, trip_name, trip_headsign, sch_arr_dt, sch_dep_dt, pre_dt, pre_away, vehicle == null?"null":vehicle.vehicle_id, stop == null?"null":stop.Count.ToString());
        }
    }
    public class Vehicle
    {
        public string vehicle_id { get; set; }
        public string vehicle_lat { get; set; }
        public string vehicle_lon { get; set; }
        public string vehicle_bearing { get; set; }
        public string vehicle_speed { get; set; }
        public string vehicle_timestamp { get; set; }
        public new string ToString()
        {
            return string.Format("vehicle_id({0}), vehicle_lat({1}), vehicle_lon({2}), vehicle_bearing({3}), vehicle_timestamp({4})", 
                vehicle_id, vehicle_lat, vehicle_lon, vehicle_bearing, vehicle_timestamp);
        }
    }
    public class StopByRoute
    {
        public List<Direction> direction { get; set; }
        public new string ToString()
        {
            return string.Format("direction({0})", direction == null?"null":direction.Count.ToString());
        }
    }
    public class StopByLocation
    {
        public List<Stop> stop { get; set; }
        public new string ToString()
        {
            return string.Format("stop({0})", stop == null?"null":stop.Count.ToString());
        }
    }
    public class ScheduleByStop
    {
        public string stop_id { get; set; }
        public string stop_name { get; set; }
        public List<Mode> mode { get; set; }
        public ServerTime server_time { get; set; }
        public new string ToString()
        {
            return string.Format("stop_id({0}), stop_name({1}), mode({2})", 
                stop_id, stop_name, mode == null ?"null":mode.Count.ToString());
        }
    }
    public class ScheduleByRoute
    {
        public string route_id { get; set; }
        public string route_name { get; set; }
        public List<Direction> direction { get; set; }
        public new string ToString()
        {
            return string.Format("route_id({0}), route_name({1}), direction({2})", 
                route_id, route_name, direction == null?"null":direction.Count.ToString());
        }
    }
    public class RouteByStop
    {
        public string stop_id { get; set; }
        public string stop_name { get; set; }
        public List<Mode> mode { get; set; }
        public new string ToString()
        {
            return string.Format("stop_id({0}), stop_name({1}), mode({2})", 
                stop_id, stop_name, mode == null?"null":mode.Count.ToString());
        }
    }
    public class Routes
    {
        public List<Mode> mode { get; set; }
        public new string ToString()
        {
            return string.Format("mode({0})", mode == null?"null":mode.Count.ToString());
        }
    }
    public class AlertHeaders
    {
        public string route_id { get; set; }
        public string route_name { get; set; }
        public List<AlertHeader> alert_headers { get; set; }
        public AlertHeaders()
        { 
            alert_headers = new List<AlertHeader>();
        }
        public new string ToString()
        {
            return string.Format("route_id({0}), route_name({1}), alert_headers({2})",
                route_id, route_name, alert_headers == null ? "null" : alert_headers.Count.ToString());
        }
    }
    public class AlertHeader
    {
        public string alert_id { get; set; }
        public string header_text { get; set; }
        public string effect_name { get; set; }
        public new string ToString()
        {
            return string.Format("alert_id({0}), header_text({1}), effect_name({2})", alert_id, header_text, effect_name);
        }

    }
    public class PredictionByStop
    {
        public string stop_id { get; set; }
        public string stop_name { get; set; }
        public List<Mode> mode { get; set; }
        public List<AlertHeader> alert_headers { get; set; }
        public ServerTime server_time { get; set; }
        public new string ToString()
        {
            return string.Format("stop_id({0}), stop_name({1}), mode({2}), alert_headers({3})", 
                stop_id, stop_name, mode==null?"null":mode.Count.ToString(), alert_headers==null?"null":alert_headers.Count.ToString());
        }
    }
    public class PredictionByRoute
    {
        // The GTFS-compatible unique identifier for the route 
        public string route_id { get; set; } 
        public string route_name { get; set; }
        public string route_type { get; set; }
        public string mode_name { get; set; }
        public List<Direction> direction { get; set; }
        public List<AlertHeader> alert_headers { get; set; }
        public ServerTime server_time { get; set; }
        public new string ToString()
        {
            return string.Format("route_id({0}), route_name({1}), route_type({2}), mode_name({3}), direction({4}), alert_headers({5})", 
                route_id, route_name, route_type, mode_name, direction == null ?"null":direction.Count.ToString(), alert_headers == null?"null":alert_headers.Count.ToString());
        }
    }
    public class ServerTime
    {
        public string server_dt { get; set; }
    }
    public class Alerts
    {
        public List<Alert> alerts { get; set; }
        public string stop_id { get; set; }
        public string stop_name { get; set; }
        public string route_id { get; set; }
        public string route_name { get; set; }
    }

    public class Alert
    {
        public int alert_id { get; set; }
        public string effect_name { get; set; }
        public string effect { get; set; }
        public string cause_name { get; set; }
        public string cause { get; set; }
        public string header_text { get; set; }
        public string short_header_text { get; set; }
        public string description_text { get; set; }
        public string severity { get; set; }
        public string created_dt { get; set; }
        public string last_modified_dt { get; set; }
        public string service_effect_text { get; set; }
        public string timeframe_text { get; set; }
        public string alert_lifecycle { get; set; }
        public string banner_text { get; set; }
        public List<EffectPeriod> effect_periods { get; set; }
        public AffectedServices affected_services { get; set; }
        public new string ToString()
        {
            return string.Format("alert_id({0}), effect_name({1}), effect({2}), cause_name({3}), cause({4}), header_text({5}), short_header_text({6}), description_text({7}), severity({8}), created_dt({9}), last_modified_dt({10}), service_effect_text({11}), timeframe_text({12}), alert_lifecycle({13}), banner_text({14})",
                alert_id, effect_name, effect, cause_name, cause, header_text, short_header_text, description_text, severity, created_dt, last_modified_dt, service_effect_text, timeframe_text, alert_lifecycle, banner_text);
        }
    }

    public class EffectPeriod
    {
        public string effect_start { get; set; }
        public string effect_end { get; set; }
    }
    public class AffectedServices
    {
        public List<Service> services { get; set; }
        public List<Elevator> elevators { get; set; }
    }

    public class Service
    {
        public string route_type { get; set; }
        public string mode_name { get; set; }
        public string route_id { get; set; }
        public string route_name { get; set; }
        public string direction_id { get; set; }
        public string direction_name { get; set; }
        public string stop_id { get; set; }
        public string stop_name { get; set; }
    }

    public class Elevator
    {
        public string elev_id { get; set; }
        public string elev_name { get; set; }
        public string elev_type { get; set; }
        public List<Stop> stops { get; set; }
    }
}
