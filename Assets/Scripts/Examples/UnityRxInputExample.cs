using RxUnity.Core;
using UnityEngine;
using System.Collections;
using System;

public class UnityRxInputExample : MonoBehaviour {

    [SerializeField] private AnonymousUpdateBehaviour _updateBehaviour;

	// Use this for initialization
	void Start () {
	    var keyboard = UnityInputObservable.KeyEvents(_updateBehaviour, UnityInputObservable.AllKeys);
	    keyboard.Subscribe(keyEvent => Debug.Log(keyEvent));
	}
}
