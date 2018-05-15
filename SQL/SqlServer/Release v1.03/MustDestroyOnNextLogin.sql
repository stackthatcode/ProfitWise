USE ProfitWise;
GO


ALTER TABLE profitwiserecurringcharge ADD MustDestroyOnNextLogin bit;

UPDATE profitwiserecurringcharge SET MustDestroyOnNextLogin = 1;

