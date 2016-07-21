'''
cold_start_data_generation.py
2016 Mary Wahl, Min Qiu, Richin Jain

Generates phony arrival time data to simulate what would be produced by Min's
web job and ingressed from a SQL table during retraining.
'''

import pandas as pd
import numpy as np

df = pd.DataFrame()
df['mbta_api_call_time'] = pd.date_range('7/12/2016', periods=60*24*7, freq='T')

'''
Let's pretend the arrival time is undergoing a directed random walk with
absorption at time zero. At most times of day, the average decrease in arrival
time will be -1 per minute, but between 7-9 AM and 4-6 PM, it will be -0.8 per
minute. Buses will depart toward our stop of interest every thirty minutes with
an initial predicted arrival time thirty minutes later.
'''
departure_times = pd.date_range('7/12/2016', periods=2*24*7, freq='30T')
df['mbta_api_pred_time_left'] = pd.Timedelta(minutes=30)

for dep_time in departure_times:
	# Initialize departure time
	pred_time_left = pd.Timedelta(minutes=30)
	current_time = dep_time
	while pred_time_left > pd.Timedelta(0):
		'''
		Check whether an earlier bus is already predicted to arrive sooner than
		the current bus. If not, add the predicted arrival time for the current
		bus.
		'''
		try:
			existing_time_left = df.loc[df['mbta_api_call_time'] == current_time,
									    'mbta_api_pred_time_left'].values[0]
		except Exception as e:
			print(current_time)
		if pred_time_left < existing_time_left:
			df.loc[df['mbta_api_call_time'] == current_time,
				   'mbta_api_pred_time_left'] = pred_time_left

		'''
		Estimated arrival time will decrease by one minute on average if this
		is a weekend or not a rush hour. Otherwise, the estimated arrival time
		will decrease by 0.8 minutes (resulting in an average lateness of 7.5
		minutes).
		'''
		if (((current_time.hour >= 7 and current_time.hour < 9) or
			(current_time.hour >= 4 and current_time.hour < 6)) and
			current_time.dayofweek < 5):
			time_step = pd.Timedelta(minutes=-0.8)
		else:
			time_step = pd.Timedelta(minutes=-1)
		rand_num = np.random.rand()
		if rand_num < 0.1:
			time_step += pd.Timedelta(minutes=-1)
		elif rand_num > 0.9:
			time_step += pd.Timedelta(minutes=1)

		''' Update the predicted arrival time and the current time '''
		pred_time_left += time_step
		current_time += pd.Timedelta(minutes=1)

''' Save the predicted arrival times to an output file '''
df.to_csv('sample_data.csv', index=False)