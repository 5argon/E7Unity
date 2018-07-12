# ScriptPostprocessor

For use with script generators. Do things when it see any script that you specified in `ScriptPostprocessor.asset` getting imported. Put that asset file somewhere in the system.

1. Create the config file in Create right click menu. Name it "ScriptPostprocessor" Fill out necessary information.
2. When some file you specified changes, it will search for lines and add something around it.
3. The first line will be added a special comment, so that the file won't be recursively postprocessed again.
4. When your script generators regenerate the script again it would detect that again.
