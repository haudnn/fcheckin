using OnetezSoft.Services;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
  public class DbService
  {
    private readonly GlobalService _globalService;

    public DbService(GlobalService globalService)
    {
      _globalService = globalService;
    }

    public GlobalService GetService()
    {
      return _globalService;
    }

    public async Task<SharedStorage> GetShareStorage(string id)
    {
      return await _globalService.GetShareStorage(id);
    }
  }
}
