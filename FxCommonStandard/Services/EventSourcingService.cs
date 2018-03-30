using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FxCommonStandard.Contracts;

namespace FxCommonStandard.Services
{
	public sealed class EventSourcingService : IDisposable
	{
		readonly IUnitOfWorkFactory<EventArgs> _unitOfWorkFactory;
		readonly ManualResetEventSlim _event = new ManualResetEventSlim(false);
		readonly ConcurrentQueue<EventArgs> _eventQueue = new ConcurrentQueue<EventArgs>();
		readonly ConcurrentBag<EventSubscription> _eventMapping = new ConcurrentBag<EventSubscription>();

		long _processingEvent;
		bool _disposing;

		public EventSourcingService(IUnitOfWorkFactory<EventArgs> unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			new Thread(EventSourcingWorker).Start();
		}

		public void Dispose()
		{
			_disposing = true;
			_event.Dispose();
		}

		public void SubscribeEvent(EventHandler @delegate, EventArgs expected = null) { _eventMapping.Add(new EventSubscription { EventHandler = @delegate, EventArgs = expected }); }

		public void AddEvent(EventArgs e = null)
		{
			//Update event per delegate count
			foreach (EventSubscription subscription in _eventMapping)
				if (Equals(subscription.EventArgs, e))
					Interlocked.Increment(ref _processingEvent);

			using (var unitOfWork = _unitOfWorkFactory.New())
			{
				unitOfWork.New(e ?? new EventArgs());
				unitOfWork.Commit();
			}

			_eventQueue.Enqueue(e);

			_event.Set();
		}

		public void WaitEventsProcessed(int timeoutMilliseconds = -1)
		{
			var sw = new Stopwatch();
			sw.Start();
			while (ProcessingEvents > 0 && timeoutMilliseconds < 0 || sw.ElapsedMilliseconds <= timeoutMilliseconds)
				Thread.Sleep(0);
		}

		public Task WaitEventsProcessedAsync(int timeoutMilliseconds = -1) { return Task.Run(() => { WaitEventsProcessed(timeoutMilliseconds); }); }

		long ProcessingEvents => Interlocked.Read(ref _processingEvent);

		void EventSourcingWorker()
		{
			while (!_disposing)
			{
				if (_eventQueue.TryDequeue(out var e))
				{
					_event.Reset();

					foreach (EventSubscription subscription in _eventMapping)
						if (Equals(subscription.EventArgs, e))
							Task.Run(() =>
							{
								try
								{
									subscription.EventHandler(this, e);
								}
								finally
								{
									Interlocked.Decrement(ref _processingEvent);
								}
							});

					continue;
				}

				_event.Wait();
			}
		}

		class EventSubscription
		{
			public EventHandler EventHandler { get; set; }
			public EventArgs EventArgs { get; set; }
		}
	}
}