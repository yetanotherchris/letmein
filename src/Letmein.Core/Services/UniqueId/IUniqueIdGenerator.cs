using Letmein.Core.Configuration;

namespace Letmein.Core.Services.UniqueId
{
	public interface IUniqueIdGenerator
	{
		string Generate(IdGenerationType idGenerationType);
	}
}