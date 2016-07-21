'''
get_predicted_time_left_as_float.py
2016 Mary Wahl, Richin Jain, Paul Oka, Min Qiu

One input to the web service will be the predicted number of minutes until the
next arrival (difference between MBTA's estimated next arrival time and the
current time). This value is passed to the trained model as a float because of
some issues formatting to/from AML's "TimeSpan" data type in Python.

To make our input data match that schema, we'll modify our cold start generated
dataset to convert from Timedelta to a float number of minutes.
'''

import pandas as pd
import numpy as np

def azureml_main(df = None, dataframe2 = None):
    ''' Get predicted time left in minutes as float '''
    df['mbta_api_pred_time_left'] = pd.to_timedelta(df['mbta_api_pred_time_left'])
    df['mbta_api_pred_time_left'] = [i.seconds / 60. for i in df['mbta_api_pred_time_left']]
    
    return df
