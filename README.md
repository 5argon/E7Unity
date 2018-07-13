# E7Unity - A collection of Unity things that Exceed7 Experiments is using.

Free Unity stuff! But don't underestimate them because I have this as a UPM package on both of my real game projects. They are **all** actively being used and are very crucial (to me). Maybe someone else needs it, so I decided to share it for free.

This is previously submodule based, but the new era is on npm-based Unity Package Manager! So I have transformed this in th UPM-based repo. (That means I have committed all the `.meta` files)

**The license for everything here is MIT.** This repo will keeps growing over time.

I am not taking any responsibility for any problem caused by this, if there are bugs, they don't compile, or causes any revenue loss to your game!

There is a `README.md` file in most folders.

## Assembly Definition Files

E7Unity is segmented into `E7.Unity` and `E7.Unity.Editor`. Reference those to use it.

There is a reference to **Odin Inspector** in those `.asmdef` because some scripts allows nice drawing by Odin!
But in the script there is an additional `#if ODIN_INSPECTOR` check. So if you don't have Odin you can safely remove the references from `.asmdef`.

## Some scripts has been separated into their own repo

- [OdinHierarchy](https://github.com/5argon/OdinHierarchy)
- [ProtobufUnity](https://github.com/5argon/protobuf-unity)
- [E7ECS](https://github.com/5argon/E7ECS)