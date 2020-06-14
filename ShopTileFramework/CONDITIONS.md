# Synchronized randomness
 
 SeededRandom is an available condition that can be useful for synchronizing randomness across different stocks, shops, for extended time periods, but can be quite complex.
 
 The structure of the condition looks like this:
 
 `SeededRandom <i:offset> <i:timeInterval/s:timeInterval> <f:random lower bounds> <f: random upper bounds>`
 
 Broken down:
 Parameter | Type | Description
 ---------|--------|--------
 offset | integer | Can be any number-- two conditions both using this number and the same timeInterval will receive the same random roll
 timeInterval | integer or a string | How long a seeded random roll lasts in days. This also supports entering certain strings as a shortcut.
 random lower/upper bounds | decimal number | This is the range the random number needs to be in to return a true
 
##### timeInterval strings

There's strings to provide a more readable shortcut for most use cases. If you want a different interval, for example every 3 days, then you'll be entering the timeInterval as an integer instead

* `Day` = 1, `Week` = 7, `Month` = `Season` = 28, `Year` = 112

* `Game` = 0, which will allow the same seed for the entire game regardless of time, allowing for save-game unique conditions

#### Examples

Here's an example of making two different stocks that change each week, and there's a 50/50 chance of one or the other being available each week for the whole week but not both. The first itemstock check for the range 0.5-1, while the second checks for 0-0.5, causing no overlap. Both checks are seeded with an offset of 22, keeping them synchronized
```js
{
  "Shops": [

  /// start of block for each item shop. You can have as many of these as you want
    {
      "ShopName": "Test",
      "ItemStocks": [
        {
          "ItemType": "Object",
          "ItemNames": ["Item1","Item2","Item4"],
          "When": [
            "SeededRandom 22 Week 0.5 1.0",
            ],
        },
        {
          "ItemType": "Object",
          "ItemNames": ["Item2","Item3","Item5"],
          "When": [
            "SeededRandom 22 Week 0 0.5",
            ],
        },
      ],
    },
  ]
}

```

More complex example setting different time ranges. The following combination of conditions will have a 25% chance of returning true for days 22-28, 25% chance of being true for the days 26-28, 25% chance of only being true on the 28th, and 25% chance of returning false for any of the days

```js
{
  "Shops": [

  /// start of block for each item shop. You can have as many of these as you want
    {
      "ShopName": "Test",
      "ItemStocks": [
        {
          "ItemType": "Object",
          "ItemNames": ["Item1","Item2","Item4"],
          "When": [
            "SeededRandom 22 Week 0.5 1.0",
            ],
        },
        {
          "ItemType": "Object",
          "ItemNames": ["Item2","Item3","Item5"],
          "When": [
            "u 22 23 24 25 26 27 28/SeededRandom 1 Season 0 0.25",
            "u 26 27 28/SeededRandom 1 Season 0.25 0.50",
            "u 28/SeededRandom 1 Season 0.50 0.75"
            ],
        },
      ],
    },
  ]
}

```
