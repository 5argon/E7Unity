# DigitSprite 

Use each `Image` or `SpriteRenderer` as a single digit. You provide `Sprite` of number 0 to 9 to this and it will change the sprite of `Image`/`SpriteRenderer` for you. 

Attach `DigitSpriteEach` on each digit individually and connect the required components. To arrage digits, use things like Unity's `HorizontalLayoutGroup` because when the amount of digit changes it will be inactive and the layout will adjust automatically.

## Benefits

- `Text` often breaks batching. With this you can pack all digits into your sprite atlas for a potential batching opportunity. (And add some strokes, etc. for free)
- You can change color with the provided method.
- Has a method to fire a SetTrigger to all digits. You can do effects like damage number bounce that is bouncing separately digit by digit like in Final Fantasy V.