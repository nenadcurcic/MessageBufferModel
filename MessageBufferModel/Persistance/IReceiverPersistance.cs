using System.Threading.Tasks;

namespace MessageBufferModel.Persistance
{
    public interface IReceiverPersistance
    {
        string GetFreeReceiverInstanceConnectionID();
        Task RegisterReceiverInstance(string connectionId);
        Task DisposeReceiverInstance(string connID);
    }
}
