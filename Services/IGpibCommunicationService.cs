namespace UniversalPressureController.Services
{
    public interface IGpibCommunicationService
    {
        void Connect(int address);
        void SendCommand(string command);
        string ReadResponse();
    }
}
