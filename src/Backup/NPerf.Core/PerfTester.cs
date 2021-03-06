using System;
using System.Reflection;

namespace NPerf.Core
{
	using NPerf.Core.Collections;
	using NPerf.Framework;
	using NPerf.Core.Monitoring;

	/// <summary>
	/// Summary description for PerfTester.
	/// </summary>
	public class PerfTester
	{
		private string description;
		private string featureDescription;
		
		private Type testerType;
		private Type testedType;
		private int testCount;
		private ConstructorInfo constructor = null;
		private MethodInfo runDescriptor = null;
		private MethodInfo setUp = null;
		private MethodInfo tearDown = null;
		private MethodInfoCollection methods;
		private TypeCollection testedTypes;
		private TimeMonitor timer;
		private MemoryMonitor memorizer;

		public PerfTester(
			Type testerType, 
			PerfTesterAttribute attr
			)
		{
			if (testerType==null)
				throw new ArgumentNullException("testerType");
			if (attr==null)
				throw new ArgumentNullException("attr");

			this.testerType = testerType;
			this.testedType = attr.TestedType;
			this.testCount = attr.TestCount;
			this.description = attr.Description;
			this.featureDescription = attr.FeatureDescription;
			
			// get constructor
			this.constructor = this.testerType.GetConstructor(Type.EmptyTypes);

			// get run descriptor
			this.runDescriptor = TypeHelper.GetAttributedMethod(this.testerType,typeof(PerfRunDescriptorAttribute));
			if (this.runDescriptor!=null)
				TypeHelper.CheckSignature(this.runDescriptor,typeof(double),typeof(int));
			
			// get set up
			this.setUp = TypeHelper.GetAttributedMethod(this.testerType,typeof(PerfSetUpAttribute));
			if (this.setUp!=null)
				TypeHelper.CheckSignature(this.setUp,typeof(void),typeof(int),this.testedType);
			
			// get tear down
			this.tearDown = TypeHelper.GetAttributedMethod(this.testerType,typeof(PerfTearDownAttribute));
			if (this.tearDown!=null)
				TypeHelper.CheckSignature(this.tearDown,typeof(void),this.testedType);

			// get test method			
			this.methods = new MethodInfoCollection();
			foreach(MethodInfo mi in TypeHelper.GetAttributedMethods(this.testerType,typeof(PerfTestAttribute)))
			{
				TypeHelper.CheckSignature(mi, typeof(void), this.testedType);
				this.methods.Add(mi);
			}

			this.testedTypes = new TypeCollection();
			this.timer = new TimeMonitor();
			this.memorizer = new MemoryMonitor();
		}

		public Type TesterType
		{
			get
			{
				return this.testerType;
			}
		}

		public Type TestedType
		{
			get
			{
				return this.testedType;
			}
		}
		
		public string Description
		{
			get
			
			{
				return this.description;
			}
		}
		
		public string FeatureDescription
		{
			get
			{
				return this.featureDescription;
			}
		}
		
		public int TestCount
		{
			get
			{
				return this.testCount;
			}
		}

		public TypeCollection TestedTypes
		{
			get
			{
				return this.testedTypes;
			}
		}
		
		public bool IsIgnored
		{
			get
			{
				return TypeHelper.HasCustomAttribute(this.testerType, typeof(PerfIgnoreAttribute));
			}
		}
		
		public string IgnoreMessage
		{
			get
			{
				PerfIgnoreAttribute attr = (PerfIgnoreAttribute)TypeHelper.GetFirstCustomAttribute(this.testerType,typeof(PerfIgnoreAttribute));
				return attr.Message;
			}
		}

		#region Events
		public event PerfTestEventHandler StartTest;
		
		protected void OnStartTest(PerfTest test)
		{
			if (this.StartTest!=null)
				this.StartTest(this, new PerfTestEventArgs(test));
		}
		
		public event PerfTestEventHandler FinishTest;

		protected void OnFinishTest(PerfTest test)
		{
			if (this.FinishTest!=null)
				this.FinishTest(this, new PerfTestEventArgs(test));
		}
		
		public event PerfTestEventHandler IgnoredTest;

		protected void OnIgnoredTest(PerfTest test)
		{
			if (this.IgnoredTest!=null)
				this.IgnoredTest(this, new PerfTestEventArgs(test));
		}	
		
		public event PerfTestRunEventHandler StartRun;
		
		protected void OnStartRun(PerfTestRun run)
		{
			if (this.StartRun!=null)
				this.StartRun(this, new PerfTestRunEventArgs(run));
		}
		
		public event PerfTestRunEventHandler FinishRun;
		
		protected void OnFinishRun(PerfTestRun run)
		{
			if (this.FinishRun!=null)
				this.FinishRun(this, new PerfTestRunEventArgs(run));
		}
	
		#endregion

		public void LoadTestedTypes(Assembly a)
		{
			if (a==null)
				throw new ArgumentNullException("a");

			if (this.testedType.IsInterface)
			{
				foreach(Type t in a.GetExportedTypes())
				{
					if (
						t.GetInterface(this.testedType.ToString())!=null
						&& !t.IsAbstract
						)
						this.testedTypes.Add(t);
				}
			}
			else
			{
				foreach(Type t in a.GetExportedTypes())
				{
					if (   t.IsInstanceOfType(TestedType)
						&& !t.IsAbstract
						)
						this.testedTypes.Add(t);
				}
			}
		}

		public PerfTestSuite RunTests()
		{	
			PerfTestSuite suite = new PerfTestSuite(this.TesterType, this.Description, this.FeatureDescription);
			
			foreach(MethodInfo test in this.methods)
			{
				PerfTest testResult = new PerfTest(test);
				if (testResult.IsIgnored)
				{
					OnIgnoredTest(testResult);
					suite.Tests.Add(testResult);
					continue;
				}
				
				OnStartTest(testResult);
				
				for(int testIndex = 0;testIndex<this.TestCount;++testIndex)
				{
					PerfTestRun run = new PerfTestRun(RunDescription(testIndex));
					
					OnStartRun(run);
					
					// for each instanced type,
					foreach(Type t in this.testedTypes)
					{
						try
						{
							//jitting if first run of the test
							if (testIndex==0)
								RunTest(-1,t,test,false);
	
							// calling
							RunTest(testIndex,t,test,true);
	
							// save results
							run.Results.Add(new PerfResult(t,this.timer.Duration,this.memorizer.Usage));
						}
						catch(Exception ex)
						{
							run.FailedResults.Add(new PerfFailedResult(t,ex));
						}
					}									
					OnFinishRun(run);
					testResult.Runs.Add(run);
				}
				OnFinishTest(testResult);
				suite.Tests.Add(testResult);
			}
			return suite;
		}

		internal void RunTest(int testIndex, Type testedType, MethodInfo method, bool monitor)
		{

			// create instance
			ConstructorInfo ci = testedType.GetConstructor(Type.EmptyTypes);
			Object tested = ci.Invoke(Type.EmptyTypes);
			// test 
			Object tester = CreateTester();
			SetUp(testIndex,tester,tested);

			// clean memory
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			// starts monitoring
			if (monitor)
			{
				this.memorizer.Start();
				this.timer.Start();
			}
			Run(tester,tested,method);
			// stop monitoring
			if (monitor)
			{
				this.timer.Stop();
				this.memorizer.Stop();
			}

			// tear down
			TearDown(tester,tested);
		}
		
		#region Static Helpers

		public static void FromAssembly(PerfTesterCollection testers, Assembly a)
		{
			if (testers==null)
				throw new ArgumentNullException("testers");
			if (a==null)
				throw new ArgumentNullException("a");

			foreach(Type t in a.GetExportedTypes())
			{
				if (TypeHelper.HasCustomAttribute(t,typeof(PerfTesterAttribute)))
				{
					PerfTesterAttribute attr = 
						(PerfTesterAttribute)TypeHelper.GetFirstCustomAttribute(
						t,
						typeof(PerfTesterAttribute)
						);
					PerfTester tester = new PerfTester(t,attr);

					testers.Add(tester);
				}
			}
		}

		public static PerfTesterCollection FromAssembly(Assembly a)
		{
			if (a==null)
				throw new ArgumentNullException("a");
			PerfTesterCollection testers = new PerfTesterCollection();
			FromAssembly(testers,a);

			return testers;
		}
		#endregion

		#region Protected
		protected Object CreateTester()
		{
			return this.constructor.Invoke(Type.EmptyTypes);
		}
	
		protected double RunDescription(int testIndex)
		{
			if (this.runDescriptor==null)
				return (double)testIndex;
			
			Object[] args = new Object[1];
			args[0]=testIndex;
			Object tester = CreateTester();
			
			return (double)this.runDescriptor.Invoke(tester,args);	
		}

		protected void SetUp(int testIndex, Object tester, Object tested)		
		{
			if (tester==null)
				throw new ArgumentNullException("tester");
			if (tested==null)
				throw new ArgumentNullException("tested");
			if (this.setUp!=null)
			{
				Object[] args = new Object[2];
				args[0]=testIndex;
				args[1]=tested;
				this.setUp.Invoke(tester,args);
			}
		}

		protected void TearDown(Object tester, Object tested)	
		{
			if (tester==null)
				throw new ArgumentNullException("tester");
			if (tested==null)
				throw new ArgumentNullException("tested");
			if (this.tearDown!=null)
			{
				Object[] args = new Object[1];
				args[0]=tested;
				this.tearDown.Invoke(tester,args);
			}
		}

		protected void Run(Object tester, Object tested, MethodInfo mi)
		{
			Object[] args = new Object[1];
			args[0]=tested;
			mi.Invoke(tester,args);
		}

		#endregion
	}
}
