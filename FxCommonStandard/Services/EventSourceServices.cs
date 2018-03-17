using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FxCommonStandard.Services
{
	public class EventSourceService : IDisposable
	{
		readonly ConcurrentQueue<EventArgs> _eventQueue = new ConcurrentQueue<EventArgs>();
		readonly ConcurrentQueue<EventArgs> _eventProcessingQueue = new ConcurrentQueue<EventArgs>();
		readonly ConcurrentBag<Tuple<EventHandler, EventArgs>> _eventMapping = new ConcurrentBag<Tuple<EventHandler, EventArgs>>();

		bool _disposed;

		public EventSourceService() { new Thread(EventSourcingWorker).Start(); }
		public void Dispose() { _disposed = true; }

		public void SubscribeEvent(EventHandler @delegate, EventArgs expected = null) { _eventMapping.Add(new Tuple<EventHandler, EventArgs>(@delegate, expected)); }


		public void AddEvent(EventArgs e = null)
		{
			foreach (Tuple<EventHandler, EventArgs> tuple in _eventMapping)
				if (Equals(tuple.Item2, e))
					_eventQueue.Enqueue(e);
		}

		public Task EventsProcessedAsync(int timeoutMs = 0)
		{
			while (!_eventQueue.IsEmpty || !_eventProcessingQueue.IsEmpty)
			{
				Thread.Sleep(timeoutMs);
				if (timeoutMs > 0)
					break;
			}
			return Task.CompletedTask;
		}

		void EventSourcingWorker()
		{
			while (!_disposed)
			{
				if (_eventQueue.TryPeek(out var e) && !_eventProcessingQueue.Contains(e))
					foreach (Tuple<EventHandler, EventArgs> tuple in _eventMapping)
						if (Equals(tuple.Item2, e))
						{
							_eventProcessingQueue.Enqueue(e);
							Task.Run(() =>
							{
								tuple.Item1.Invoke(this, tuple.Item2);
								_eventQueue.TryDequeue(out e);
								_eventProcessingQueue.TryDequeue(out e);
							});
						}

				Thread.Sleep(0);
			}
		}
	}
}