# MotionMario



## Installation
1. MediaPipe Unity Plugin [Download](https://github.com/homuler/MediaPipeUnityPlugin/releases/download/v0.12.0/MediaPipeUnity.0.12.0.unitypackage)

2. Add `MotionProxy.GetInstance().SetPoseLandmark(eventArgs.value);` into `OnPoseLandmarksOutput` method
```csharp
private void OnPoseLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs)
{
    MotionProxy.GetInstance().SetPoseLandmark(eventArgs.value);
    _poseLandmarksAnnotationController.DrawLater(eventArgs.value);
}
```

