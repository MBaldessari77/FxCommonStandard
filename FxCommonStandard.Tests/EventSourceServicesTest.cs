using System;
using System.Diagnostics;
using System.Threading;
using FxCommonStandard.Services;
using Xunit;
using Xunit.Abstractions;

namespace FxCommonStandard.Tests
{
	public class EventSourceServicesTest
	{
		private readonly ITestOutputHelper _output;

		public EventSourceServicesTest(ITestOutputHelper output)
		{
			_output = output;
		}

		[Fact]
		public async void WhenAnEventIsRaisedTheListenerReceiveTheEvent()
		{
			int raisedCount = 0;

			using (var service = new EventSourceService())
			{
				service.SubscribeEvent((sender, args) => { raisedCount++; });

				service.AddEvent();

				await service.WaitEventsProcessedAsync();
			}

			Assert.Equal(1, raisedCount);
		}

		[Fact]
		public async void WhenAParticularEventIsRaisedTheListenerRegisterToThisEventReceiveTheEvent()
		{
			int raisedCount = 0;

			using (var service = new EventSourceService())
			{
				service.SubscribeEvent((sender, args) => { raisedCount++; }, new CustomEventArgs());
				service.SubscribeEvent((sender, args) => { raisedCount++; });

				service.AddEvent(new CustomEventArgs());

				await service.WaitEventsProcessedAsync();
			}

			Assert.Equal(1, raisedCount);
		}

		[Fact]
		public async void WhenAnEventIsRaisedAllTheListenerReceiveTheEvent()
		{
			int raisedCount1 = 0;
			int raisedCount2 = 0;

			using (var service = new EventSourceService())
			{
				service.SubscribeEvent((sender, args) => { raisedCount1++; });
				service.SubscribeEvent((sender, args) => { raisedCount2++; });

				service.AddEvent();

				await service.WaitEventsProcessedAsync();
			}

			Assert.Equal(1, raisedCount1);
			Assert.Equal(1, raisedCount2);
		}

		[Fact]
		public async void AStallOnAListenerOnEventCantBlockOtherListener()
		{
			int raisedCount = 0;
			using (var service = new EventSourceService())
			{
				service.SubscribeEvent((sender, args) => { Thread.Sleep(int.MaxValue); });
				service.SubscribeEvent((sender, args) => { raisedCount++; });

				service.AddEvent();

				Stopwatch stopwatch=new Stopwatch();
				stopwatch.Start();
				await service.WaitEventsProcessedAsync(100);
				_output.WriteLine($"ElapsedMilliseconds = {stopwatch.ElapsedMilliseconds}");
			}

			Assert.Equal(1, raisedCount);
		}

		class CustomEventArgs : EventArgs
		{
			public override bool Equals(object obj) { return obj != null && obj.GetType().IsAssignableFrom(GetType()); }
			public override int GetHashCode() { return GetType().FullName.GetHashCode(); }
		}
	}
}