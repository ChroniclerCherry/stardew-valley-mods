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
 
 *Essentially what happens is this:*
The offset + the time interval returns a random number between 0 and 1. This number will always be the same for each time interval.

E.g if the time interval is 3, that random number will be the same on the same save file for three days at a time, changing on day 4, then day 7

Changing the offset will produce a different random number, but using the same offset will always produce the same.

The lower/upper bounds is then used to check if that random number is in the specified range.

Example: For one save file, the offset is 50 and the interval is 7.
On the first 7 days, the random number results in an 0.3. It will stay 0.3 for the entire week, so for example a condition check of 0-0.5 would return true, as 0.3 is in that range

On the second week, the random number produced becomes 0.8, and stays for the entire week. The previous conditions would then become false for the entire week, as 0.8 is not between 0 and 0.5

On the same save file, an offset of 30 might produce 0.4 the first week and then 0.1 the second.
 
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
