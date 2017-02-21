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

ko.extenders.numeric = function (target, parameters) {
    //create a writable computed observable to intercept writes to our observable
    var result = ko.pureComputed({
        read: target,  //always return the original observables value
        write: function (newValue) {
            var current = target();
            var newValueAsNum = isNaN(newValue) ? 0 : +newValue;

            if (!isNaN(parameters.LowConstraint) && newValueAsNum < parameters.LowConstraint) {
                newValueAsNum = parameters.LowConstraint;
            }
            if (!isNaN(parameters.HighConstraint) && newValueAsNum > parameters.HighConstraint) {
                newValueAsNum = parameters.HighConstraint;
            }
            
            var valueToWrite = numeral(newValueAsNum || 0).format('0.00');

            //only write if it changed
            if (valueToWrite !== current) {
                target(valueToWrite);
            } else {
                //if the rounded value is the same, but a different value was written, force a notification for the current field
                if (newValue !== current) {
                    target.notifySubscribers(valueToWrite);
                }
            }
        }
    }).extend({ notify: 'always' });

    //initialize with current value to make sure it is rounded appropriately
    result(target());

    //return the new computed observable
    return result;
};

