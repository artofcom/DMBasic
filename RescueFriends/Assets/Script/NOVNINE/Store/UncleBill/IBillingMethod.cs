
namespace NOVNINE.Store
{

public interface IBillingMethod : System.IDisposable
{
    void Initialize(System.Action<string> callback);
    new void Dispose();

    void Purchase(string productIdentifier, bool consumable, System.Action<string> callback);
    void ConsumePurchase(string productIdentifier);

    void RestoreAllTransactions(System.Action<string[]> callback);

    void OpenStore(string storeParam = null, bool reviewPage = false);
    void SearchStore(string keyword);
}

}

