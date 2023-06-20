namespace LChocolate
{
    public interface LNetworkClient_Interface
    {
        
        void Network_Connect_Try();//System.Action<object> todo = null, object param = null);
        void RunOnGameThread(System.Action action);
        void Network_Connect_Complete();
        void Network_Connect_Fail();
        void Network_Connect_Error(string err);

        void Network_ProcessPacket(ref ST_Packet packet);
    }
}