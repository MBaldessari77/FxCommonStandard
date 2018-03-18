using System;
using System.Threading;
using FxCommonStandard.Services;
using Xunit;

namespace FxCommonStandard.Tests
{
	public class EventSourceServicesTest
	{
		[Fact]
		public void WhenAnEventIsRaisedTheListenerReceiveTheEvent()
		{
			int raisedCount = 0;

			using (var service = new EventSourceService())
			{
				service.SubscribeEvent((sender, args) => { raisedCount++; });

				service.AddEvent();

				WaitSpinLock(service);
			}

			Assert.Equal(1, raisedCount);
		}

		[Fact]
		public void WhenAParticularEventIsRaisedTheListenerRegisterToThisEventReceiveTheEvent()
		{
			int raisedCount = 0;

			using (var service = new EventSourceService())
			{
				service.SubscribeEvent((sender, args) => { raisedCount++; }, new CustomEventArgs());
				service.SubscribeEvent((sender, args) => { raisedCount++; });

				service.AddEvent(new CustomEventArgs());

				//await service.WaitEventsProcessedAsync();
				WaitSpinLock(service);
			}

			Assert.Equal(1, raisedCount);
		}

		[Fact]
		public void WhenAnEventIsRaisedAllTheListenerReceiveTheEvent()
		{
			int raisedCount1 = 0;
			int raisedCount2 = 0;

			using (var service = new EventSourceService())
			{
				service.SubscribeEvent((sender, args) => { raisedCount1++; });
				service.SubscribeEvent((sender, args) => { raisedCount2++; });

				service.AddEvent();

				WaitSpinLock(service);
			}

			Assert.Equal(1, raisedCount1);
			Assert.Equal(1, raisedCount2);
		}

		[Fact]
		public void AStallOnAListenerOnEventCantBlockOtherListener()
		{
			int raisedCount = 0;
			using (var service = new EventSourceService())
			{
				service.SubscribeEvent((sender, args) => { Thread.Sleep(int.MaxValue); });
				service.SubscribeEvent((sender, args) => { raisedCount++; });

				service.AddEvent();

				WaitSpinLock(service, 1);
			}

			Assert.Equal(1, raisedCount);
		}

		void WaitSpinLock(EventSourceService service, int remainingProcessingEvents = 0)
		{
			while (service.ProcessingEvents > remainingProcessingEvents)
				Thread.Sleep(0);
		}

		class CustomEventArgs : EventArgs
		{
			public override bool Equals(object obj) { return obj != null && obj.GetType().IsAssignableFrom(GetType()); }
			public override int GetHashCode() { return GetType().FullName.GetHashCode(); }
		}
	}
}