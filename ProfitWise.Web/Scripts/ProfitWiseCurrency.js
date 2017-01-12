﻿// Currency Functions
var ProfitWiseFunctions = ProfitWiseFunctions || {};

ProfitWiseFunctions.CurrencyCache = [
    { Id: 1, Abbr: "USD", Symbol: "$" },
    { Id: 2, Abbr: "EUR", Symbol: "€" },
    { Id: 3, Abbr: "JPY", Symbol: "¥" },
    { Id: 4, Abbr: "GBP", Symbol: "£" },
    { Id: 5, Abbr: "AUD", Symbol: "$" },
    { Id: 6, Abbr: "CHF", Symbol: "Fr" },
    { Id: 7, Abbr: "CAD", Symbol: "$" },
];

ProfitWiseFunctions.FormatCurrencyWithAbbr = function (amount, currencyId) {
    var item =
        AQ(ProfitWiseFunctions.CurrencyCache)
            .firstOrDefault(function (item) { return item.Id == currencyId; });

    if (!item) {
        throw "Unable to locate Currency for Id: " + currencyId;
    }
    return item.Symbol + numeral(amount).format("0,0.00") + " " + item.Abbr;
};

ProfitWiseFunctions.FormatCurrency = function (amount, currencyId) {
    var item =
        AQ(ProfitWiseFunctions.CurrencyCache)
            .firstOrDefault(function (item) { return item.Id == currencyId; });

    if (!item) {
        throw "Unable to locate Currency for Id: " + currencyId;
    }
    return item.Symbol + numeral(amount).format("0,0.00");
};

ProfitWiseFunctions.UnformatCurrency = function (amount) {
    amount = amount || 0;
    amountAsString = typeof amount == "string" ? amount : amount.toString();
    return numeral().unformat(amountAsString);
};


