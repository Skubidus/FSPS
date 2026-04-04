using FSPSLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FSPSLibrary
{
	public interface IProfileStore
	{
		Task<IEnumerable<ProfileModel>> LoadAsync();
		Task SaveAsync(IEnumerable<ProfileModel> profiles);
	}
}
