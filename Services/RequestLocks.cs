using EcobayApp.Contex;
using Microsoft.EntityFrameworkCore;

namespace EcobayApp.Services;

public static class RequestLocks
{
    public static Task AcquireAsync(AppDBContext db, long requestId) =>
        db.Database.ExecuteSqlInterpolatedAsync($@"
            DECLARE @result int;
            EXEC @result=sys.sp_getapplock @Resource={"ecobay:request:" + requestId},
                 @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000;
            IF @result < 0 THROW 51000, 'Request is busy. Retry.', 1;");
}
