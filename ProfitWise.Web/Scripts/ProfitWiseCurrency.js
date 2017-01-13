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


// Parameters  { knockoutValue, lowConstraint, highConstraint }
ProfitWiseFunctions.MakeKnockoutNumberOnlyInterceptor =
    function (parameters) {
        return ko.computed({
            read: function () {
                // Executed after user input
                var numericValue = ProfitWiseFunctions.ExtractRawNumber(ko.unwrap(parameters.knockoutValue));

                numericValue =
                    (typeof parameters.lowConstraint !== "undefined" && numericValue < parameters.lowConstraint)
                        ? parameters.lowConstraint
                        : numericValue;
                numericValue =
                    (typeof parameters.highConstraint !== "undefined" && numericValue > parameters.highConstraint)
                        ? parameters.highConstraint
                        : numericValue;
                
                var output = numeral(numericValue).format('0,0.00');
                return output;
            },
            write: function (newValue) {
                // Executed before user input
                if ($.trim(newValue) == '') {
                    parameters.knockoutValue("0");
                } else {
                    parameters.knockoutValue(numeral().unformat(newValue));
                }
                parameters.knockoutValue.valueHasMutated();
            }
        }).extend({ notify: 'always' });
    };


