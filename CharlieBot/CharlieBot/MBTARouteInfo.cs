using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlieBot
{

    [Serializable]
    public class MBTARouteInfo
    {
#pragma warning disable 1998
        public static IForm<MBTARouteInfo> MakeForm()
        {
            FormBuilder<MBTARouteInfo> _Order = new FormBuilder<MBTARouteInfo>();
            return _Order
                .Message("Welcome to the Charlie Bot!")
                .Field(nameof(MBTARouteInfo.StopName))
                .Field(nameof(MBTARouteInfo.mode))
                .Field(nameof(MBTARouteInfo.routeName))
                .AddRemainingFields()
                .Confirm("You want to know about {mode} from {StopName} for route: {routeName}? ")
                .OnCompletion(async (session, MBTARouteInfo) =>
                {
                    Debug.WriteLine("{0}", MBTARouteInfo);
                })
                .Build();
        }


        [Prompt("Which Stock Ticker are you interested in? {||}")]
        [Describe("Stock Ticker, example: MSFT")]
        public string StopName;

        [Prompt("Which MBTA service do you want to use?  {||}")]
        [Describe("MBTA service, example : Bus, train, commuter rail")]
        public string mode;

        [Prompt("Which route are you interested to? {||}")]
        [Describe("Route Name, example: 77, Red, Fitchburg")]
        public int routeName;
    };
}
