using RxUnity.Core;
using UnityEngine;
using System.Collections;
using System;

public class UnityRxInputExample : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    var keyboard = UnityInputObservables.KeyEvents(UnityInputObservables.AllKeys);
	    keyboard.Subscribe(keyEvent => Debug.Log(keyEvent));
	}
}
