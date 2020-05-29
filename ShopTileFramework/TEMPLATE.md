A template of the full `shops.json` format

Full description of each field can be found in the README

```js
{
  "RemovePacksFromVanilla": [
        "JAPack.UniqueID1",
        "JAPack.UniqueID2"
      ],
  "Shops": [

  /// start of block for each item shop. You can have as many of these as you want
    {
      "ShopName": "ShopName1",
      "StoreCurrency": "Money",
      "CategoriesToSellHere":[ 
        -4,
        -5,
      ],
      "PortraitPath": "PortraitPath1",
      "Quote": "Quote1",
      "ShopPrice": -1,
      "MaxNumItemsSoldInStore": 2147483647,
      "DefaultSellPriceMultipler": 1.0,
      "PriceMultiplierWhen": {
        "0.5": [
          "condition1",
          "condition1"
        ],
        "2": [
          "condition3",
          "condition4"
        ]
      },
      "ItemStocks": [

      ///start of block for each itemstock. Each shop can have as many of these as you want
        {
          "ItemType": "ItemType1",
          "IsRecipe": false,
          "StockPrice": -1,
          "StockItemCurrency": "Money",
          "StockCurrencyStack": 1,
          "Quality": 1,
          "ItemIDs": [1,2,3],
          "JAPacks":  ["JAPack.UniqueID1","JAPack.UniqueID2"],
          "ItemNames": ["Item1","Item2"],
          "Stock": 2147483647,
          "MaxNumItemsSoldInItemStock": 2147483647,
          "When": [
            "condition1",
            "condition2"
            ],
        },
       ///end of block for each itemstock.

      ],
      "When": [
        "condition1",
        "condition2"
      ],
      "ClosedMessage": "ClosedMessage1",
      "LocalizedQuote": { "zh": "你好，世界",
                           "fr" : "bonjour"},
      "LocalizedClosedMessage": { "zh": "你好，世界",
                           "fr" : "bonjour"},
    },
  /// end of block for each item shop.


  ],
  "AnimalShops": [

  ///start of block for each animal shop. You can have multiples of these
    {
      "ShopName": "ShopName1",
      "AnimalStock":  [
        "Animal1",
        "Animal12"
      ],
      "ExcludeFromMarnies": [
        "Animal3",
        "Animal14"
      ],
      "When": [
        "condition1",
        "condition2"
      ],
      "ClosedMessage": "ClosedMessage1",
      "LocalizedClosedMessage": { "zh": "你好，世界",
                           "fr" : "bonjour"},
    },
  ///end of block for each animal shop.

  ],
  "VanillaShops": [

    ///start of block for each vanilla shop. You can have one for each vanilla shop supported by STF
    {
      "ShopName": "ShopName1",
      "ReplaceInsteadOfAdd": false,
      "ShopPrice": -1,
      "MaxNumItemsSoldInStore": 2147483647,
      "ItemStocks": [

      ///start of block for each itemstock. Each shop can have as many of these as you want
        {
          "ItemType": "ItemType1",
          "IsRecipe": false,
          "StockPrice": -1,
          "StockItemCurrency": "Money",
          "StockCurrencyStack": 1,
          "Quality": 1,
          "ItemIDs": [1,2,3],
          "JAPacks":  ["JAPack.UniqueID1","JAPack.UniqueID2"],
          "ItemNames": ["Item1","Item2"],
          "Stock": 2147483647,
          "MaxNumItemsSoldInItemStock": 2147483647,
          "When":  [
            "condition1",
            "condition2"
          ],
        },
       ///end of block for each itemstock.
      ]
    },
    ///end of block for each vanilla shop

  ]
}

```