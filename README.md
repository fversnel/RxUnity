# RxUnity

Easy usage of Rx in Unity3D projects.

## Features

* Removes all boilerplate needed to use Rx from Unity.
* Includes some observables for input, raycasting, time.
* Manages the lifecycle of Observables for you, e.g. no resource leaks when forgetting to unsubscribe etc.
* Makes an effort to have zero allocation observables to prevent a garbage collection nightmare.

## Installation

Install Rx 1.x from NuGet:

* Install from NuGet:

        Install-Package Rx-Main -Version 1.0.11226

* Move the .Net35 dll to the Assets folder, e.g. Assets/Plugins/Rx
* Copy the Assets/Plugins/RxUnity to your own project.

## Usage

Create an anonymous update behaviour:

To allow Rx Observers hook into Unity's Object lifecycle you have to create an AnonymousUpdateBehaviour.
This is a single MonoBehaviour that can for example be attached to a global GameObject. The Monobehaviour
is then used in creation of new Observables, e.g. to capture mouse events from the Unity Update loop. Moreover,
Whenever the MonoBehaviour is destroyed any subscriptions will automatically be disposed of. So no need to worry about subscription leaks.

Let's get on with it by creating our own observable. In this we create an observable that contains the delta time of every update tick:

	IUpdateBehaviour updateBehaviour = ... // Reference to an UpdateBehaviour, e.g. from the scene.
    var update = UnityRxObservable.CreateUpdate<float>(updateBehaviour, observer => observer.OnNext(Time.deltaTime))
    update.subscribe(delta => Debug.Log(delta));

We can also use one of the readily available Observables:

    var mouse = UnityInputObservable.MouseMove(updateBehaviour);