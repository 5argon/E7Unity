# LocalizeEntryToPrompt

Copy a ready-made translation prompt to the clipboard, to paste into an external LLM.

Right-click the header of any **Localize String Event** component — or use its context menu — and pick one of three
entries. Each builds a prompt describing the string, copies it to the clipboard, and logs what it did.

| Menu item | What the prompt asks for |
| --- | --- |
| Copy Translation Filling Prompt | Fill in the languages that are still empty, using the existing ones as reference. |
| Copy Translation Review Prompt | Leave nothing to fill; review the existing translations against each other. |
| Copy Translation Review Prompt (Entire Hierarchy) | Review every localized string on screen together, as a set. |

## What goes into the prompt

The hard part of translating a UI string is that the words alone do not say where they appear. The prompt supplies
that context in two ways.

First, the game object hierarchy leading to the component, with every other branch collapsed, and the game object
holding the component marked with `[[double brackets]]`. A legend explains the marking. This leans on descriptive
naming, since names are all the prompt has to work with.

Second, every language that already has a value for the entry. Those existing translations are presented as
references to work from rather than as material to rewrite.

The prompt then asks for several candidate translations in descending order of certainty, each with its reasoning,
on the grounds that a prompt holding one screen's worth of context cannot be sure which reading is right and should
offer the choice rather than guess silently.

## Filling versus reviewing

**Filling** refuses to run when no language has a value at all, because the whole prompt is built around using a
trusted existing translation as the reference. With nothing to reference there is nothing to fill from. It also
declines when there is nothing missing.

It does report problems in the existing translations, but only outright grammatical mistakes it is completely certain
about — a typo, a wrong conjugation, a broken particle. Anything short of certain is left alone, so a filling pass
never turns into an unrequested rewrite.

**Reviewing** skips the filling entirely and gives feedback on each existing translation, judged against the others
and against the context.

## Reviewing an entire hierarchy

The third item ignores which component was clicked and sweeps everything. Inside Prefab Mode it covers that prefab;
otherwise it covers the whole active scene. The log line says which of the two it settled on.

Every localized game object is marked in the tree instead of just one, and the strings are laid out as a Markdown
table with the game object down the first column and one column per locale. A matrix makes the goal legible at a
glance: everything in a column should sound like one voice, and everything in a row should mean the same thing.

Feedback from this one is asked to stay at the level of the screen as a whole — consistency of terminology, register
and length — rather than drilling into individual word choices the way the single-entry prompts do.

## Requirements

Editor only, and compiled only when the Unity Localization package is installed, guarded by the `E7UNITY_LOCALIZATION`
define. A project without that package does not get the menu items.
