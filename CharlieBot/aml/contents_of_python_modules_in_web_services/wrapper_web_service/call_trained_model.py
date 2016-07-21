'''
call_trained_model.py
2016 Mary Wahl, Paul Oka, Richin Jain, Min Qiu

This web service takes as input the route and stop numbers as well as the
arrival time predicted by the MBTA. It subtracts the current time from the
predicted arrival time to get the MBTA prediction for number of minutes
remaining until the next arrival. It then looks up the appropriate web service
credentials for the route/stop of interest, including URL and API key, which it
attempts to call. The output is a delta (number of minutes to add to the MBTA
predicted arrival time to make it more accurate) as well as a status code
(which indicates success of the child web service call) and a status message
(describing the reason for failure, if applicable.)

Used Python 2.7 in AML.
'''

import pandas as pd
import urllib2
import json
from pytz import timezone, utc
import ast
  
def azureml_main(df = None, webservice_df = None):
    ''' Get now in Boston time -- may need to examine this more closely for DST and so forth '''   
    localFormat = "%Y-%m-%d %H:%M:%S"
    utcmoment_unaware = pd.datetime.utcnow()
    utcmoment = utcmoment_unaware.replace(tzinfo=utc)
    localDatetime = utcmoment.astimezone(timezone('US/Eastern'))
    now_in_est = pd.to_datetime(localDatetime.strftime(localFormat))

    ''' Reformat input to what web services expect '''
    df['mbta_api_call_time'] = str(now_in_est)
    df['mbta_api_pred_time_left'] = ['%f' % (i.seconds / 60.) for i in pd.to_datetime(df['mbta_api_pred_arrival_time']) - now_in_est]
    df.drop('mbta_api_pred_arrival_time', 1, inplace=True)
    
    output = []
    for route_tag, stop_tag, mbta_api_call_time, mbta_api_pred_time_left in df.values:
        ''' Try to get a web service URL for this route and stop tag '''
        try:
            webservice_url = webservice_df.loc[(webservice_df['route_tag'] == route_tag) & (webservice_df['stop_tag'] == stop_tag), 'webservice_url'][0]
            api_key = webservice_df.loc[(webservice_df['route_tag'] == route_tag) & (webservice_df['stop_tag'] == stop_tag), 'api_key'][0]
        except Exception as e:
            print("Couldn't find a webservice URL/api key for route %s, stop %s\n%s" % (route_tag, stop_tag, e))
            output.append([None, '400', "Couldn't find a URL for this route/stop combination"])
            continue
            
        ''' Try to call this web service '''
        try:
            ''' Create and send the request '''
            data = {"Inputs": {"input1": {"ColumnNames": ["mbta_api_call_time", "mbta_api_pred_time_left"],
                                               "Values": [[mbta_api_call_time, mbta_api_pred_time_left]]}},
                    "GlobalParameters": {}}

            body = str.encode(json.dumps(data))
            headers = {'Content-Type': 'application/json', 'Authorization': ('Bearer '+ api_key)}
            req = urllib2.Request(webservice_url, body, headers)           
        except Exception as e:
            print("Couldn't successfully call the web service with URL %s\n%s" % (webservice_url, e))
            output.append([None, '401', "Error formulating the web service request"])
            continue
        
        ''' Try to read the response '''
        try:
            response = urllib2.urlopen(req)
            result = response.read()
            result = ast.literal_eval(result)
            delta = result['Results']['output1']['value']['Values'][0][0]
        except urllib2.HTTPError, error:
            print("The request failed with status code: " + str(error.code))
            print(error.info())
            print(json.loads(error.read())) 
            output.append([None, str(error.code), json.loads(error.read())])
            continue 
        
        ''' Successful call -- return result '''
        output.append([delta, '200', 'SUCCESS'])
    
    ''' Placeholder for actual output '''
    output_df = pd.DataFrame(output, columns=['delta', 'status_code', 'status_message'])
    return output_df
