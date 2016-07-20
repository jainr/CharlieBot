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
//using Windows.UI.Xaml.Media;

namespace CatchIt.Data
{
    public class SolidColorBrush
    {
        public int color;

    }

    public class Route
    {
        public string Id {get; private set;}
        public string AgencyId {get; private set;}
        public string ShortName {get; private set;}
        public string LongName {get; private set;}
        public string Desc {get; private set;}
        private string _backgroundColor;
        private string _textColor;
        private bool bInitialize;
        private SolidColorBrush _BackgroundColor;
        private SolidColorBrush _TextColor;
        public GTFS.TransitType TransitType {get; private set;}
        public String Url {get; private set;}

        public SolidColorBrush BackgroundColor 
        { 
            get 
            {
                if (!bInitialize)
                {
                    // Initialize brush when accessed
                    bInitialize = InitializeColors(_backgroundColor, _textColor);
                }

                return _BackgroundColor;
            } 
            private set
            {
                // Initialize brush when accessed
                _BackgroundColor = value;
            }
        }

        public SolidColorBrush TextColor 
        {
            get
            {
                if (!bInitialize)
                {
                    bInitialize = InitializeColors(_backgroundColor, _textColor);
                }

                return _TextColor;
            } 
            private set
            {
                _TextColor = value;
            }
        }

        public Route()
        {
            Id = "";
            AgencyId = "";
            ShortName = "";
            LongName = "";
            Desc = "";
            Url = "";
            TransitType = GTFS.TransitType.All;
            _backgroundColor = "";
            _textColor = "";
            bInitialize = false;

        }
        public Route(string id, string agencyId, string shortName, string longName, string desc,
    string url, string backgroundColor, string textColor, GTFS.TransitType routeType, object shapeDict)
        {
            Id = id;
            AgencyId = agencyId;
            ShortName = shortName;
            LongName = longName;
            Desc = desc;
            Url = url;
            //ShapeDict = shapeDict;
            TransitType = routeType;

            _backgroundColor = backgroundColor;
            _textColor = textColor;
            bInitialize = false;

            // Delay Initializing the Colors to make sure it is on the UI thread
            // this.InitializeColors(backgroundColor, textColor);
        }

        // given two strings representing the text and background colors of the route in hex,
        // assigns the fields of the route to those colors provided they are valid hex values,
        // otherwise defaults to a black background and white text
        public bool InitializeColors(string backgroundColor, string textColor)
        {
            bool retVal = false;
#if false
            BackgroundColor = new SolidColorBrush(Windows.UI.Colors.Black);
            TextColor = new SolidColorBrush(Windows.UI.Colors.White);

            try
            {
                if (backgroundColor.Length == 6)
                {
                    if(backgroundColor != "FFFF7C") 
                    {

                        BackgroundColor = new SolidColorBrush(
                            Windows.UI.Color.FromArgb(0xFF, byte.Parse(backgroundColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                                                        byte.Parse(backgroundColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                                                        byte.Parse(backgroundColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber)));
                    }
                }
            }
            catch (FormatException)
            {
                Debug.WriteLine("Background color not found.");
            }

            try
            {
                if (textColor.Length == 6)
                {
                    TextColor = new SolidColorBrush(
                        Windows.UI.Color.FromArgb(0xFF, byte.Parse(textColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                                                    byte.Parse(textColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                                                    byte.Parse(textColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber)));
                }
            }
            catch (FormatException)
            {
                Debug.WriteLine("Text color not found.");
            }

            if ((_BackgroundColor != null) && (_TextColor != null))
            {
                retVal = true;
            }
            else
            {
                retVal = false;
            }
#endif          
            return retVal;
        }

        public override sealed string ToString()
        {
            return "<route_id: " + Id + ", agency_id: " + AgencyId + ", route_short_name: " + ShortName + ", route_long_name: " + LongName + ", route_desc: " + Desc + ", route_type: " + /*TransitType +*/ ", stop_url: " + Url + ", route_text_color: " + TextColor + ">";
        }
    }
}
