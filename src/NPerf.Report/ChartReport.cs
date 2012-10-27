//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 1.1.4322.573
//
//     Changes to this file may cause incorrect behavior and will be lost if 
//     the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Collections;

namespace NPerf.Report
{
	using NPerf.Core;
	using NPerf.Core.Collections;
	
	using scpl;
	
	public class ChartReport
	{
		private int width;
		private int height;
		private ColorGenerator colors;

		
		public ChartReport(int width, int height)
		{
			if (width<1)
				throw new ArgumentException("width smaller that 1");
			if (height<1)
				throw new ArgumentException("height smaller that 1");
			this.width = width;
			this.height = height;
			this.colors = new ColorGenerator(ColorMap.Jet,255);
		}
		
		public ColorGenerator Colors
		{
			get
			{
				return this.colors;
			}
		}
		
		public IDictionary Render(PerfTestSuite suite)
		{
			if (suite==null)
				throw new ArgumentNullException("suite");
			
			Hashtable bitmaps = new Hashtable();
			foreach(PerfTest test in suite.Tests)
			{
				bitmaps.Add(test, Render(suite,test));					
			}
			return bitmaps;
		}
		
		public Bitmap Render(PerfTestSuite suite, PerfTest test)
		{
			Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
			Render(bmp,suite,test);
			
			return bmp;
		}
		
		public void Render(Bitmap bmp, PerfTestSuite suite, PerfTest test)
		{
			if (bmp==null)
				throw new ArgumentNullException("bmp");
			if (suite==null)
				throw new ArgumentNullException("suite");
			if (!suite.Tests.Contains(test))
				throw new ArgumentException("suite does not contain test");
			if (test.Runs.Count==0)
				throw new ArgumentException("no run in benchmark");
						
			Hashtable resultDatas = new Hashtable();
			PerfTestRun firstRun = test.Runs[0];
			foreach(PerfResult result in firstRun.Results)
			{
				resultDatas[result.TestedType] = new double[test.Runs.Count];
			}
			foreach(PerfFailedResult failedResult in firstRun.FailedResults)
			{
				resultDatas[failedResult.TestedType] = new double[test.Runs.Count];
			}
			
			// setting up histograms
			int index = 0;
			foreach(PerfTestRun run in test.Runs)
			{
				AddResult(run,resultDatas, index++);
			}
			
			// creating lines
			PlotSurface2D plot = CreatePlot(suite,test);			
			double[] xData = CreateFeatureData(test);
			this.Colors.Reset(resultDatas.Count);
			foreach(DictionaryEntry de in resultDatas)
			{
				AddLine(plot, xData, (double[])de.Value, (string)de.Key);
			}
			
			if (plot.YAxis1!=null)
				plot.YAxis1.Label = "Duration [log(s)]";
			if (suite.FeatureDescription!=null && plot.XAxis1!=null)
				plot.XAxis1.Label = suite.FeatureDescription;
			
			// plot refresh
			Graphics g = Graphics.FromImage(bmp);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextRenderingHint = TextRenderingHint.AntiAlias;
			g.FillRectangle(new SolidBrush(Color.LightGray), new Rectangle(0,0,bmp.Width,bmp.Height));
			plot.Draw(g, new Rectangle(0,0,bmp.Width,bmp.Height));
		}
		
		internal PlotSurface2D CreatePlot(PerfTestSuite suite, PerfTest test)
		{
			PlotSurface2D plot = new PlotSurface2D();
						
			plot.Title = String.Format("{0} - {1}", suite.Name, test.Name);
			// setting up plot
			plot.LegendBorderStyle = Legend.BorderType.Shadow;
			plot.ShowLegend = true;
			plot.PlotBackColor = Color.White;
			plot.VerticalEdgeLegendPlacement = Legend.Placement.Outside;
			
			return plot;
		}
		
		internal void AddResult(PerfTestRun run, Hashtable resultDatas, int index)
		{
			foreach(PerfResult result in run.Results)
			{
				double[] data = (double[])resultDatas[result.TestedType];
				data[index] = Math.Log(result.Duration);
			}
			foreach(PerfFailedResult failedResult in run.FailedResults)
			{
				double[] data = (double[])resultDatas[failedResult.TestedType];
				data[index] = 0;
			}
		}
		
		internal double[] CreateFeatureData(PerfTest test)
		{
			double[] xdata = new double[test.Runs.Count];
			int index = 0;
			foreach(PerfTestRun run in test.Runs)
			{
				xdata[index++] = run.Value;
			}
			
			return xdata;
		}
		
		internal void AddLine(PlotSurface2D plot, double[] x, double[] y, string name)
		{
			ArrayAdapter data = new ArrayAdapter(x,y);
			Color c= NextColor();
			LinePlot line = new LinePlot(data);			
			line.Label = name;
			Pen p = new Pen(c,2);
			line.Pen = p;
			plot.Add(line);
		}
		
		internal Color NextColor()
		{
			return this.colors.MoveNextColor();
		}
	}
}
