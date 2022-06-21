namespace WebApplication2.Services
{
    public interface IMQService
    {
        void CreateMQ(string message);
        string ConsumeMQ();
    }
}
