//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 1.1.4322.573
//
//     Changes to this file may cause incorrect behavior and will be lost if 
//     the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------

namespace NPerf.Core
{
	using System;
	
	
	/// <summary>
	/// TODO - Add class summary
	/// </summary>
	/// <remarks>
	/// 	created by - dehalleux
	/// 	created on - 26/01/2004 13:13:00
	/// </remarks>
	public class PerfTestRunEventArgs : System.EventArgs
	{
		private PerfTestRun run;
		
		/// <summary>
		/// Default constructor - initializes all fields to default values
		/// </summary>
		public PerfTestRunEventArgs(PerfTestRun run)
		{
			if (run==null)
				throw new ArgumentNullException("run");
			this.run = run;
		}
		
		public PerfTestRun Run
		{
			get
			{
				return this.run;
			}
		}
	}
	
	public delegate void PerfTestRunEventHandler(Object sender,PerfTestRunEventArgs e); 
}
