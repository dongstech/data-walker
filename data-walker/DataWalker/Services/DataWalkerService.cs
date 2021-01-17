using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace DataWalker.Services
{
    internal class DataWalkerService : IHostedService
    {
        private readonly IDataWalker _dataWalker;

        public DataWalkerService(IDataWalker dataWalker)
        {
            _dataWalker = dataWalker;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _dataWalker.Walk();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }
    }
}