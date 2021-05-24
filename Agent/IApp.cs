using System.Threading;

namespace Agent
{
    interface IApp
    {
        void Run(CancellationToken cancellationToken);
    }
}