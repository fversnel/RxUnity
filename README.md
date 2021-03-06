# RxUnity

Making usage of Rx from Unity a breeze.

## Features

* Provides an easy way for creating Observables from Unity's Update and FixedUpdate loops.
* Includes some observables for input, raycasting, time.
* Manages the lifecycle of Observables for you, e.g. no resource leaks when forgetting to unsubscribe etc.

## Installation

Install Rx 1.x from NuGet:

* Install from NuGet:

        Install-Package Rx-Main -Version 1.0.11226
		Install-Package TaskParallelLibrary

* Move the .Net35 dll from Rx to the Assets folder, e.g. Assets/Plugins/Rx
* Move the System.Threading.dll to the Assets folder, e.g. Assets/Plugins/TaskParallelLibrary
* Copy the Assets/Plugins/RxUnity folder to your own project.

## Usage

Create a custom observable based on Unity's update loop:

    var update = UnityObservable.EveryUpdate<float>(observer => observer.OnNext(Time.deltaTime))
    update.Subscribe(delta => Debug.Log(delta));

Or use one of the readily available Observables:

    var mouse = UnityInputObservables.MouseMovement();
	var rays = UnityObservables.Rays(mouse, camera);
	rays.Subscribe(ray => Debug.Log(ray));