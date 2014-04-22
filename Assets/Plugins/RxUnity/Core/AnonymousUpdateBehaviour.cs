using System.Collections.Generic;
using RxUnity.Core;
using UnityEngine;

public class AnonymousUpdateBehaviour : MonoBehaviour, IUpdateBehaviour {
    private IList<UpdateTask> _activeUpdates;
    private IList<UpdateTask> _addUpdateQueue;
    private IList<UpdateTask> _removeUpdateQueue;

    public void Add(UpdateTask updateTask) {
        _addUpdateQueue.Add(updateTask);
    }

    public void Remove(UpdateTask updateTask) {
        _addUpdateQueue.Remove(updateTask);
    }

    private void Awake() {
        _addUpdateQueue = new List<UpdateTask>();
        _removeUpdateQueue = new List<UpdateTask>();
        _activeUpdates = new List<UpdateTask>();
    }

    private void Update() {
        ProcessQueuedChanges();

        for (int i = 0; i < _activeUpdates.Count; i++) {
            _activeUpdates[i].UpdateFn();
        }
    }

    private void OnDestroy() {
        for (int i = 0; i < _activeUpdates.Count; i++) {
            _activeUpdates[i].OnDestroyFn();
        }
    }

    private void ProcessQueuedChanges() {
        for (int i = 0; i < _addUpdateQueue.Count; i++) {
            _activeUpdates.Add(_addUpdateQueue[i]);
        }
        _addUpdateQueue.Clear();

        for (int i = 0; i < _removeUpdateQueue.Count; i++) {
            _activeUpdates.Remove(_removeUpdateQueue[i]);
        }
        _removeUpdateQueue.Clear();
    }
}