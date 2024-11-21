using net_test_generator_svc.Repositories.Cache;
using net_test_generator_svc.Repositories.MySql;
using net_test_generator_svc.Repositories;

namespace net_test_generator_svc.Repositories
{
	public interface ISiswaRepository {
		ISiswaDb db();
		ISiswaCache cache();
	}

public class SiswaRepository: ISiswaRepository 
{
	private readonly ISiswaDb _Db;
	private readonly ISiswaCache _Cache;

	public SiswaRepository(ISiswaDb Db, ISiswaCache Cache)
	{
	    _Db = Db ?? throw new ArgumentNullException(nameof(Db));
	    _Cache = Cache ?? throw new ArgumentNullException(nameof(Cache));
	}

	public ISiswaDb db()
	{
	    return _Db;
	}

	public ISiswaCache cache()
	{
	    return _Cache;
	}
}
}
