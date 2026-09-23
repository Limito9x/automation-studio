using System.Threading;
using System.Threading.Tasks;
using TickerQ.Utilities.Base;

namespace Automation.Files.Jobs;

public class TickerQJobs(FileJobs fileJobs)
{
    // Ch?y m?i ngày 1 l?n vào lúc 2:00 AM sáng
    [TickerFunction("RunOrphanedCleanupAsync", "0 2 * * *")]
    public async Task RunOrphanedCleanupAsync(CancellationToken ct)
    {
        await fileJobs.CleanupOrphanedFiles(ct);
    }

    // Ch?y 1 tu?n 1 l?n vào lúc 3:00 AM sáng Ch? Nh?t
    [TickerFunction("RunUntrackedStorageCleanupAsync", "0 3 * * 0")]
    public async Task RunUntrackedStorageCleanupAsync(CancellationToken ct)
    {
        await fileJobs.CleanupUntrackedStorageFiles(ct);
    }
}


