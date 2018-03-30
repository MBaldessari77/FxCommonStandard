namespace FxCommonStandard.Contracts
{
	public interface IUnitOfWorkFactory<in T>
	{
		IUnitOfWork<T> New();
	}
}