using System;
using System.Reactive.Disposables;

namespace RxUnity.Core {
    public static class AnonymousMonoBehaviour {
        private static readonly Action EmptyAction = () => { };

        /// <summary>
        ///     Creates an anonymous monobehaviour instance from the given update Action
        ///     The instance can be destroyed by calling Dispose on the returned IDisposable.
        /// </summary>
        /// <param name="updateBehaviour">The MonoBehaviour to attach the update action to.</param>
        /// <param name="updateFn">Called whenever AnonymousMonobehaviour.Update() is called.</param>
        /// <param name="onDestroyFn">Called whenever AnonymousMonobehaviour is destroyed, useful for cleaning up.</param>
        /// <returns>A disposable allowing the caller to destroy this anonymous instance.</returns>
        public static IDisposable Update(IUpdateBehaviour updateBehaviour, Action updateFn, Action onDestroyFn = null) {
            var updateTask = new UpdateTask(updateFn, onDestroyFn ?? EmptyAction);
            updateBehaviour.Add(updateTask);
            return Disposable.Create(() => updateBehaviour.Remove(updateTask));
        }
    }
}