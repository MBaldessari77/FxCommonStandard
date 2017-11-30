using System;

namespace FxCommon.Extensions
{
// ReSharper disable once InconsistentNaming
	public static class GCExtensions
	{
		public static void CollectAndWaitFinalizers()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		public static void CompleteCollectAndWaitFinalizers()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}
	}
}