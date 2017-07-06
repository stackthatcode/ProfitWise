USE ProfitWise
GO

ALTER TABLE profitwiseproduct
ALTER COLUMN Title NVARCHAR(255);

ALTER TABLE profitwiseproduct
ALTER COLUMN Vendor NVARCHAR(255);

ALTER TABLE profitwiseproduct
ALTER COLUMN ProductType NVARCHAR(255);

ALTER TABLE profitwisevariant
ALTER COLUMN Title NVARCHAR(255);

ALTER TABLE profitwisevariant
ALTER COLUMN Sku NVARCHAR(255);



ALTER TABLE profitwiseprofitquerystub
ALTER COLUMN ProductTitle NVARCHAR(255);

ALTER TABLE profitwiseprofitquerystub
ALTER COLUMN VariantTitle NVARCHAR(255);

ALTER TABLE profitwiseprofitquerystub
ALTER COLUMN Vendor NVARCHAR(255);

ALTER TABLE profitwiseprofitquerystub
ALTER COLUMN ProductType NVARCHAR(255);

ALTER TABLE profitwiseprofitquerystub
ALTER COLUMN Sku NVARCHAR(255);



ALTER TABLE profitwisegoodsonhandquerystub
ALTER COLUMN ProductTitle NVARCHAR(255);

ALTER TABLE profitwisegoodsonhandquerystub
ALTER COLUMN VariantTitle NVARCHAR(255);

ALTER TABLE profitwisegoodsonhandquerystub
ALTER COLUMN Vendor NVARCHAR(255);

ALTER TABLE profitwisegoodsonhandquerystub
ALTER COLUMN ProductType NVARCHAR(255);

ALTER TABLE profitwisegoodsonhandquerystub
ALTER COLUMN Sku NVARCHAR(255);



