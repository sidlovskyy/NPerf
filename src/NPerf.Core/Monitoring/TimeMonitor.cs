 using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;

namespace NPerf.Core.Monitoring
{
	/// <summary>
	/// A high performance timer
	/// </summary>
	/// <remarks>
	/// High Precision Timer based on Win32 methods.
	/// </remarks>
	public class TimeMonitor
	{
		[DllImport("Kernel32.dll")]
		private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);  

		[DllImport("Kernel32.dll")]
		private static extern bool QueryPerformanceFrequency(out long lpFrequency);
		
		private long startTime, stopTime;
		private long now;
		private long freq;
		
		// Constructor
		public TimeMonitor()
		{
			startTime = 0;
			stopTime  = 0;

			if (QueryPerformanceFrequency(out freq) == false)
			{
				// high-performance counter not supported 
				throw new Win32Exception(); 
			}
		}
		
		public long Frequency
		{
			get
			{
				return this.freq;
			}
		}
		
		// Start the timer
		public void Start()
		{
			// lets do the waiting threads there work
			Thread.Sleep(0);  

			QueryPerformanceCounter(out startTime);
		}
		
		// Stop the timer
		public void Stop()
		{
			QueryPerformanceCounter(out stopTime);
		}
		
		public double Now
		{
			get
			{
				QueryPerformanceCounter(out now);
				return (double)(now - startTime) / (double) freq;
			}		
		}

		// Returns the duration of the timer (in seconds)
		public double Duration
		{
			get
			{
				return (double)(stopTime - startTime) / (double) freq;
			}
		}
	}
}
