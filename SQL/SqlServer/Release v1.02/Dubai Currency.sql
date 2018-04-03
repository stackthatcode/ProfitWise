USE ProfitWise;
GO


DELETE FROM currency WHERE CurrencyId = 8;

INSERT INTO currency VALUES (8, 'AED', 'AED', 'Arab Emirate Dirham')


DELETE FROM exchangerate;

-- NEXT STEP - import CSV file with new exchange rate data

SELECT MAX(DATE) FROM  exchangerate;

UPDATE systemstate SET ExchangeRateLastDate = '2018-03-28';

SELECT * FROM systemstate;

