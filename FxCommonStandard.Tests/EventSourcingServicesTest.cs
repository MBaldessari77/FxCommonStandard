using System;
using System.Threading;
using FxCommonStandard.Contracts;
using FxCommonStandard.Services;
using Moq;
using Xunit;

namespace FxCommonStandard.Tests
{
	public class EventSourcingServicesTest
	{
		[Fact]
		public void WhenAnEventIsRaisedTheListenerReceiveTheEvent()
		{
			int raisedCount = 0;

			using (var service = new EventSourcingService(new Mock<IUnitOfWork<EventArgs>>().Object))
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

			using (var service = new EventSourcingService(new Mock<IUnitOfWork<EventArgs>>().Object))
			{
				service.SubscribeEvent((sender, args) => { raisedCount++; }, new CustomEventArgs());
				service.SubscribeEvent((sender, args) => { raisedCount++; });

				service.AddEvent(new CustomEventArgs());

				WaitSpinLock(service);
			}

			Assert.Equal(1, raisedCount);
		}

		[Fact]
		public void WhenAnEventIsRaisedAllTheListenerReceiveTheEvent()
		{
			int raisedCount1 = 0;
			int raisedCount2 = 0;

			using (var service = new EventSourcingService(new Mock<IUnitOfWork<EventArgs>>().Object))
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
			using (var service = new EventSourcingService(new Mock<IUnitOfWork<EventArgs>>().Object))
			{
				service.SubscribeEvent((sender, args) => { Thread.Sleep(int.MaxValue); });
				service.SubscribeEvent((sender, args) => { raisedCount++; });

				service.AddEvent();

				WaitSpinLock(service, 1);
			}

			Assert.Equal(1, raisedCount);
		}

		[Fact]
		public void TwoServiceSubriscribeAnEventReceivedEventOnTheirListener()
		{
			int raisedCount1 = 0;
			int raisedCount2 = 0;

			using (var service = new EventSourcingService(new Mock<IUnitOfWork<EventArgs>>().Object))
			{
				service.SubscribeEvent((sender, args) => ++raisedCount1);
				service.SubscribeEvent((sender, args) => ++raisedCount2);

				var th1 = new Thread(() =>
				  {
					  // ReSharper disable once AccessToDisposedClosure
					  WaitSpinLock(service);
				  });

				var th2 = new Thread(() =>
				  {
					  // ReSharper disable once AccessToDisposedClosure
					  WaitSpinLock(service);
				  });

				service.AddEvent();

				th1.Start();
				th2.Start();

				th1.Join();
				th2.Join();
			}

			Assert.Equal(1, raisedCount1);
			Assert.Equal(1, raisedCount2);
		}

		[Fact]
		public void AnAddedEventIsPersistedAsNewInAnUnitOfWork()
		{
			var unitOfWork = new Mock<IUnitOfWork<EventArgs>>();
			var e = new EventArgs();

			using (var service = new EventSourcingService(unitOfWork.Object))
				service.AddEvent(e);

			unitOfWork.Verify(u => u.New(e));
			unitOfWork.Verify(u => u.Commit());
		}

		[Fact]
		public void IfAnEventSubscriberThrowAnExceptionTheEventMustBeUpdateTheProcessingQueueRequest()
		{
			using (var service = new EventSourcingService(new Mock<IUnitOfWork<EventArgs>>().Object))
			{
				service.SubscribeEvent((sender, args) => throw new NotImplementedException());

				service.AddEvent();

				WaitSpinLock(service);
			}
		}

		[Fact]
		public void TheEventRaisedToSusbscriberIsExactlyTheSameAddedToEventSourcing()
		{
			using (var service = new EventSourcingService(new Mock<IUnitOfWork<EventArgs>>().Object))
			{
				var @event=new CustomEventArgs();
				EventArgs receivedEvent = null;

				service.SubscribeEvent((sender, args) => { receivedEvent = args; }, new CustomEventArgs());

				service.AddEvent(@event);

				WaitSpinLock(service);

				Assert.Same(receivedEvent, @event);
			}
		}

		void WaitSpinLock(EventSourcingService service, int remainingProcessingEvents = 0)
		{
			while (service.ProcessingEvents > remainingProcessingEvents)
				Thread.Sleep(0);
		}

		class CustomEventArgs : EventArgs
		{
			public override bool Equals(object obj) { return obj.GetType()==GetType(); }
			public override int GetHashCode() { return GetType().FullName.GetHashCode(); }
		}
	}
}