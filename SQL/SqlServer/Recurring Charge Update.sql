USE ProfitWise;
GO



DELETE FROM profitwiserecurringcharge WHERE PwShopId = 100001;

UPDATE profitwiseshop SET TempFreeTrialOverride = 1000 WHERE PwShopId = 100001;


SELECT * FROM profitwiserecurringcharge WHERE PwShopId = 100001;

SELECT * FROM profitwiseshop WHERE PwShopId = 100001;
SELECT * FROM vw_profitwiseshop WHERE PwShopId = 100001



DELETE FROM profitwiserecurringcharge WHERE PwShopId = 100001;


{ "id": 1319862372,   
"name": "ProfitWise Monthly Charge",    
 "api_client_id": "1564907",    
 "price": 9.95,   
 "status": "active",    
 "return_url": "https://gracie2/ProfitWise/ShopifyAuth/VerifyBilling",    
 "billing_on": "2018-10-10T00:00:00",    "created_at": "2018-10-10T16:44:40-05:00",    "updated_at": "2018-10-10T16:44:50-05:00",    "test": true,    "activated_on": "2018-10-10T00:00:00",    "trial_ends_on": "2018-10-10T00:00:00",    "cancelled_on": null,    "trial_days": 0,    "decorated_return_url": "https://gracie2/ProfitWise/ShopifyAuth/VerifyBilling?charge_id=1319862372",    "confirmation_url": null  }


WHERE IsPrimary = 1 AND
PwShopId IN (
	SELECT t2.PwShopId
	FROM AspNetUsers t1 INNER JOIN 
		profitwiseshop t2 ON 
			t1.Id = t2.ShopOwnerUserId
	WHERE Email IN 
	( 'thespraysource@gmail.com',
	'capsonline@cheerathletics.com',
	'appstoretest112@gmail.com',
	'smorgan7095@me.com',
	'info@culture-edit.com',
	'cocobrookside@gmail.com',
	'ben@highlandpartners.org',
	'monica.cordon@shopify.com',
	'bouchecosmetics55@gmail.com',
	'support@banksjournal.com',
	'contact@georgesofdubai.com',
	'michaela@shopfebras.com' ) );



SELECT * FROM profitwiserecurringcharge
WHERE IsPrimary = 1 AND
PwShopId IN (
	SELECT t2.PwShopId
	FROM AspNetUsers t1 INNER JOIN 
		profitwiseshop t2 ON 
			t1.Id = t2.ShopOwnerUserId
	WHERE Email IN 
	( 'thespraysource@gmail.com',
	'capsonline@cheerathletics.com',
	'appstoretest112@gmail.com',
	'smorgan7095@me.com',
	'info@culture-edit.com',
	'cocobrookside@gmail.com',
	'ben@highlandpartners.org',
	'monica.cordon@shopify.com',
	'bouchecosmetics55@gmail.com',
	'support@banksjournal.com',
	'contact@georgesofdubai.com',
	'michaela@shopfebras.com' ) )

