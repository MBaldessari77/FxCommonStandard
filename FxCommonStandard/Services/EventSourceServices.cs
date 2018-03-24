using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using FxCommonStandard.Contracts;

namespace FxCommonStandard.Services
{
	public class EventSourcingService : IDisposable
	{
		readonly IUnitOfWork<EventArgs> _unitOfWork;
		readonly ManualResetEventSlim _event = new ManualResetEventSlim(false);
		readonly ConcurrentQueue<EventArgs> _eventQueue = new ConcurrentQueue<EventArgs>();
		readonly ConcurrentBag<Tuple<EventHandler, EventArgs>> _eventMapping = new ConcurrentBag<Tuple<EventHandler, EventArgs>>();

		long _processingEvent;
		bool _disposed;

		public EventSourcingService(IUnitOfWork<EventArgs> unitOfWork)
		{
			_unitOfWork = unitOfWork;
			new Thread(EventSourcingWorker).Start();
		}

		public void Dispose()
		{
			_event.Set();
			_disposed = true;
		}

		public long ProcessingEvents => Interlocked.Read(ref _processingEvent);

		public void SubscribeEvent(EventHandler @delegate, EventArgs expected = null) { _eventMapping.Add(new Tuple<EventHandler, EventArgs>(@delegate, expected)); }

		public void AddEvent(EventArgs e = null)
		{
			//Update event per delegate count
			foreach (Tuple<EventHandler, EventArgs> tuple in _eventMapping)
				if (Equals(tuple.Item2, e))
					Interlocked.Increment(ref _processingEvent);

			_eventQueue.Enqueue(e);

			_unitOfWork.New(e ?? new EventArgs());
			_unitOfWork.Commit();

			_event.Set();
		}

		void EventSourcingWorker()
		{
			while (!_disposed)
			{
				if (_eventQueue.TryDequeue(out var e))
				{
					_event.Reset();

					foreach (Tuple<EventHandler, EventArgs> tuple in _eventMapping)
						if (Equals(tuple.Item2, e))
							Task.Run(() =>
							{
								try
								{
									tuple.Item1(this, e);
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
	}
}