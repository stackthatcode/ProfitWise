USE ProfitWise;
GO


ALTER TABLE profitwiserecurringcharge ADD MustDestroyOnNextLogin bit;

UPDATE profitwiserecurringcharge SET MustDestroyOnNextLogin = 1;


-- ADDENDUM - Run this on 5/19/2018
UPDATE profitwiserecurringcharge SET MustDestroyOnNextLogin = 0 WHERE PWShopId = 100099;

-- ADDENDUM - Run this on 6/1/2018
UPDATE profitwiserecurringcharge SET MustDestroyOnNextLogin = 0 WHERE PWShopId = 100102;

