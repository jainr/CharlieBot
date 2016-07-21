'''
single_routestop_api_tester.py
2016 Mary Wahl, Min Qiu, Paul Oka, Richin Jain

Calls an individual route-stop combination's trained model to determine delta,
the predicted difference between real arrival time and the MBTA prediction.
'''

import pandas as pd
import urllib
import json
from pytz import timezone, utc
import ast

def main():
	''' The only stop/route combo we have currently '''
	route_tag = '134'
	stop_tag = '9161'

	'''
	Webservice URL and API key for route 134, stop 9161

	In general this needs to be looked up, and it is possible to return an error 400 if
	no webservice URL and API key can be found for the requested route and stop.
	'''
	webservice_url = 'https://ussouthcentral.services.azureml.net/workspaces/19281ada00d9423fa52a17290f26de3e/services/de673107d0dd431884978d14163aee51/execute?api-version=2.0&details=true'
	headers = {'Content-Type': 'application/json', 'Authorization': 'Bearer FzxnMAzyhOCnQ8a6ZZLiCw+Ft8eaE3HW+4dPxCWtDbyZXG5s/h9vCLMHSGcCHOr5c1GqYjsVT5DPIDPrnnHk+w=='}

	''' Make a phony call time value (current Boston time) '''   
	localFormat = "%Y-%m-%d %H:%M:%S"
	utcmoment_unaware = pd.datetime.utcnow()
	utcmoment = utcmoment_unaware.replace(tzinfo=utc)
	localDatetime = utcmoment.astimezone(timezone('US/Eastern'))
	mbta_api_call_time = localDatetime.strftime(localFormat)

	'''
	In general the predicted time left to arrival would be calculated as a difference
	between the MBTA predicted arrival time and the current time, converted to a float
	in minutes. We'll just assume the current prediction is for 10 minutes from now.
	'''
	mbta_api_pred_time_left = 10.
	    
	''' Try to call this web service '''
	try:
	    ''' Create and send the request '''
	    data = {"Inputs": {"input1": {"ColumnNames": ["mbta_api_call_time", "mbta_api_pred_time_left"],
	                                       "Values": [[mbta_api_call_time, mbta_api_pred_time_left]]}},
	            "GlobalParameters": {}}
	    body = str.encode(json.dumps(data))
	    req = urllib.request.Request(webservice_url, body, headers)           
	except Exception as e:
	    print("Couldn't successfully call the web service with URL %s\n%s" % (webservice_url, e))
	    output = [None, '401', "Error formulating the web service request"]
	    return(output)

	''' Try to read the response '''
	try:
	    response = urllib.request.urlopen(req)
	    result = response.read().decode('utf-8')
	    result = ast.literal_eval(result)
	    delta = float(result['Results']['output1']['value']['Values'][0][0])
	    output = [delta, '200', 'SUCCESS']
	except urllib.request.HTTPError as error:
	    print("The request failed with status code: " + str(error.code))
	    print(error.info())
	    print(json.loads(error.read().decode('utf-8'))) 
	    output = [None, str(error.code), json.loads(error.read())]
	return(output)

if __name__ == '__main__':
	print(main())