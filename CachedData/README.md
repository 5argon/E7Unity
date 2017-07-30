# CachedData

The DIY cache-able data structure! The class actually cannot automatically cache anything, you must do it yourself by setting its Data accessor, lol.

Currently responsible for huge performance boost in Gameplay Scene of Mel Cadence. If lane does not change, any other things read lane's data from CachedData. If it changes, each CachedData gets the SetDirty(), and my other script will update the Data before anyone can get it again. (If someone did get it while it's dirty, an exception will be thrown.)