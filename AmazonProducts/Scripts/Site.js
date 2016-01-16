$('.pagination a').on('click', function (evt) {
    evt.preventDefault();
    var newPage = $(this).data('page') - 1;
    var currentPage = $('.page').index('.page.active');

    if (newPage != currentPage) {
        $('.page.active').removeClass('active');
        $('.page').eq(newPage).addClass('active');

        $('.pagination li').removeClass('active');
        $(this).parent().addClass('active');
    }
});

$('[data-currencyCode]').on('click', function () {
    var $this = $(this);
    var code = $this.data('currencycode');
    lastCurrency = currency;
    currency = code;

    var params = {
        fromCurrency: lastCurrency,
        toCurrency: currency
    };
    $.getJSON('/Home/GetCurrencyRate', params, function (result) {
        if (result.From == lastCurrency && result.To == currency) {
            $('[data-currencyCode]').removeClass('btn-primary').addClass('btn-default');
            $this.removeClass('btn-default').addClass('btn-primary');

            $('.price').each(function () {
                var price = $(this).text();
                var newPrice = (price * result.Rate).toFixed(2);
                $(this).text(newPrice);
            });
        }
    });
});