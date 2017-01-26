USE [ProfitWise]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserRoles] DROP CONSTRAINT IF EXISTS [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserRoles] DROP CONSTRAINT IF EXISTS [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserLogins]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserLogins] DROP CONSTRAINT IF EXISTS [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserClaims]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserClaims] DROP CONSTRAINT IF EXISTS [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[systemstate]') AND type in (N'U'))
ALTER TABLE [dbo].[systemstate] DROP CONSTRAINT IF EXISTS [DF__systemsta__Excha__3C34F16F]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyvariant]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyvariant] DROP CONSTRAINT IF EXISTS [DF__shopifyva__Price__3B40CD36]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyvariant]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyvariant] DROP CONSTRAINT IF EXISTS [DF__shopifyva__Title__3A4CA8FD]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyvariant]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyvariant] DROP CONSTRAINT IF EXISTS [DF__shopifyvari__Sku__395884C4]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyproduct]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyproduct] DROP CONSTRAINT IF EXISTS [DF__shopifypr__Title__3864608B]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorderrefund]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyorderrefund] DROP CONSTRAINT IF EXISTS [DF__shopifyor__Amoun__37703C52]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorderlineitem]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyorderlineitem] DROP CONSTRAINT IF EXISTS [DF__shopifyor__Total__367C1819]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorderlineitem]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyorderlineitem] DROP CONSTRAINT IF EXISTS [DF__shopifyor__Total__3587F3E0]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorderlineitem]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyorderlineitem] DROP CONSTRAINT IF EXISTS [DF__shopifyor__UnitP__3493CFA7]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorderlineitem]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyorderlineitem] DROP CONSTRAINT IF EXISTS [DF__shopifyor__Order__339FAB6E]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorderadjustment]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyorderadjustment] DROP CONSTRAINT IF EXISTS [DF__shopifyor__Reaso__32AB8735]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorderadjustment]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyorderadjustment] DROP CONSTRAINT IF EXISTS [DF__shopifyord__Kind__31B762FC]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorderadjustment]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyorderadjustment] DROP CONSTRAINT IF EXISTS [DF__shopifyor__TaxAm__30C33EC3]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorderadjustment]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyorderadjustment] DROP CONSTRAINT IF EXISTS [DF__shopifyor__Amoun__2FCF1A8A]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorder]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyorder] DROP CONSTRAINT IF EXISTS [DF__shopifyor__Updat__2EDAF651]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorder]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyorder] DROP CONSTRAINT IF EXISTS [DF__shopifyor__Creat__2DE6D218]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorder]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyorder] DROP CONSTRAINT IF EXISTS [DF__shopifyord__Tags__2CF2ADDF]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorder]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyorder] DROP CONSTRAINT IF EXISTS [DF__shopifyor__Order__2BFE89A6]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorder]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyorder] DROP CONSTRAINT IF EXISTS [DF__shopifyor__Order__2B0A656D]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorder]') AND type in (N'U'))
ALTER TABLE [dbo].[shopifyorder] DROP CONSTRAINT IF EXISTS [DF__shopifyor__Email__2A164134]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisevariant]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisevariant] DROP CONSTRAINT IF EXISTS [DF__profitwis__LastU__29221CFB]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisevariant]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisevariant] DROP CONSTRAINT IF EXISTS [DF__profitwis__Inven__282DF8C2]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisevariant]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisevariant] DROP CONSTRAINT IF EXISTS [DF__profitwis__Title__2739D489]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisevariant]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisevariant] DROP CONSTRAINT IF EXISTS [DF__profitwisev__Sku__2645B050]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisevariant]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisevariant] DROP CONSTRAINT IF EXISTS [DF__profitwis__Shopi__25518C17]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisevariant]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisevariant] DROP CONSTRAINT IF EXISTS [DF__profitwis__Shopi__245D67DE]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseshop]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwiseshop] DROP CONSTRAINT IF EXISTS [DF__profitwis__Start__236943A5]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseshop]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwiseshop] DROP CONSTRAINT IF EXISTS [DF__profitwis__TimeZ__22751F6C]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseshop]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwiseshop] DROP CONSTRAINT IF EXISTS [DF__profitwis__Curre__2180FB33]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseshop]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwiseshop] DROP CONSTRAINT IF EXISTS [DF__profitwis__Shopi__208CD6FA]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereportquerystub]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisereportquerystub] DROP CONSTRAINT IF EXISTS [DF__profitwis__Varia__1F98B2C1]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereportquerystub]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisereportquerystub] DROP CONSTRAINT IF EXISTS [DF__profitwiser__Sku__1EA48E88]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereportquerystub]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisereportquerystub] DROP CONSTRAINT IF EXISTS [DF__profitwis__Produ__1DB06A4F]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereportquerystub]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisereportquerystub] DROP CONSTRAINT IF EXISTS [DF__profitwis__Produ__1CBC4616]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereportquerystub]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisereportquerystub] DROP CONSTRAINT IF EXISTS [DF__profitwis__Vendo__1BC821DD]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereportfilter]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisereportfilter] DROP CONSTRAINT IF EXISTS [DF__profitwis__Descr__1AD3FDA4]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereportfilter]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisereportfilter] DROP CONSTRAINT IF EXISTS [DF__profitwis__Title__19DFD96B]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereportfilter]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisereportfilter] DROP CONSTRAINT IF EXISTS [DF__profitwis__Strin__18EBB532]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereportfilter]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisereportfilter] DROP CONSTRAINT IF EXISTS [DF__profitwis__Numbe__17F790F9]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereport]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisereport] DROP CONSTRAINT IF EXISTS [DF__profitwis__LastA__17036CC0]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereport]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisereport] DROP CONSTRAINT IF EXISTS [DF__profitwis__Creat__160F4887]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereport]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisereport] DROP CONSTRAINT IF EXISTS [DF__profitwis__Order__151B244E]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereport]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisereport] DROP CONSTRAINT IF EXISTS [DF__profitwis__Group__14270015]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereport]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisereport] DROP CONSTRAINT IF EXISTS [DF__profitwise__Name__1332DBDC]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseprofitreportentry]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwiseprofitreportentry] DROP CONSTRAINT IF EXISTS [DF__profitwis__Quant__123EB7A3]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseprofitreportentry]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwiseprofitreportentry] DROP CONSTRAINT IF EXISTS [DF__profitwise__CoGS__114A936A]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseprofitreportentry]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwiseprofitreportentry] DROP CONSTRAINT IF EXISTS [DF__profitwis__NetSa__10566F31]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseprofitreportentry]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwiseprofitreportentry] DROP CONSTRAINT IF EXISTS [DF__profitwis__PwVar__0F624AF8]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseprofitreportentry]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwiseprofitreportentry] DROP CONSTRAINT IF EXISTS [DF__profitwis__PwPro__0E6E26BF]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseproduct]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwiseproduct] DROP CONSTRAINT IF EXISTS [DF__profitwis__LastU__0D7A0286]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseproduct]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwiseproduct] DROP CONSTRAINT IF EXISTS [DF__profitwis__Produ__0C85DE4D]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseproduct]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwiseproduct] DROP CONSTRAINT IF EXISTS [DF__profitwis__Vendo__0B91BA14]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseproduct]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwiseproduct] DROP CONSTRAINT IF EXISTS [DF__profitwis__Title__0A9D95DB]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseproduct]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwiseproduct] DROP CONSTRAINT IF EXISTS [DF__profitwis__Shopi__09A971A2]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisepicklist]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisepicklist] DROP CONSTRAINT IF EXISTS [DF__profitwis__Creat__08B54D69]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisemastervariantcogsdetail]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisemastervariantcogsdetail] DROP CONSTRAINT IF EXISTS [DF__profitwis__CogsM__07C12930]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisemastervariantcogsdetail]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisemastervariantcogsdetail] DROP CONSTRAINT IF EXISTS [DF__profitwis__CogsA__06CD04F7]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisemastervariantcogsdetail]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisemastervariantcogsdetail] DROP CONSTRAINT IF EXISTS [DF__profitwis__CogsC__05D8E0BE]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisemastervariant]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisemastervariant] DROP CONSTRAINT IF EXISTS [DF__profitwis__CogsM__04E4BC85]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisemastervariant]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisemastervariant] DROP CONSTRAINT IF EXISTS [DF__profitwis__CogsA__03F0984C]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisemastervariant]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisemastervariant] DROP CONSTRAINT IF EXISTS [DF__profitwis__CogsC__02FC7413]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisemastervariant]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisemastervariant] DROP CONSTRAINT IF EXISTS [DF__profitwis__CogsT__02084FDA]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisebatchstate]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisebatchstate] DROP CONSTRAINT IF EXISTS [DF__profitwis__Order__01142BA1]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisebatchstate]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisebatchstate] DROP CONSTRAINT IF EXISTS [DF__profitwis__Order__00200768]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisebatchstate]') AND type in (N'U'))
ALTER TABLE [dbo].[profitwisebatchstate] DROP CONSTRAINT IF EXISTS [DF__profitwis__Produ__7F2BE32F]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ints]') AND type in (N'U'))
ALTER TABLE [dbo].[ints] DROP CONSTRAINT IF EXISTS [DF__ints__i__7E37BEF6]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[calendar_table]') AND type in (N'U'))
ALTER TABLE [dbo].[calendar_table] DROP CONSTRAINT IF EXISTS [DF__calendar___isPay__7D439ABD]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[calendar_table]') AND type in (N'U'))
ALTER TABLE [dbo].[calendar_table] DROP CONSTRAINT IF EXISTS [DF__calendar___holid__7C4F7684]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[calendar_table]') AND type in (N'U'))
ALTER TABLE [dbo].[calendar_table] DROP CONSTRAINT IF EXISTS [DF__calendar___isHol__7B5B524B]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[calendar_table]') AND type in (N'U'))
ALTER TABLE [dbo].[calendar_table] DROP CONSTRAINT IF EXISTS [DF__calendar___isWee__7A672E12]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[calendar_table]') AND type in (N'U'))
ALTER TABLE [dbo].[calendar_table] DROP CONSTRAINT IF EXISTS [DF__calendar_tabl__w__797309D9]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[calendar_table]') AND type in (N'U'))
ALTER TABLE [dbo].[calendar_table] DROP CONSTRAINT IF EXISTS [DF__calendar___dayNa__787EE5A0]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[calendar_table]') AND type in (N'U'))
ALTER TABLE [dbo].[calendar_table] DROP CONSTRAINT IF EXISTS [DF__calendar___month__778AC167]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[calendar_table]') AND type in (N'U'))
ALTER TABLE [dbo].[calendar_table] DROP CONSTRAINT IF EXISTS [DF__calendar_tab__dw__76969D2E]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[calendar_table]') AND type in (N'U'))
ALTER TABLE [dbo].[calendar_table] DROP CONSTRAINT IF EXISTS [DF__calendar_tabl__d__75A278F5]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[calendar_table]') AND type in (N'U'))
ALTER TABLE [dbo].[calendar_table] DROP CONSTRAINT IF EXISTS [DF__calendar_tabl__m__74AE54BC]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[calendar_table]') AND type in (N'U'))
ALTER TABLE [dbo].[calendar_table] DROP CONSTRAINT IF EXISTS [DF__calendar_tabl__q__73BA3083]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[calendar_table]') AND type in (N'U'))
ALTER TABLE [dbo].[calendar_table] DROP CONSTRAINT IF EXISTS [DF__calendar_tabl__y__72C60C4A]
GO


/****** Object:  Table [dbo].[systemstate]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[systemstate]
GO
/****** Object:  Table [dbo].[shopifyvariant]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[shopifyvariant]
GO
/****** Object:  Table [dbo].[shopifyproduct]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[shopifyproduct]
GO
/****** Object:  Table [dbo].[shopifyorderrefund]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[shopifyorderrefund]
GO
/****** Object:  Table [dbo].[shopifyorderlineitem]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[shopifyorderlineitem]
GO
/****** Object:  Table [dbo].[shopifyorderadjustment]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[shopifyorderadjustment]
GO
/****** Object:  Table [dbo].[shopifyorder]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[shopifyorder]
GO
/****** Object:  Table [dbo].[shop]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[shop]
GO
/****** Object:  Table [dbo].[profitwisevariant]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[profitwisevariant]
GO
/****** Object:  Table [dbo].[profitwiseshop]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[profitwiseshop]
GO
/****** Object:  Table [dbo].[profitwisereportvendor]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[profitwisereportvendor]
GO
/****** Object:  Table [dbo].[profitwisereportsku]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[profitwisereportsku]
GO
/****** Object:  Table [dbo].[profitwisereportquerystub]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[profitwisereportquerystub]
GO
/****** Object:  Table [dbo].[profitwisereportproducttype]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[profitwisereportproducttype]
GO
/****** Object:  Table [dbo].[profitwisereportmasterproduct]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[profitwisereportmasterproduct]
GO
/****** Object:  Table [dbo].[profitwisereportfilter]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[profitwisereportfilter]
GO
/****** Object:  Table [dbo].[profitwisereport]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[profitwisereport]
GO
/****** Object:  Table [dbo].[profitwiseprofitreportentry]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[profitwiseprofitreportentry]
GO
/****** Object:  Table [dbo].[profitwiseproduct]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[profitwiseproduct]
GO
/****** Object:  Table [dbo].[profitwisepicklistmasterproduct]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[profitwisepicklistmasterproduct]
GO
/****** Object:  Table [dbo].[profitwisepicklist]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[profitwisepicklist]
GO
/****** Object:  Table [dbo].[profitwisemastervariantcogsdetail]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[profitwisemastervariantcogsdetail]
GO
/****** Object:  Table [dbo].[profitwisemastervariant]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[profitwisemastervariant]
GO
/****** Object:  Table [dbo].[profitwisemasterproduct]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[profitwisemasterproduct]
GO
/****** Object:  Table [dbo].[profitwisedatelookup]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[profitwisedatelookup]
GO
/****** Object:  Table [dbo].[profitwisebatchstate]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[profitwisebatchstate]
GO
/****** Object:  Table [dbo].[ints]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[ints]
GO
/****** Object:  Table [dbo].[exchangerate]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[exchangerate]
GO
/****** Object:  Table [dbo].[currency]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[currency]
GO
/****** Object:  Table [dbo].[calendar_table]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[calendar_table]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[AspNetUsers]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[AspNetUserRoles]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[AspNetUserLogins]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[AspNetUserClaims]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP TABLE IF EXISTS [dbo].[AspNetRoles]
GO
/****** Object:  View [dbo].[vw_reportvendortoproduct]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP VIEW IF EXISTS [dbo].[vw_reportvendortoproduct]
GO
/****** Object:  View [dbo].[vw_reportproducttypetoproduct]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP VIEW IF EXISTS [dbo].[vw_reportproducttypetoproduct]
GO
/****** Object:  View [dbo].[vw_reportmasterproducttomastervariant]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP VIEW IF EXISTS [dbo].[vw_reportmasterproducttomastervariant]
GO
/****** Object:  View [dbo].[vw_masterproductandvariantsearch]    Script Date: 1/26/2017 8:40:07 AM ******/
DROP VIEW IF EXISTS [dbo].[vw_masterproductandvariantsearch]
GO
/****** Object:  View [dbo].[vw_masterproductandvariantsearch]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_masterproductandvariantsearch]'))
EXEC dbo.sp_executesql @statement = N'/*
*   SSMA informational messages:
*   M2SS0003: The following SQL clause was ignored during conversion:
*   ALGORITHM =  UNDEFINED.
*   M2SS0003: The following SQL clause was ignored during conversion:
*   DEFINER = `skip-grants user`@`skip-grants host`.
*   M2SS0003: The following SQL clause was ignored during conversion:
*   SQL SECURITY DEFINER.
*/

CREATE VIEW [dbo].[vw_masterproductandvariantsearch] (
   [PwShopId], 
   [PwMasterProductId], 
   [ProductTitle], 
   [Vendor], 
   [ProductType], 
   [PwMasterVariantId], 
   [VariantTitle], 
   [Sku])
AS 
   SELECT 
      t1.PwShopId AS PwShopId, 
      t1.PwMasterProductId AS PwMasterProductId, 
      t1.Title AS ProductTitle, 
      t1.Vendor AS Vendor, 
      t1.ProductType AS ProductType, 
      t3.PwMasterVariantId AS PwMasterVariantId, 
      t3.Title AS VariantTitle, 
      t3.Sku AS Sku
   FROM ((profitwise.profitwiseproduct  AS t1 
      INNER JOIN profitwise.profitwisemastervariant  AS t2 
      ON ((t1.PwMasterProductId = t2.PwMasterProductId))) 
      INNER JOIN profitwise.profitwisevariant  AS t3 
      ON ((t2.PwMasterVariantId = t3.PwMasterVariantId)))
   WHERE ((t1.IsPrimary = 1) AND (t3.IsPrimary = 1))' 
GO
/****** Object:  View [dbo].[vw_reportmasterproducttomastervariant]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_reportmasterproducttomastervariant]'))
EXEC dbo.sp_executesql @statement = N'/*
*   SSMA informational messages:
*   M2SS0003: The following SQL clause was ignored during conversion:
*   ALGORITHM =  UNDEFINED.
*   M2SS0003: The following SQL clause was ignored during conversion:
*   DEFINER = `skip-grants user`@`skip-grants host`.
*   M2SS0003: The following SQL clause was ignored during conversion:
*   SQL SECURITY DEFINER.
*/

CREATE VIEW [dbo].[vw_reportmasterproducttomastervariant] (
   [PwShopId], 
   [PwReportId], 
   [PwMasterProductId], 
   [PwMasterVariantId])
AS 
   SELECT t1.PwShopId AS PwShopId, t1.PwReportId AS PwReportId, t1.PwMasterProductId AS PwMasterProductId, t2.PwMasterVariantId AS PwMasterVariantId
   FROM (profitwise.profitwisereportmasterproduct  AS t1 
      INNER JOIN profitwise.profitwisemastervariant  AS t2 
      ON (((t1.PwMasterProductId = t2.PwMasterProductId) AND (t1.PwShopId = t2.PwShopId))))' 
GO
/****** Object:  View [dbo].[vw_reportproducttypetoproduct]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_reportproducttypetoproduct]'))
EXEC dbo.sp_executesql @statement = N'/*
*   SSMA informational messages:
*   M2SS0003: The following SQL clause was ignored during conversion:
*   ALGORITHM =  UNDEFINED.
*   M2SS0003: The following SQL clause was ignored during conversion:
*   DEFINER = `skip-grants user`@`skip-grants host`.
*   M2SS0003: The following SQL clause was ignored during conversion:
*   SQL SECURITY DEFINER.
*/

CREATE VIEW [dbo].[vw_reportproducttypetoproduct] (
   [PwShopId], 
   [PwReportId], 
   [ProductType], 
   [PwProductId], 
   [Vendor])
AS 
   SELECT 
      t1.PwShopId AS PwShopId, 
      t1.PwReportId AS PwReportId, 
      t1.ProductType AS ProductType, 
      t2.PwProductId AS PwProductId, 
      t2.Vendor AS Vendor
   FROM (profitwise.profitwisereportproducttype  AS t1 
      INNER JOIN profitwise.profitwiseproduct  AS t2 
      ON (((t1.ProductType = t2.ProductType) AND (t1.PwShopId = t2.PwShopId))))' 
GO
/****** Object:  View [dbo].[vw_reportvendortoproduct]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_reportvendortoproduct]'))
EXEC dbo.sp_executesql @statement = N'/*
*   SSMA informational messages:
*   M2SS0003: The following SQL clause was ignored during conversion:
*   ALGORITHM =  UNDEFINED.
*   M2SS0003: The following SQL clause was ignored during conversion:
*   DEFINER = `skip-grants user`@`skip-grants host`.
*   M2SS0003: The following SQL clause was ignored during conversion:
*   SQL SECURITY DEFINER.
*/

CREATE VIEW [dbo].[vw_reportvendortoproduct] (
   [PwShopId], 
   [PwReportId], 
   [Vendor], 
   [PwProductId])
AS 
   SELECT t1.PwShopId AS PwShopId, t1.PwReportId AS PwReportId, t1.Vendor AS Vendor, t2.PwProductId AS PwProductId
   FROM (profitwise.profitwisereportvendor  AS t1 
      INNER JOIN profitwise.profitwiseproduct  AS t2 
      ON (((t1.Vendor = t2.Vendor) AND (t1.PwShopId = t2.PwShopId))))' 
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetRoles]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserClaims]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserLogins]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](128) NOT NULL,
	[Email] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEndDateUtc] [datetime] NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO


/****** Object:  Table [dbo].[calendar_table]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[calendar_table]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[calendar_table](
	[dt] [date] NOT NULL,
	[y] [smallint] NULL,
	[q] [smallint] NULL,
	[m] [smallint] NULL,
	[d] [smallint] NULL,
	[dw] [smallint] NULL,
	[monthName] [nvarchar](9) NULL,
	[dayName] [nvarchar](9) NULL,
	[w] [smallint] NULL,
	[isWeekday] [binary](1) NULL,
	[isHoliday] [binary](1) NULL,
	[holidayDescr] [nvarchar](32) NULL,
	[isPayday] [binary](1) NULL,
 CONSTRAINT [PK_calendar_table_dt] PRIMARY KEY CLUSTERED 
(
	[dt] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[currency]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[currency]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[currency](
	[CurrencyId] [int] NOT NULL,
	[Abbreviation] [nvarchar](3) NOT NULL,
	[Symbol] [nvarchar](3) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_currency_CurrencyId] PRIMARY KEY CLUSTERED 
(
	[CurrencyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[exchangerate]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[exchangerate]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[exchangerate](
	[SourceCurrencyId] [int] NOT NULL,
	[DestinationCurrencyId] [int] NOT NULL,
	[Date] [date] NOT NULL,
	[Rate] [decimal](9, 6) NOT NULL,
 CONSTRAINT [PK_exchangerate_SourceCurrencyId] PRIMARY KEY CLUSTERED 
(
	[SourceCurrencyId] ASC,
	[DestinationCurrencyId] ASC,
	[Date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[ints]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ints]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ints](
	[i] [smallint] NULL
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[profitwisebatchstate]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisebatchstate]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisebatchstate](
	[PwShopId] [bigint] NOT NULL,
	[ProductsLastUpdated] [datetime] NULL,
	[OrderDatasetStart] [datetime] NULL,
	[OrderDatasetEnd] [datetime] NULL,
 CONSTRAINT [PK_profitwisebatchstate_PwShopId] PRIMARY KEY CLUSTERED 
(
	[PwShopId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[profitwisedatelookup]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisedatelookup]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisedatelookup](
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
 CONSTRAINT [PK_profitwisedatelookup_StartDate] PRIMARY KEY CLUSTERED 
(
	[StartDate] ASC,
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[profitwisemasterproduct]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisemasterproduct]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisemasterproduct](
	[PwMasterProductId] [bigint] IDENTITY(201,1) NOT NULL,
	[PwShopId] [bigint] NOT NULL,
 CONSTRAINT [PK_profitwisemasterproduct_PwMasterProductId] PRIMARY KEY CLUSTERED 
(
	[PwMasterProductId] ASC,
	[PwShopId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[profitwisemastervariant]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisemastervariant]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisemastervariant](
	[PwMasterVariantId] [bigint] IDENTITY(872,1) NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[PwMasterProductId] [bigint] NOT NULL,
	[Exclude] [smallint] NOT NULL,
	[StockedDirectly] [smallint] NOT NULL,
	[CogsTypeId] [smallint] NULL,
	[CogsCurrencyId] [int] NULL,
	[CogsAmount] [decimal](15, 2) NULL,
	[CogsMarginPercent] [decimal](4, 2) NULL,
	[CogsDetail] [smallint] NOT NULL,
 CONSTRAINT [PK_profitwisemastervariant_PwMasterVariantId] PRIMARY KEY CLUSTERED 
(
	[PwMasterVariantId] ASC,
	[PwShopId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[profitwisemastervariantcogsdetail]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisemastervariantcogsdetail]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisemastervariantcogsdetail](
	[PwMasterVariantId] [bigint] NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[CogsDate] [date] NOT NULL,
	[CogsTypeId] [smallint] NOT NULL,
	[CogsCurrencyId] [int] NULL,
	[CogsAmount] [decimal](15, 2) NULL,
	[CogsMarginPercent] [decimal](4, 2) NULL,
 CONSTRAINT [PK_profitwisemastervariantcogsdetail_PwMasterVariantId] PRIMARY KEY CLUSTERED 
(
	[PwMasterVariantId] ASC,
	[PwShopId] ASC,
	[CogsDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[profitwisepicklist]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisepicklist]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisepicklist](
	[PwPickListId] [bigint] IDENTITY(100001,1) NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_profitwisepicklist_PwPickListId] PRIMARY KEY CLUSTERED 
(
	[PwPickListId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[profitwisepicklistmasterproduct]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisepicklistmasterproduct]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisepicklistmasterproduct](
	[PwPickListId] [bigint] NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[PwMasterProductId] [bigint] NOT NULL,
 CONSTRAINT [PK_profitwisepicklistmasterproduct_PwPickListId] PRIMARY KEY CLUSTERED 
(
	[PwPickListId] ASC,
	[PwShopId] ASC,
	[PwMasterProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[profitwiseproduct]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseproduct]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwiseproduct](
	[PwProductId] [bigint] IDENTITY(203,1) NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[PwMasterProductId] [bigint] NOT NULL,
	[ShopifyProductId] [bigint] NULL,
	[Title] [nvarchar](200) NULL,
	[Vendor] [nvarchar](100) NULL,
	[ProductType] [nvarchar](100) NULL,
	[Tags] [nvarchar](max) NULL,
	[IsActive] [tinyint] NOT NULL,
	[IsPrimary] [tinyint] NOT NULL,
	[IsPrimaryManual] [tinyint] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_profitwiseproduct_PwProductId] PRIMARY KEY CLUSTERED 
(
	[PwProductId] ASC,
	[PwShopId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[profitwiseprofitreportentry]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseprofitreportentry]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwiseprofitreportentry](
	[PwShopId] [bigint] NOT NULL,
	[EntryDate] [date] NOT NULL,
	[EntryType] [smallint] NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[SourceId] [bigint] NOT NULL,
	[PwProductId] [bigint] NULL,
	[PwVariantId] [bigint] NULL,
	[NetSales] [decimal](15, 2) NULL,
	[CoGS] [decimal](15, 2) NULL,
	[Quantity] [int] NULL,
 CONSTRAINT [PK_profitwiseprofitreportentry_PwShopId] PRIMARY KEY CLUSTERED 
(
	[PwShopId] ASC,
	[EntryDate] ASC,
	[EntryType] ASC,
	[ShopifyOrderId] ASC,
	[SourceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[profitwisereport]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereport]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisereport](
	[PwReportId] [bigint] IDENTITY(99790,1) NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[Name] [nvarchar](50) NULL,
	[IsSystemReport] [smallint] NOT NULL,
	[CopyForEditing] [smallint] NOT NULL,
	[OriginalReportId] [bigint] NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
	[GroupingId] [smallint] NULL,
	[OrderingId] [smallint] NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LastAccessedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_profitwisereport_PwReportId] PRIMARY KEY CLUSTERED 
(
	[PwReportId] ASC,
	[PwShopId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[profitwisereportfilter]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereportfilter]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisereportfilter](
	[PwReportId] [bigint] NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[PwFilterId] [bigint] NOT NULL,
	[FilterType] [smallint] NOT NULL,
	[NumberKey] [bigint] NULL,
	[StringKey] [nvarchar](100) NULL,
	[Title] [nvarchar](100) NULL,
	[Description] [nvarchar](150) NULL,
	[DisplayOrder] [int] NOT NULL,
 CONSTRAINT [PK_profitwisereportfilter_PwReportId] PRIMARY KEY CLUSTERED 
(
	[PwReportId] ASC,
	[PwShopId] ASC,
	[PwFilterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[profitwisereportmasterproduct]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereportmasterproduct]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisereportmasterproduct](
	[PwReportId] [bigint] IDENTITY(1,1) NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[PwMasterProductId] [bigint] NOT NULL,
 CONSTRAINT [PK_profitwisereportmasterproduct_PwReportId] PRIMARY KEY CLUSTERED 
(
	[PwReportId] ASC,
	[PwShopId] ASC,
	[PwMasterProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[profitwisereportproducttype]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereportproducttype]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisereportproducttype](
	[PwReportId] [bigint] IDENTITY(1,1) NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[ProductType] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_profitwisereportproducttype_PwReportId] PRIMARY KEY CLUSTERED 
(
	[PwReportId] ASC,
	[PwShopId] ASC,
	[ProductType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[profitwisereportquerystub]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereportquerystub]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisereportquerystub](
	[PwReportId] [bigint] NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[PwMasterVariantId] [bigint] NOT NULL,
	[PwMasterProductId] [bigint] NOT NULL,
	[Vendor] [nvarchar](100) NULL,
	[ProductType] [nvarchar](100) NULL,
	[ProductTitle] [nvarchar](100) NULL,
	[Sku] [nvarchar](100) NULL,
	[VariantTitle] [nvarchar](100) NULL,
 CONSTRAINT [PK_profitwisereportquerystub_PwReportId] PRIMARY KEY CLUSTERED 
(
	[PwReportId] ASC,
	[PwShopId] ASC,
	[PwMasterVariantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[profitwisereportsku]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereportsku]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisereportsku](
	[PwReportId] [bigint] IDENTITY(1,1) NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[PwMasterVariantId] [bigint] NOT NULL,
 CONSTRAINT [PK_profitwisereportsku_PwReportId] PRIMARY KEY CLUSTERED 
(
	[PwReportId] ASC,
	[PwShopId] ASC,
	[PwMasterVariantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[profitwisereportvendor]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereportvendor]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisereportvendor](
	[PwReportId] [bigint] IDENTITY(1,1) NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[Vendor] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_profitwisereportvendor_PwReportId] PRIMARY KEY CLUSTERED 
(
	[PwReportId] ASC,
	[PwShopId] ASC,
	[Vendor] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[profitwiseshop]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseshop]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwiseshop](
	[PwShopId] [bigint] IDENTITY(100002,1) NOT NULL,
	[ShopOwnerUserId] [nvarchar](128) NOT NULL,
	[ShopifyShopId] [bigint] NULL,
	[CurrencyId] [int] NULL,
	[TimeZone] [nvarchar](50) NULL,
	[IsAccessTokenValid] [smallint] NOT NULL,
	[IsShopEnabled] [smallint] NOT NULL,
	[IsDataLoaded] [smallint] NOT NULL,
	[StartingDateForOrders] [datetime] NULL,
	[UseDefaultMargin] [smallint] NOT NULL,
	[DefaultMargin] [decimal](15, 2) NOT NULL,
	[ProfitRealization] [smallint] NOT NULL,
	[DateRangeDefault] [smallint] NOT NULL,
 CONSTRAINT [PK_profitwiseshop_PwShopId] PRIMARY KEY CLUSTERED 
(
	[PwShopId] ASC,
	[ShopOwnerUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[profitwisevariant]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisevariant]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisevariant](
	[PwVariantId] [bigint] IDENTITY(873,1) NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[PwProductId] [bigint] NOT NULL,
	[PwMasterVariantId] [bigint] NOT NULL,
	[ShopifyProductId] [bigint] NULL,
	[ShopifyVariantId] [bigint] NULL,
	[Sku] [nvarchar](100) NULL,
	[Title] [nvarchar](200) NULL,
	[LowPrice] [decimal](15, 2) NOT NULL,
	[HighPrice] [decimal](15, 2) NOT NULL,
	[Inventory] [int] NULL,
	[IsActive] [smallint] NOT NULL,
	[IsPrimary] [smallint] NOT NULL,
	[IsPrimaryManual] [tinyint] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_profitwisevariant_PwVariantId] PRIMARY KEY CLUSTERED 
(
	[PwVariantId] ASC,
	[PwShopId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO


/****** Object:  Table [dbo].[shop]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shop]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[shop](
	[ShopId] [bigint] IDENTITY(955975,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_shop_ShopId] PRIMARY KEY CLUSTERED 
(
	[ShopId] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[shopifyorder]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorder]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[shopifyorder](
	[PwShopId] [bigint] NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[Email] [nvarchar](128) NULL,
	[OrderNumber] [nvarchar](128) NULL,
	[OrderDate] [date] NOT NULL,
	[OrderLevelDiscount] [decimal](15, 2) NULL,
	[FinancialStatus] [smallint] NOT NULL,
	[Tags] [nvarchar](500) NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NOT NULL,
	[Cancelled] [smallint] NOT NULL,
 CONSTRAINT [PK_shopifyorder_PwShopId] PRIMARY KEY CLUSTERED 
(
	[PwShopId] ASC,
	[ShopifyOrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[shopifyorderadjustment]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorderadjustment]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[shopifyorderadjustment](
	[PwShopId] [bigint] NOT NULL,
	[ShopifyAdjustmentId] [bigint] NOT NULL,
	[AdjustmentDate] [date] NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[Amount] [decimal](15, 2) NULL,
	[TaxAmount] [decimal](15, 2) NULL,
	[Kind] [nvarchar](100) NULL,
	[Reason] [nvarchar](100) NULL,
 CONSTRAINT [PK_shopifyorderadjustment_PwShopId] PRIMARY KEY CLUSTERED 
(
	[PwShopId] ASC,
	[ShopifyAdjustmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[shopifyorderlineitem]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorderlineitem]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[shopifyorderlineitem](
	[PwShopId] [bigint] NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[ShopifyOrderLineId] [bigint] NOT NULL,
	[OrderDateTimestamp] [datetime] NOT NULL,
	[OrderDate] [date] NOT NULL,
	[FinancialStatus] [smallint] NOT NULL,
	[PwProductId] [bigint] NOT NULL,
	[PwVariantId] [bigint] NOT NULL,
	[Quantity] [bigint] NOT NULL,
	[UnitPrice] [decimal](15, 2) NULL,
	[TotalDiscount] [decimal](15, 2) NULL,
	[TotalAfterAllDiscounts] [decimal](15, 2) NULL,
	[NetQuantity] [int] NOT NULL,
	[UnitCogs] [decimal](15, 2) NOT NULL,
 CONSTRAINT [PK_shopifyorderlineitem_PwShopId] PRIMARY KEY CLUSTERED 
(
	[PwShopId] ASC,
	[ShopifyOrderId] ASC,
	[ShopifyOrderLineId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[shopifyorderrefund]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorderrefund]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[shopifyorderrefund](
	[PwShopId] [bigint] NOT NULL,
	[ShopifyRefundId] [bigint] NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[ShopifyOrderLineId] [bigint] NOT NULL,
	[RefundDate] [date] NOT NULL,
	[PwProductId] [bigint] NOT NULL,
	[PwVariantId] [bigint] NOT NULL,
	[Amount] [decimal](15, 2) NULL,
	[RestockQuantity] [bigint] NOT NULL,
 CONSTRAINT [PK_shopifyorderrefund_PwShopId] PRIMARY KEY CLUSTERED 
(
	[PwShopId] ASC,
	[ShopifyRefundId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[shopifyproduct]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyproduct]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[shopifyproduct](
	[ShopId] [nvarchar](128) NOT NULL,
	[ShopifyProductId] [bigint] NOT NULL,
	[Title] [nvarchar](200) NULL,
 CONSTRAINT [PK_shopifyproduct_ShopId] PRIMARY KEY CLUSTERED 
(
	[ShopId] ASC,
	[ShopifyProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[shopifyvariant]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyvariant]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[shopifyvariant](
	[ShopId] [nvarchar](128) NOT NULL,
	[ShopifyVariantId] [bigint] NOT NULL,
	[ShopifyProductId] [bigint] NOT NULL,
	[Sku] [nvarchar](100) NULL,
	[Title] [nvarchar](200) NULL,
	[Price] [decimal](15, 2) NULL,
 CONSTRAINT [PK_shopifyvariant_ShopId] PRIMARY KEY CLUSTERED 
(
	[ShopId] ASC,
	[ShopifyVariantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[systemstate]    Script Date: 1/26/2017 8:40:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[systemstate]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[systemstate](
	[ExchangeRateLastDate] [date] NULL
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__calendar_tabl__y__72C60C4A]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[calendar_table] ADD  DEFAULT (NULL) FOR [y]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__calendar_tabl__q__73BA3083]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[calendar_table] ADD  DEFAULT (NULL) FOR [q]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__calendar_tabl__m__74AE54BC]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[calendar_table] ADD  DEFAULT (NULL) FOR [m]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__calendar_tabl__d__75A278F5]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[calendar_table] ADD  DEFAULT (NULL) FOR [d]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__calendar_tab__dw__76969D2E]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[calendar_table] ADD  DEFAULT (NULL) FOR [dw]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__calendar___month__778AC167]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[calendar_table] ADD  DEFAULT (NULL) FOR [monthName]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__calendar___dayNa__787EE5A0]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[calendar_table] ADD  DEFAULT (NULL) FOR [dayName]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__calendar_tabl__w__797309D9]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[calendar_table] ADD  DEFAULT (NULL) FOR [w]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__calendar___isWee__7A672E12]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[calendar_table] ADD  DEFAULT (NULL) FOR [isWeekday]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__calendar___isHol__7B5B524B]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[calendar_table] ADD  DEFAULT (NULL) FOR [isHoliday]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__calendar___holid__7C4F7684]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[calendar_table] ADD  DEFAULT (NULL) FOR [holidayDescr]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__calendar___isPay__7D439ABD]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[calendar_table] ADD  DEFAULT (NULL) FOR [isPayday]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__ints__i__7E37BEF6]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ints] ADD  DEFAULT (NULL) FOR [i]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Produ__7F2BE32F]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisebatchstate] ADD  DEFAULT (NULL) FOR [ProductsLastUpdated]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Order__00200768]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisebatchstate] ADD  DEFAULT (NULL) FOR [OrderDatasetStart]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Order__01142BA1]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisebatchstate] ADD  DEFAULT (NULL) FOR [OrderDatasetEnd]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__CogsT__02084FDA]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisemastervariant] ADD  DEFAULT (NULL) FOR [CogsTypeId]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__CogsC__02FC7413]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisemastervariant] ADD  DEFAULT (NULL) FOR [CogsCurrencyId]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__CogsA__03F0984C]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisemastervariant] ADD  DEFAULT (NULL) FOR [CogsAmount]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__CogsM__04E4BC85]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisemastervariant] ADD  DEFAULT (NULL) FOR [CogsMarginPercent]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__CogsC__05D8E0BE]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisemastervariantcogsdetail] ADD  DEFAULT (NULL) FOR [CogsCurrencyId]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__CogsA__06CD04F7]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisemastervariantcogsdetail] ADD  DEFAULT (NULL) FOR [CogsAmount]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__CogsM__07C12930]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisemastervariantcogsdetail] ADD  DEFAULT (NULL) FOR [CogsMarginPercent]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Creat__08B54D69]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisepicklist] ADD  DEFAULT (getdate()) FOR [CreatedDate]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Shopi__09A971A2]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwiseproduct] ADD  DEFAULT (NULL) FOR [ShopifyProductId]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Title__0A9D95DB]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwiseproduct] ADD  DEFAULT (NULL) FOR [Title]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Vendo__0B91BA14]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwiseproduct] ADD  DEFAULT (NULL) FOR [Vendor]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Produ__0C85DE4D]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwiseproduct] ADD  DEFAULT (NULL) FOR [ProductType]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__LastU__0D7A0286]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwiseproduct] ADD  DEFAULT (getdate()) FOR [LastUpdated]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__PwPro__0E6E26BF]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwiseprofitreportentry] ADD  DEFAULT (NULL) FOR [PwProductId]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__PwVar__0F624AF8]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwiseprofitreportentry] ADD  DEFAULT (NULL) FOR [PwVariantId]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__NetSa__10566F31]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwiseprofitreportentry] ADD  DEFAULT (NULL) FOR [NetSales]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwise__CoGS__114A936A]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwiseprofitreportentry] ADD  DEFAULT (NULL) FOR [CoGS]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Quant__123EB7A3]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwiseprofitreportentry] ADD  DEFAULT (NULL) FOR [Quantity]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwise__Name__1332DBDC]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisereport] ADD  DEFAULT (NULL) FOR [Name]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Group__14270015]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisereport] ADD  DEFAULT (NULL) FOR [GroupingId]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Order__151B244E]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisereport] ADD  DEFAULT (NULL) FOR [OrderingId]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Creat__160F4887]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisereport] ADD  DEFAULT (getdate()) FOR [CreatedDate]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__LastA__17036CC0]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisereport] ADD  DEFAULT (getdate()) FOR [LastAccessedDate]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Numbe__17F790F9]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisereportfilter] ADD  DEFAULT (NULL) FOR [NumberKey]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Strin__18EBB532]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisereportfilter] ADD  DEFAULT (NULL) FOR [StringKey]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Title__19DFD96B]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisereportfilter] ADD  DEFAULT (NULL) FOR [Title]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Descr__1AD3FDA4]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisereportfilter] ADD  DEFAULT (NULL) FOR [Description]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Vendo__1BC821DD]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisereportquerystub] ADD  DEFAULT (NULL) FOR [Vendor]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Produ__1CBC4616]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisereportquerystub] ADD  DEFAULT (NULL) FOR [ProductType]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Produ__1DB06A4F]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisereportquerystub] ADD  DEFAULT (NULL) FOR [ProductTitle]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwiser__Sku__1EA48E88]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisereportquerystub] ADD  DEFAULT (NULL) FOR [Sku]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Varia__1F98B2C1]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisereportquerystub] ADD  DEFAULT (NULL) FOR [VariantTitle]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Shopi__208CD6FA]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwiseshop] ADD  DEFAULT (NULL) FOR [ShopifyShopId]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Curre__2180FB33]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwiseshop] ADD  DEFAULT (NULL) FOR [CurrencyId]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__TimeZ__22751F6C]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwiseshop] ADD  DEFAULT (NULL) FOR [TimeZone]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Start__236943A5]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwiseshop] ADD  DEFAULT (NULL) FOR [StartingDateForOrders]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Shopi__245D67DE]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisevariant] ADD  DEFAULT (NULL) FOR [ShopifyProductId]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Shopi__25518C17]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisevariant] ADD  DEFAULT (NULL) FOR [ShopifyVariantId]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwisev__Sku__2645B050]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisevariant] ADD  DEFAULT (NULL) FOR [Sku]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Title__2739D489]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisevariant] ADD  DEFAULT (NULL) FOR [Title]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__Inven__282DF8C2]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisevariant] ADD  DEFAULT (NULL) FOR [Inventory]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__profitwis__LastU__29221CFB]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[profitwisevariant] ADD  DEFAULT (getdate()) FOR [LastUpdated]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifyor__Email__2A164134]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyorder] ADD  DEFAULT (NULL) FOR [Email]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifyor__Order__2B0A656D]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyorder] ADD  DEFAULT (NULL) FOR [OrderNumber]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifyor__Order__2BFE89A6]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyorder] ADD  DEFAULT (NULL) FOR [OrderLevelDiscount]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifyord__Tags__2CF2ADDF]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyorder] ADD  DEFAULT (NULL) FOR [Tags]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifyor__Creat__2DE6D218]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyorder] ADD  DEFAULT (getdate()) FOR [CreatedAt]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifyor__Updat__2EDAF651]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyorder] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifyor__Amoun__2FCF1A8A]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyorderadjustment] ADD  DEFAULT (NULL) FOR [Amount]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifyor__TaxAm__30C33EC3]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyorderadjustment] ADD  DEFAULT (NULL) FOR [TaxAmount]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifyord__Kind__31B762FC]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyorderadjustment] ADD  DEFAULT (NULL) FOR [Kind]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifyor__Reaso__32AB8735]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyorderadjustment] ADD  DEFAULT (NULL) FOR [Reason]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifyor__Order__339FAB6E]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyorderlineitem] ADD  DEFAULT (getdate()) FOR [OrderDateTimestamp]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifyor__UnitP__3493CFA7]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyorderlineitem] ADD  DEFAULT (NULL) FOR [UnitPrice]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifyor__Total__3587F3E0]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyorderlineitem] ADD  DEFAULT (NULL) FOR [TotalDiscount]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifyor__Total__367C1819]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyorderlineitem] ADD  DEFAULT (NULL) FOR [TotalAfterAllDiscounts]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifyor__Amoun__37703C52]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyorderrefund] ADD  DEFAULT (NULL) FOR [Amount]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifypr__Title__3864608B]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyproduct] ADD  DEFAULT (NULL) FOR [Title]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifyvari__Sku__395884C4]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyvariant] ADD  DEFAULT (NULL) FOR [Sku]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifyva__Title__3A4CA8FD]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyvariant] ADD  DEFAULT (NULL) FOR [Title]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__shopifyva__Price__3B40CD36]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[shopifyvariant] ADD  DEFAULT (NULL) FOR [Price]
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__systemsta__Excha__3C34F16F]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[systemstate] ADD  DEFAULT (NULL) FOR [ExchangeRateLastDate]
END

GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId]') AND parent_object_id = OBJECT_ID(N'[dbo].[AspNetUserClaims]'))
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId]') AND parent_object_id = OBJECT_ID(N'[dbo].[AspNetUserClaims]'))
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId]') AND parent_object_id = OBJECT_ID(N'[dbo].[AspNetUserLogins]'))
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId]') AND parent_object_id = OBJECT_ID(N'[dbo].[AspNetUserLogins]'))
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId]') AND parent_object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]'))
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId]') AND parent_object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]'))
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId]') AND parent_object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]'))
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId]') AND parent_object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]'))
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId]
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'calendar_table', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.calendar_table' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'calendar_table'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'currency', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.currency' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'currency'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'exchangerate', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.exchangerate' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'exchangerate'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'ints', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.ints' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ints'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'profitwisebatchstate', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.profitwisebatchstate' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'profitwisebatchstate'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'profitwisedatelookup', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.profitwisedatelookup' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'profitwisedatelookup'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'profitwisemasterproduct', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.profitwisemasterproduct' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'profitwisemasterproduct'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'profitwisemastervariant', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.profitwisemastervariant' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'profitwisemastervariant'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'profitwisemastervariantcogsdetail', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.profitwisemastervariantcogsdetail' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'profitwisemastervariantcogsdetail'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'profitwisepicklist', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.profitwisepicklist' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'profitwisepicklist'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'profitwisepicklistmasterproduct', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.profitwisepicklistmasterproduct' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'profitwisepicklistmasterproduct'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'profitwiseproduct', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.profitwiseproduct' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'profitwiseproduct'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'profitwiseprofitreportentry', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.profitwiseprofitreportentry' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'profitwiseprofitreportentry'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'profitwisereport', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.profitwisereport' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'profitwisereport'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'profitwisereportfilter', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.profitwisereportfilter' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'profitwisereportfilter'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'profitwisereportmasterproduct', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.profitwisereportmasterproduct' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'profitwisereportmasterproduct'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'profitwisereportproducttype', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.profitwisereportproducttype' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'profitwisereportproducttype'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'profitwisereportquerystub', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.profitwisereportquerystub' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'profitwisereportquerystub'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'profitwisereportsku', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.profitwisereportsku' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'profitwisereportsku'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'profitwisereportvendor', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.profitwisereportvendor' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'profitwisereportvendor'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'profitwiseshop', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.profitwiseshop' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'profitwiseshop'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'profitwisevariant', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.profitwisevariant' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'profitwisevariant'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'shop', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.shop' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'shop'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'shopifyorder', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.shopifyorder' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'shopifyorder'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'shopifyorderadjustment', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.shopifyorderadjustment' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'shopifyorderadjustment'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'shopifyorderlineitem', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.shopifyorderlineitem' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'shopifyorderlineitem'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'shopifyorderrefund', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.shopifyorderrefund' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'shopifyorderrefund'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'shopifyproduct', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.shopifyproduct' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'shopifyproduct'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'shopifyvariant', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.shopifyvariant' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'shopifyvariant'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'TABLE',N'systemstate', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.systemstate' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'systemstate'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'VIEW',N'vw_masterproductandvariantsearch', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.vw_masterproductandvariantsearch' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vw_masterproductandvariantsearch'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'VIEW',N'vw_reportmasterproducttomastervariant', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.vw_reportmasterproducttomastervariant' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vw_reportmasterproducttomastervariant'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'VIEW',N'vw_reportproducttypetoproduct', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.vw_reportproducttypetoproduct' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vw_reportproducttypetoproduct'
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_SSMA_SOURCE' , N'SCHEMA',N'dbo', N'VIEW',N'vw_reportvendortoproduct', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'profitwise.vw_reportvendortoproduct' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vw_reportvendortoproduct'
GO
