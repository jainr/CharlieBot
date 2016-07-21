'''
add_model_features.py
2016 Mary Wahl, Paul Oka, Richin Jain, Min Qiu

Converts string containing a datetime into a datetime64 value. Ensure that the
float input is correctly converted to float type. Then, adds features based on
the call time, including the hour, day of the week (0=Monday), and whether the 
day of the week is a weekday. Afte extracting these features, removes the
original call time feature.
'''

import pandas as pd
import numpy as np

def azureml_main(df = None, df2 = None):
    ''' Reformat input field '''
    df['mbta_api_pred_time_left'] = df['mbta_api_pred_time_left'].astype(float)
    df['mbta_api_call_time'] = pd.to_datetime(df['mbta_api_call_time'])
    
    ''' Add more features '''
    df['hour'] = [i.hour for i in df['mbta_api_call_time']]
    df['day_of_week'] = [i.dayofweek for i in df['mbta_api_call_time']]
    df['is_weekday'] = [i < 5 for i in df['day_of_week']]
    
    ''' Remove intermediate fields '''
    df.drop(['mbta_api_call_time'], 1, inplace=True)
    
    return df
