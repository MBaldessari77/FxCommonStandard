using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FxCommonStandard.Services
{
	public class EventSourceService : IDisposable
	{
		readonly ManualResetEventSlim _event = new ManualResetEventSlim(false);
		readonly ConcurrentQueue<EventArgs> _eventQueue = new ConcurrentQueue<EventArgs>();
		readonly ConcurrentBag<Tuple<EventHandler, EventArgs>> _eventMapping = new ConcurrentBag<Tuple<EventHandler, EventArgs>>();
		readonly Thread _eventSourcingThread;

		long _processingEvent;
		bool _disposed;

		public EventSourceService()
		{
			_eventSourcingThread = new Thread(EventSourcingWorker);
			_eventSourcingThread.Start();
		}

		public void Dispose()
		{
			Signal();
			_disposed = true;
#if DEBUG
			_eventSourcingThread.Join();
#endif
		}

		public void SubscribeEvent(EventHandler @delegate, EventArgs expected = null) { _eventMapping.Add(new Tuple<EventHandler, EventArgs>(@delegate, expected)); }

		public void AddEvent(EventArgs e = null)
		{
			//Update event per delegate count
			foreach (Tuple<EventHandler, EventArgs> tuple in _eventMapping)
				if (Equals(tuple.Item2, e))
					Interlocked.Increment(ref _processingEvent);

			_eventQueue.Enqueue(e);

			Signal();
		}

		public Task WaitEventsProcessedAsync(int millisecondsTimeout = -1)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			return Task.Run(() =>
			{
				while (true)
				{
					if (Interlocked.Read(ref _processingEvent) == 0)
						break;
					if (millisecondsTimeout >= 0 && stopwatch.ElapsedMilliseconds > millisecondsTimeout)
						break;
					WaitSignal(millisecondsTimeout);
				}
			});
		}

		void EventSourcingWorker()
		{
			while (!_disposed)
			{
				if (_eventQueue.TryDequeue(out var e))
				{
					ResetSignal();

					foreach (Tuple<EventHandler, EventArgs> tuple in _eventMapping)
						if (Equals(tuple.Item2, e))
							Task.Run(() =>
							{
								tuple.Item1(this, tuple.Item2);

								Interlocked.Decrement(ref _processingEvent);
								if (Interlocked.Read(ref _processingEvent) == 0)
									Signal();
							});

					continue;
				}

				WaitSignal();
			}
		}

		void Signal()
		{
			lock (this)
				_event.Set();
		}

		void ResetSignal()
		{
			lock (this)
				_event.Reset();
		}

		void WaitSignal(int millisecondsTimeout = -1)
		{
			// ReSharper disable once InconsistentlySynchronizedField
			_event.Wait(millisecondsTimeout);
		}
	}
}