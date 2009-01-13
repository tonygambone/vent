using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace DEQServices {
    /// <summary>
    /// Interface defenition for the chat service.
    /// </summary>
    [ServiceContract(Namespace="DEQServices")]
    interface IChatService
    {
        [OperationContract]
        string GetCurrentUsername();

        [OperationContract]
        System.Collections.Generic.IList<Message> GetMessages();

        [OperationContract]
        System.Collections.Generic.IList<string> GetJoinedUsers();

        [OperationContract]
        bool Join();

        [OperationContract]
        void Leave();

        [OperationContract]
        void PostMessage(string message);
    }
}
