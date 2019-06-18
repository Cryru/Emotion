# Coroutines (Adfectus.Game.Time.Routines)

_Last Updated: Version 0.0.15_

Coroutines allow you to execute an action over a length of time - instead of at once. To create one, first create an instance of the `CoroutineManager` class and start calling its `Update` function in your scene's Update. Afterward invoke the `StartCoroutine()` function by providing a function which will be the routine logic. 

An example for such a function would be:

```
public IEnumerator Routine() {
    DoStuff();
    return null;
}
```

This example function will be executed at once, but we can use objects which implement the `IRoutineWaiter` interface to wait for actions. For instance:

```
public IEnumerator RoutineTime() {
    DoStuff();
    yield return new WaitForSeconds(1);
    DoStuff();
}
```

This function will execute the two calls to `DoStuff` a second apart from each other.
The `WaitForSeconds` class comes included, and you can implement your own. Returning null instead of such an object will cause the routine to wait for one tick.

## References

- [Coroutine Tests](https://github.com/Cryru/Emotion/blob/master/Adfectus.Tests/Coroutines.cs)
- [Adfectus.Game.Time.Routines Namespace](https://github.com/Cryru/Emotion/tree/master/Adfectus/Game/Time/Routines)