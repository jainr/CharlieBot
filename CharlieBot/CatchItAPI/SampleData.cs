using System;
using System.Collections.Generic;
using System.Text;
//using Windows.UI.Xaml.Controls;
using CatchIt.Data;
using CatchIt.GTFS;

namespace CatchIt
{
#if SAMPLEDATA
    public static class SampleData
    {
        public static Alert AlertDetail()
        {
            Alert alert = new Alert();

            alert.alert_id = 0;
            // alert.affected_services = "affected_services";
            alert.alert_lifecycle = "alert_lifecycle";
            alert.banner_text = "banner_text";
            alert.description_text = "Sample Text ";
            alert.cause = "cause";
            alert.cause_name = "cause_name";
            alert.created_dt = "created_dt";
            alert.description_text = "description_text ";
            alert.effect_name = "effect_name";
            alert.effect = "effect";
            alert.effect_name = "effect_name";
            //alert.effect_periods = "effect_period";
            alert.header_text = "header_text";
            alert.last_modified_dt = "last_modfied_dt";
            alert.service_effect_text = "servcice_effect_text";
            alert.severity = "severity";
            alert.short_header_text = "short_header_text";
            alert.timeframe_text = "timeframe_text";
 
            for (int i = 0; i < 10; i++)
            {
                alert.description_text += alert.description_text;
            }
            return alert;
        }

        public static List<StopDataModel> StopData()
        {
            List<StopDataModel> result = new List<StopDataModel>();


            List<Tuple<bool, DateTime>> predictions1 = new List<Tuple<bool, DateTime>>()
            {
                new Tuple<bool, DateTime>(true, DateTime.Now.AddSeconds(120)),
                new Tuple<bool, DateTime>(true, DateTime.Now.AddSeconds(180)),
                new Tuple<bool, DateTime>(true, DateTime.Now.AddSeconds(240))
            };
            List<Tuple<bool, DateTime>> predictions2 = new List<Tuple<bool, DateTime>>()
            {
                new Tuple<bool, DateTime>(false, DateTime.Now.AddSeconds(10)),
                new Tuple<bool, DateTime>(false, DateTime.Now.AddSeconds(180)),
                new Tuple<bool, DateTime>(false, DateTime.Now.AddSeconds(240))
            };
            List<Tuple<bool, DateTime>> predictions3 = new List<Tuple<bool, DateTime>>()
            {
                new Tuple<bool, DateTime>(true, DateTime.Now.AddSeconds(60)),
                new Tuple<bool, DateTime>(true, DateTime.Now.AddSeconds(180)),
                new Tuple<bool, DateTime>(true, DateTime.Now.AddSeconds(240))
            };

            AlertDataModel avd1 = new AlertDataModel() { Text = "Train Delayed due to signal problems" };
            AlertDataModel avd2 = new AlertDataModel() { Text = "Subway Delayed due to signal problems" };

            StopDataModel svd1 = new StopDataModel("North Station", "place-north");
            StopDataModel svd2 = new StopDataModel("Park", "");
            StopDataModel svd3 = new StopDataModel("Aquarium", "");
            StopDataModel svd4 = new StopDataModel("Haymarket", "");



            RouteDataModel rvd1 = new RouteDataModel("", "Light Rail", "Subway");
            rvd1.Alerts.Add(avd1);
            rvd1.Alerts.Add(avd2);

            RouteDataModel rvd2 = new RouteDataModel("", "Fitchburg/Acton", "Commuter Rail");
            RouteDataModel rvd3 = new RouteDataModel("", "Lowell", "Commuter Rail");
            RouteDataModel rvd4 = new RouteDataModel("", "Haverhill", "Commuter Rail");
            RouteDataModel rvd5 = new RouteDataModel("", "Orange Line", "Subway");
            RouteDataModel rvd6 = new RouteDataModel("", "Bus Route 77", "Bus");
            RouteDataModel rvd7 = new RouteDataModel("", "Bus Route 100", "Bus");
            RouteDataModel rvd8 = new RouteDataModel("", "Bus Route 112", "Bus");
            RouteDataModel rvd9 = new RouteDataModel("", "Bus Route 113", "Bus");

            PredictionDataModel pvd1 = new PredictionDataModel("Next Train to Fitchburg", "Outbound", "1", predictions1);
            PredictionDataModel pvd2 = new PredictionDataModel("Next Train to North Station", "Inbound", "0", predictions2);
            PredictionDataModel pvd3 = new PredictionDataModel("Next Bus Outbound", "Outbound", "1", predictions3);
            PredictionDataModel pvd4 = new PredictionDataModel("Next Bus Inbound", "Inbound", "0", predictions1);
            PredictionDataModel pvd5 = new PredictionDataModel("Next Train Outbound", "Outbound", "1", predictions3);
            PredictionDataModel pvd6 = new PredictionDataModel("Next Train Inbound", "Inbound", "0", predictions1);

            rvd1.Predictions.Add(pvd5);
            rvd1.Predictions.Add(pvd6);

            rvd2.Predictions.Add(pvd1);
            rvd2.Predictions.Add(pvd2);

            rvd3.Predictions.Add(pvd1);
            rvd3.Predictions.Add(pvd2);
            rvd4.Predictions.Add(pvd1);
            rvd4.Predictions.Add(pvd2);
            rvd5.Predictions.Add(pvd5);
            rvd5.Predictions.Add(pvd6);
            rvd6.Predictions.Add(pvd3);
            rvd6.Predictions.Add(pvd4);
            rvd7.Predictions.Add(pvd3);
            rvd7.Predictions.Add(pvd4);
            rvd8.Predictions.Add(pvd3);
            rvd8.Predictions.Add(pvd4);
            rvd9.Predictions.Add(pvd3);
            rvd9.Predictions.Add(pvd4);

            svd1.Routes.Add(rvd1);
            svd1.Routes.Add(rvd2);
            svd1.Routes.Add(rvd3);
            svd1.Routes.Add(rvd4);
            svd1.Routes.Add(rvd5);
            svd1.Routes.Add(rvd6);
            svd1.Routes.Add(rvd7);
            svd1.Routes.Add(rvd8);
            svd1.Routes.Add(rvd9);


            result.Add(svd1);
            result.Add(svd2);
            result.Add(svd3);
            result.Add(svd4);

            return result;
        }
        public static List<FavoriteDataModel> FavoriteData()
        {
            List<FavoriteDataModel> result = new List<FavoriteDataModel>();

            List<Tuple<bool, DateTime>> predictions1 = new List<Tuple<bool, DateTime>>()
            {
                new Tuple<bool, DateTime>(true, DateTime.Now.AddSeconds(120)),
                new Tuple<bool, DateTime>(false, DateTime.Now.AddSeconds(180)),
                new Tuple<bool, DateTime>(true, DateTime.Now.AddSeconds(240))
            };
            List<Tuple<bool, DateTime>> predictions2 = new List<Tuple<bool, DateTime>>()
            {
                new Tuple<bool, DateTime>(false, DateTime.Now.AddSeconds(10)),
                new Tuple<bool, DateTime>(false, DateTime.Now.AddSeconds(180)),
                new Tuple<bool, DateTime>(false, DateTime.Now.AddSeconds(240))
            };
            List<Tuple<bool, DateTime>> predictions3 = new List<Tuple<bool, DateTime>>()
            {
                new Tuple<bool, DateTime>(false, DateTime.Now.AddSeconds(60)),
                new Tuple<bool, DateTime>(false, DateTime.Now.AddSeconds(180)),
                new Tuple<bool, DateTime>(false, DateTime.Now.AddSeconds(240))
            };

            AlertDataModel avd1 = new AlertDataModel() { AlertId = "01", EffectName = "Red Line", Text = "Train Delayed due to signal problems" };
            AlertDataModel avd2 = new AlertDataModel() { AlertId = "02", EffectName = "Red Line", Text = "Subway Delayed due to mechanical problems" };

            RouteDataModel rvd1 = new RouteDataModel("Red", "Red Line", "Subway") { StopName = "MIT/Kendall" };
            RouteDataModel rvd2 = new RouteDataModel("routeid", "Fitchburg/Acton", "Commuter Rail") { StopName = "South Acton" };
            RouteDataModel rvd3 = new RouteDataModel("1", "1", "Bus") { StopName = "Harvard" };

            PredictionDataModel pvd1 = new PredictionDataModel("Next Train to Fitchburg", "Outbound", "1", predictions1);
            PredictionDataModel pvd2 = new PredictionDataModel("Next Train to North Station", "Inbound", "0", predictions2);
            PredictionDataModel pvd3 = new PredictionDataModel("Next Bus Outbound", "Outbound", "1", predictions3);
            PredictionDataModel pvd4 = new PredictionDataModel("Next Bus Inbound", "Inbound", "0", predictions1);
            PredictionDataModel pvd5 = new PredictionDataModel("Next Train Outbound", "Outbound", "1", predictions3);
            PredictionDataModel pvd6 = new PredictionDataModel("Next Train Inbound", "Inbound", "0", predictions1);

            FavoriteDataModel fvd1 = new FavoriteDataModel("MIT/Kendall", "stop_id", "Alewife", "1");
            FavoriteDataModel fvd2 = new FavoriteDataModel("Porter", "stop_id", "Fitchburg", "0");
            FavoriteDataModel fvd3 = new FavoriteDataModel("North Station", "stop_id", "Outbound", "0");
            FavoriteDataModel fvd4 = new FavoriteDataModel("Harvard", "stop_id", "Outbound", "0");


            rvd1.Predictions.Add(pvd5);
            rvd1.Predictions.Add(pvd6);
            rvd1.Alerts.Add(avd1);
            rvd1.Alerts.Add(avd2);

            rvd2.Predictions.Add(pvd1);
            rvd2.Predictions.Add(pvd2);

            rvd3.Predictions.Add(pvd3);
            rvd3.Predictions.Add(pvd4);

            fvd1.Routes.Add(rvd1);
            fvd2.Routes.Add(rvd2);
            fvd3.Routes.Add(rvd3);

            result.Add(fvd1);
            result.Add(fvd2);
            result.Add(fvd3);

            return result;
        }
    }
#endif
}
