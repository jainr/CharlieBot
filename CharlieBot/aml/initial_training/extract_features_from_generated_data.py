'''
extract_features_from_generated_data.py
2016 Mary Wahl, Paul Oka, Richin Jain, Min Qiu

Takes the generated cold-start data as input and creates the features that will
be used to train the model. This requires e.g. inferring the true arrival times
in order to calculate delta, the feature we would like our model to predict.

*** The model itself ***
I have used a Decision Forest Regression model with the following settings,
although we didn't experiment much with these:
Resampling method: bagging
Number of trees: 1000
Maximum depth: 32
Number of random splits per node: 128
Minimum number of samples per leaf node: 50
Allow unknown values: checked
'''

import pandas as pd
import numpy as np

def azureml_main(df = None, dataframe2 = None):
    '''
    Need to analyze the input dataframe to find the true arrival times for the route.
    Then, need to report for each timepoint what the true time-to-next-arrival was.
    Finally, we can determine what the delta is between the predicted and true
    time-to-next arrival at each timepoint.
    '''
    df['mbta_api_call_time'] = pd.to_datetime(df['mbta_api_call_time'])
    df['mbta_api_pred_time_left'] = [i.seconds / 60. for i in pd.to_timedelta(df['mbta_api_pred_time_left'])]
    
    df['pred_delta_from_last_timepoint'] = df['mbta_api_pred_time_left'].shift(periods=-1) - df['mbta_api_pred_time_left']
    df['arrival_occurred'] = False
    df.loc[df['pred_delta_from_last_timepoint'] > 1.0, 'arrival_occurred'] = True
    
    
    ''' Fill in true time left till next arrival '''

    df['true_time_left'] = None
    last_arrival_time = df['mbta_api_call_time'].values[0]
    arrival_times = df.loc[df['arrival_occurred'], 'mbta_api_call_time']
    
    for arr_time in arrival_times:
        num_of_minutes_elapsed = (arr_time - last_arrival_time).seconds / 60.
        times_to_update = pd.date_range(last_arrival_time, periods=num_of_minutes_elapsed + 1, freq='T')
        for i, time_to_update in enumerate(times_to_update):
            current = df.loc[df['mbta_api_call_time'] == time_to_update, 'true_time_left'].values[0]
            df.loc[df['mbta_api_call_time'] == time_to_update, 'true_time_left'] = np.min(num_of_minutes_elapsed - i, current)
        last_arrival_time = arr_time + pd.Timedelta(minutes=1)
    df.drop(df['mbta_api_call_time'] > last_arrival_time, 0, inplace=True) # can't fill in true arrival times at end of day_of_week
    
    
    ''' Add delta, the field to be predicted, and features to use for the model '''
    df['delta'] = df['true_time_left'] - df['mbta_api_pred_time_left'] 
    df['hour'] = [i.hour for i in df['mbta_api_call_time']]
    df['day_of_week'] = [i.dayofweek for i in df['mbta_api_call_time']]
    df['is_weekday'] = [i < 5 for i in df['day_of_week']]
    
    
    ''' Remove intermediate fields '''
    df.drop(['mbta_api_call_time', 'arrival_occurred',
             'pred_delta_from_last_timepoint', 'true_time_left'], 1, inplace=True)
    
    return df
