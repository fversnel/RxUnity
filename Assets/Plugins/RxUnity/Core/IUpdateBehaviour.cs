namespace RxUnity.Core {
    /// <summary>
    ///     Implemented by a MonoBehaviour to provide facilities to Observables
    ///     for using Observers inside an update loop. The attached observable can also use the same lifecycle
    ///     methods as the MonoBehaviour, e.g. when the MonoBehaviour is destroyed the attached Observers will be completed.
    /// </summary>
    public interface IUpdateBehaviour {
        void Add(UpdateTask updateTask);

        void Remove(UpdateTask updateTask);
    }
}