/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatchIt.GTFS;

namespace CatchIt.Data
{
    public class Stop
    {
        private string _name;

        // Properties
        public string Id { get; private set; }
        public string Code { get; private set; }
        public string Name { get { return ParentStationName == "" ? _name : ParentStationName; }}
        public string Desc { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public IEnumerable<TransitType> TransitType { get; set; }
        public string ZoneId { get; private set; }
        public string Url { get; private set; }
        public string ParentStation { get; private set; }
        public string ParentStationName { get; set; }
        public string LocationType { get; private set; }
        public Stop()
        { 
        }

        public Stop(string id, string code, string name, string desc, double latitude, double longitude, 
            string zoneId, string url, string parentstation, string locationtype) 
        {
            _name = name;
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

        public override string ToString()
        {
            //"stop_id","stop_code","stop_name","stop_desc","stop_lat","stop_lon","zone_id","stop_url","location_type","parent_station"
            return "<stop_id: " + Id + ", stop_code: " + Code + ", stop_name: " + Name + " + stop_desc: " + Desc + ", stop_lat: " +
                Latitude + ", stop_long: " + Longitude + ", zone_id: " + ZoneId + ", stop_url: " + Url + ", parent_station: " + 
                ParentStation + ", location_type: " + LocationType + ">";
        }
    }
}
