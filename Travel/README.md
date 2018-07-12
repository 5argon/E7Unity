# Travel

A `Travel` data structure is a record of events that has **position** and **time**. The analogy is like a diary that records "I arrived at this position at this time".

Each event is called a `TravelEvent`. The property is that they connects to the next one sequentially. As you add events to the `Travel` they are automatically connected. When adding, you specify its position and **elapsed time** from the latest one. (must be positive)

Time and position together can infer velocity. If the time increase but position stays the same that could means there is no movement. If the time increase but position moves very little it could be that the velocity is low.

After you have added all the events, there are many operations that you could efficiently do to find your data on the Travel more efficiently and easily than storing your data on the `List` and do a binary search.

And you don't have to add position and time to your data by yourself, just state it while adding your data and it will be there.

The questions are like :

1. What is the most recent event that happen before this time/this position? How far is it from the current time/position?
2. Search for an event that satisfies a predicate, but start the search from this time/position.

PS. It uses C#6.0 syntax. I made it mainly for myself so I stopped caring about others using .NET 3.5 ...