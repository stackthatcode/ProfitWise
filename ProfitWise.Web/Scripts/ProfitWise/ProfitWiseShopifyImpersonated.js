var ProfitWiseShopify = ProfitWiseShopify || {};
var ProfitWiseConfig = ProfitWiseConfig || {};

ProfitWiseConfig.BaseUrl = '/ProfitWise';

ProfitWiseShopify.LaunchModal = function (settings, callback) {
    $('.modal').modal({ show: true });
    $('.modal').on('shown.bs.modal', function () {
        //correct here use 'shown.bs.modal' event which comes in bootstrap3
        $(this).find('iframe').attr('src', settings.src);
    });

    $('.modal').on('hidden.bs.modal',
        function() {
            callback();
        });
}

//ProfitWiseShopify.LaunchModal({
//    src: url,
//    title: 'Variant Consolidation with ' + item.SkuTitleText,
//    width: 'large',
//    height: 500,
//}, self.Refresh);


ProfitWiseShopify.ErrorMessage =
    "We're sorry for the inconvenience, but the System has encountered an error. " +
    "Please try reloading the page. If the problem persists, reach out to our Support Team!";

ProfitWiseShopify.ErrorPopup = function () {
    alert(ProfitWiseShopify.ErrorMessage);
};

