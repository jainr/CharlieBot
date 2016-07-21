'''
wrapper_api_tester.py
2016 Mary Wahl, Min Qiu, Paul Oka, Richin Jain

Calls the wrapper web service which handles selection of an appropriate
route-stop trained model.
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
	webservice_url = 'https://ussouthcentral.services.azureml.net/workspaces/19281ada00d9423fa52a17290f26de3e/services/a95aaef7de9f4f48826086b8cae36043/execute?api-version=2.0&details=true'
	headers = {'Content-Type': 'application/json', 'Authorization': 'Bearer SjCoFqE4ovBSNzXXaevofbGH/XKtdqFCljX0oJxd7YJ49smq0BA8rGxRXD+UqtcM/Hlgih5o4sK6wxe0wkcOww=='}

	''' Suppose the MBTA predicts the bus will come 10 minutes from now '''   
	localFormat = "%Y-%m-%d %H:%M:%S"
	utcmoment_unaware = pd.datetime.utcnow() + pd.Timedelta(10)
	utcmoment = utcmoment_unaware.replace(tzinfo=utc)
	localDatetime = utcmoment.astimezone(timezone('US/Eastern'))
	mbta_api_pred_arrival_time = localDatetime.strftime(localFormat)

	''' Try to call this web service '''
	try:
	    ''' Create and send the request '''
	    data = {"Inputs": {"input1": {"ColumnNames": ["route_tag", "stop_tag", "mbta_api_pred_arrival_time"],
	                                       "Values": [[route_tag, stop_tag, mbta_api_pred_arrival_time]]}},
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