# FirebaseToolkit

Make your own Firebase client by subclassing this one.

It does not work if you use the experimental .NET 4.6, since Firebase team decided to bundle `System.Threading.Task` in their SDK (for compatibility with .NET 3.5) The result is you cannot use `Task` in your 4.6 program. if you build a wrapper and do `using System.Threading.Task` again you will get double declaration compile error since it is the same name as official ones. But you cannot remove the Firebase one since they compiled their SDK with that one. Firebase SDK cannot refer to .NET 4.6's `Task`.

I have e-mailed them but they said they cannot promise when will this be resolved.

There's no way to fix this untill they use different name for their own version of `Task`.