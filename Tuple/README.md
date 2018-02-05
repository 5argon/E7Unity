# Tuple

A Tuple that looks like C#'s `Tuple<>`, makes the transition easy if you can't use C#'s right now.
For example, Firebase's SDK stole our precious C#'s native Tuple class and use it internally, now we can't use it. (double declaration)

The namespace will be `System.CustomTuple` since if we use just System then it will be a collision. When migrating, just remove the namespace reference so it falls back to just `System` ones.

Feature-wise it sucks, but at least the `Item*` syntax matches.