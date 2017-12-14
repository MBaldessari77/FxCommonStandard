using System.Threading;
using FxCommonStandard.Extensions;
using FxCommonStandard.Tests.TestDoubles;
using Xunit;

namespace FxCommonStandard.Tests
{
// ReSharper disable once InconsistentNaming
	public class GCExtesionsTest
	{
		[Fact(Skip = "In.NET Core 2.0 See https://github.com/dotnet/coreclr/issues/15207")]
		public void WhenIsCalledCollectAndWaitFinalizersFinalizersOnReleasedObjectIsInvoked()
		{
			int ctorCalls = LifeTimeSpyStub.CtorCalls;
			int finalizerCalls = LifeTimeSpyStub.FinalizerCalls;

			// ReSharper disable once NotAccessedVariable
			var stub = new LifeTimeSpyStub();

			Assert.Equal(ctorCalls + 1, LifeTimeSpyStub.CtorCalls);
			Assert.Equal(finalizerCalls, LifeTimeSpyStub.FinalizerCalls);

			// ReSharper disable once RedundantAssignment
			stub = null;

			Assert.Equal(ctorCalls + 1, LifeTimeSpyStub.CtorCalls);
			Assert.Equal(finalizerCalls, LifeTimeSpyStub.FinalizerCalls);

			GCExtensions.CollectAndWaitFinalizers();

			Thread.Sleep(1000);

			Assert.Equal(ctorCalls + 1, LifeTimeSpyStub.CtorCalls);
			Assert.Equal(finalizerCalls + 1, LifeTimeSpyStub.FinalizerCalls); 
		}
	}
}