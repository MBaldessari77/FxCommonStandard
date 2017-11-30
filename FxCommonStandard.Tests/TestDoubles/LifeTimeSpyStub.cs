using System.Threading;

namespace FxCommonStandard.Tests.TestDoubles
{
	public class LifeTimeSpyStub
	{
		public static int CtorCalls;
		public static int FinalizerCalls;

		public LifeTimeSpyStub()
		{
			Interlocked.Increment(ref CtorCalls);
		}

		~LifeTimeSpyStub()
		{
			Interlocked.Increment(ref FinalizerCalls);
		}

		public LifeTimeSpyStub Child { get; set; }
	}
}