# TextFlatten

## Text draw call problems
- If you put a text in the middle of Unity UI hierarchy-wise (it is usually that way since you need the anchoring) it breaks batching because UI shader depends on sibling index and the text won't have the same texture as UI.
- If you use a custom shader for text that has higher rendering queue in an attempt to make it render later after all the UIs, it will still break batching **even though they are drawn later**. (In Frame Debugger it will be a mysterious break, where 2 probably batchable UI draw call has been separated with **nothing**)
- If you make a sub-Canvas for the text and override draw order to be higher, the text will be successfully separate from the UI and the remaining UI can batch. But the batching among the separated texts cannot be batched even with the same override draw order number. (Because they are technially from different canvases)
- Assuming that they can batch, the order will be according to top-to-bottom sibling index merged together if you have multiple such sub-Canvas for different text. This potentially breaks batching in between the texts if you have a different font and set the same order.

## Solution

`TextFlatten` can gather all `Text` that you want and group them together in the same common sibling to enable batching. You can order them as you like regardless of their original parent and sibling indexes.

- Put all the `Text` to be flatten to `TextFlatten`'s `Text` array! You can order them and they will be ordered this way after flattened.
- Call `Flatten()` and the `Text` will be immediately put out of any parent they were and will be ordered linearly as last siblings on any `Transform` of your choice. Remember that you should not move the original parent after flattened.
- Call `Restore()` if you want to move the original parent. They will be put back to their original spot. (For example call before doing a scene transition out)

## Tricks

You can have the "Transform of your choice" as a sub-canvas so you can control it's draw order directly.