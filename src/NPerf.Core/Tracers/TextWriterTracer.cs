//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 1.1.4322.573
//
//     Changes to this file may cause incorrect behavior and will be lost if 
//     the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------

namespace NPerf.Core.Tracer
{
	using System;
	using System.IO;
	
	
	/// <summary>
	/// TODO - Add class summary
	/// </summary>
	/// <remarks>
	/// 	created by - dehalleux
	/// 	created on - 26/01/2004 13:21:54
	/// </remarks>
	public class TextWriterTracer
	{
		private TextWriter writer;
		/// <summary>
		/// Default constructor - initializes all fields to default values
		/// </summary>
		public TextWriterTracer(TextWriter writer)
		{
			if (writer==null)
				throw new ArgumentNullException("writer");
			this.writer = writer;
		}
		
		public TextWriterTracer()
		{
			this.writer = Console.Out;
		}
		
		public void Attach(PerfTester tester)
		{
			if (tester==null)
				throw new ArgumentNullException("tester");
			
			tester.StartTest += new PerfTestEventHandler(this.StartTest);
			tester.FinishTest += new PerfTestEventHandler(this.FinishTest);		
			tester.IgnoredTest += new PerfTestEventHandler(this.IgnoredTest);
			tester.StartRun += new PerfTestRunEventHandler(this.StartRun);
			tester.FinishRun += new PerfTestRunEventHandler(this.FinishRun);
		}
		
		private void StartTest(Object sender, PerfTestEventArgs args)
		{
			this.writer.WriteLine("Start Test: {0}", args.Test.Name);			
		}
		
		
		private void FinishTest(Object sender, PerfTestEventArgs args)
		{
			this.writer.WriteLine("Finish Test: {0} runs",args.Test.Runs.Count);			
		}

		private void IgnoredTest(Object sender, PerfTestEventArgs args)
		{
			this.writer.WriteLine("Ignore Test: {0}, {1}",args.Test.Name,args.Test.IgnoredMessage);			
		}

		private void StartRun(Object sender, PerfTestRunEventArgs args)
		{
			this.writer.WriteLine("Start Run: {0}",args.Run.Value);						
		}

		private void FinishRun(Object sender, PerfTestRunEventArgs args)
		{
			this.writer.WriteLine("Finish Run: {0}/{1}",args.Run.Results.Count, args.Run.FailedResults.Count);						
		}
	
	}
}
