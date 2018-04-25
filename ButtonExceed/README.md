# ButtonExceed

Unity's `Button` the way I like it!

- Separated `Down` and `Up` action. Usually you might want it to sound when down but take action on up. 
- No `Highlighted` anything on the inspector. (I am making mainly a mobile game.)
- No `Navigation` anything on the inspector.
- No need for `Animator` for each button. I think it was an overkill. Uses just a simple legacy animation, which I have an another component to make it semi stateful like `Animator`.
- Every removed features is in fact overridden with my own custom class/struct. The old one is there since I inherited from `Selectable`.

## Requirement

`LegacyAnimator` (It is in the other folder)