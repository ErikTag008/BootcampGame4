using Gilzoide.UpdateManager;
using KBCore.Refs;

namespace Project.Assets._Project._Scripts
{
    public abstract class ValidatedManagedBehaviour : ValidatedMonoBehaviour, IManagedObject
    {
        protected virtual void OnEnable()
        {
            this.RegisterInManager();
        }

        protected virtual void OnDisable()
        {
            this.UnregisterInManager();
        }
    }
}
