'''
fill_in_true_times_to_arrival.py
2016 Mary Wahl, Min Qiu, Richin Jain

Designed to load phony arrival time data and identify the times when arrival
occurred. (They can be found as timepoints when the predicted arrival time
increases.) This allows addition of a "true time to next arrival" feature to
compare with the prediction included in the original dataset. A third feature
(delta, how much we should shift the mbta prediction by) can then be calculated
and used in the 
'''

import pandas as pd
import numpy as np

df = pd.read_csv('sample_data.csv')

''' Identify when the arrrivals are occurring '''

df['mbta_api_call_time'] = pd.to_datetime(df['mbta_api_call_time'])
df['mbta_api_pred_time_left'] = [i.seconds / 60. for i in pd.to_timedelta(df['mbta_api_pred_time_left'])]
    
df['pred_delta_from_last_timepoint'] = df['mbta_api_pred_time_left'].shift(periods=-1) - df['mbta_api_pred_time_left']
df['arrival_occurred'] = False
df.loc[df['pred_delta_from_last_timepoint'] > 1.0, 'arrival_occurred'] = True


''' Fill in true time left till next arrival '''

df['true_time_left'] = 0.0
last_arrival_time = df['mbta_api_call_time'].values[0]
arrival_times = df.loc[df['arrival_occurred'], 'mbta_api_call_time']

for arr_time in arrival_times:
	num_of_minutes_elapsed = (arr_time - last_arrival_time).seconds / 60.
	times_to_update = pd.date_range(last_arrival_time, periods=num_of_minutes_elapsed, freq='T')
	for i, time_to_update in enumerate(times_to_update):
		df.loc[df['mbta_api_call_time'] == time_to_update, 'true_time_left'] = num_of_minutes_elapsed - i
	last_arrival_time = arr_time

df.to_csv('added_true_arrival_times.csv', index=None)
