# MouseTouch

Get a `Touch` variable even if you are using a mouse. Useful for making a code that works on both editor, standalone, and real touch device while only check for `Touch`.

It will fake a `Touch` with `fingerId` -1 and a `phase` depending on mouse's movement.