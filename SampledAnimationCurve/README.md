# SampledAnimationCurve

An `AnimationCurve` that is a struct and can go into the C# Jobs. Works by calculating values beforehand and when you want an evaluate it uses `lerp` to compute when the time lands between samples.

You will have to `Dispose()` it as it contains `NativeArray<float>`.