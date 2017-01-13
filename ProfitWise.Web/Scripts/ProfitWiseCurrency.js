// Currency Functions
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

ProfitWiseFunctions.ExtractRawNumber = function (amount) {
    amount = amount || 0;
    amountAsString = typeof amount == "string" ? amount : amount.toString();
    return numeral().unformat(amountAsString);
};

ProfitWiseFunctions.MakeKnockoutNumberOnlyInterceptor
    = function (knockoutValue, lowConstraint, highConstraint) {
        return ko.computed({
            read: function() {
                // Here's the formatting injection point
                var numericValue = ProfitWiseFunctions.ExtractRawNumber(ko.unwrap(knockoutValue));
                numericValue =
                    (lowConstraint && numericValue < lowConstraint)
                        ? lowConstraint
                        : numericValue;
                numericValue =
                    (highConstraint && numericValue > highConstraint)
                        ? highConstraint
                        : numericValue;

                var output = numeral(numericValue).format('0,0.00');
                return output;
            },
            write: function(newValue) {
                if ($.trim(newValue) == '') {
                    knockoutValue("0");
                } else {
                    knockoutValue(numeral().unformat(newValue));
                }
                knockoutValue.valueHasMutated();
            }
        }).extend({ notify: 'always' });
    };


