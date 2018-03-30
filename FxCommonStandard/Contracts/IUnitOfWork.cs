// ReSharper disable UnusedMember.Global

using System;

namespace FxCommonStandard.Contracts
{
	public interface IUnitOfWork<in T> : IDisposable
	{
		void New(T @object);
		void Update(T @object);
		void Delete(T @object);
		void Commit();
		void RollBack();
	}
}