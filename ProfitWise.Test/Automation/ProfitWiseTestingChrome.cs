using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace ProfitWise.Test.Automation
{
    [TestClass]
    public class ProfitWiseTestingChrome
    {
        static IWebDriver driverGC;

        [AssemblyInitialize]
        //[TestInitialize]
        public static void SetUp(TestContext context)
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            driverGC = new ChromeDriver(options);
        }


        //Google Chrome test procedures

        //[TestMethod]
        //public void GCRemoveProfitWiseIfInstalled()
        //{
        //    WebDriverWait wait = new WebDriverWait(driverGC, TimeSpan.FromSeconds(30));

        //    driverGC.Navigate().GoToUrl("https://3duniverse.myshopify.com/admin/apps");

        //    wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("login-input")));

        //    driverGC.FindElement(By.Id("login-input")).SendKeys("jeremy@3duniverse.org");
        //    driverGC.FindElement(By.Id("password")).SendKeys("Pi3141592654" + Keys.Enter);


        //    wait.Until(ExpectedConditions.ElementToBeClickable(By.PartialLinkText("Visit Shopify App Store")));

        //    if (driverGC.IsElementEnabled(By.PartialLinkText("ProfitWise")))
        //    {

        //        //Remove ProfitWise app

        //    }
        //}

        //[TestMethod]
        //public void GCInstallProfitWise()
        //{
        //    WebDriverWait wait = new WebDriverWait(driverGC, TimeSpan.FromSeconds(30));
        //
        //    driverGC.Navigate().GoToUrl("<install URL>");
        //
        // 
        //
        //}


        [TestMethod]
        public void GCLoginToProfitWise()
        {
            driverGC.Navigate().GoToUrl("https://3duniverse.myshopify.com/admin/apps/50d69dbaf54ee35929a946790d5884e4");

            WebDriverWait wait = new WebDriverWait(driverGC, TimeSpan.FromSeconds(30));
            wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("login-input")));

            driverGC.FindElement(By.Id("login-input")).SendKeys("jeremy@3duniverse.org");
            driverGC.FindElement(By.Id("password")).SendKeys("Pi3141592654" + Keys.Enter);


            wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("EmbeddedApp")));

            driverGC.SwitchTo().Frame("app-iframe");

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\Chrome\\Dashboard.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        [TestMethod]
        public void GCLoadOverallProfitability2016Report()
        {


            //Selecting elements by Partial Link Text
            //driverGC.FindElement(By.PartialLinkText("Select Report")).Click();


            WebDriverWait wait = new WebDriverWait(driverGC, TimeSpan.FromSeconds(30));

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            //Wait for "Select Report" drop-down to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.PartialLinkText("Select Report")));

            driverGC.FindElement(By.PartialLinkText("Select Report")).Click();

            //Wait for Reports drop-down list to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.PartialLinkText("Overall Profitability 2016")));

            driverGC.FindElement(By.PartialLinkText("Overall Profitability 2016")).Click();

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            //Wait for chart to finish loading
            Thread.Sleep(2500);

            //Check 2016 Revenues
            var Total2016Revenues = driverGC.FindElement(By.XPath("//*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[1]/div[2]/h1")).Text;
            Assert.AreEqual("$2,656,441.92", Total2016Revenues);
            Console.WriteLine("Total Revenues for 2016 = " + Total2016Revenues);

            //Check 2016 Profits
            var Total2016Profit = driverGC.FindElement(By.XPath("//*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[1]/div[4]/h1")).Text;
            Assert.AreEqual("$591,173.73", Total2016Profit);
            Console.WriteLine("Total Profit for 2016 = " + Total2016Profit);

            Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\Chrome\\OverallProfitability2016.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        [TestMethod]
        public void GCTestDrillDown()
        {


            WebDriverWait wait = new WebDriverWait(driverGC, TimeSpan.FromSeconds(30));

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            driverGC.FindElement(By.ClassName("highcharts-drilldown-point")).Click();

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            //Wait for chart to finish loading
            Thread.Sleep(1000);

            Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\Chrome\\DrillDown.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        [TestMethod]
        public void GCSetCustomDateRange()
        {

            WebDriverWait wait = new WebDriverWait(driverGC, TimeSpan.FromSeconds(30));

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            //Wait for Date Range Picker to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".date-ranger-picker.form-control")));

            driverGC.FindElement(By.CssSelector(".date-ranger-picker.form-control")).Click();

            //Wait for Date Range Picker Input Fields to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.Name("daterangepicker_start")));

            driverGC.FindElement(By.Name("daterangepicker_start")).Clear();
            driverGC.FindElement(By.Name("daterangepicker_start")).SendKeys("12\\1\\2016");

            driverGC.FindElement(By.Name("daterangepicker_end")).Clear();
            driverGC.FindElement(By.Name("daterangepicker_end")).SendKeys("12\\31\\2016");

            driverGC.FindElement(By.CssSelector(".applyBtn.btn.btn-sm.btn-primary")).Click();


            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));
            Thread.Sleep(2500);

            //Check December, 2016 Revenues
            var Dec2016Revenues = driverGC.FindElement(By.XPath("//*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[1]/div[2]/h1")).Text;
            Assert.AreEqual("$296,188.84", Dec2016Revenues);
            Console.WriteLine("Revenues for December, 2016 = " + Dec2016Revenues);

            //Check December, 2016 Profits
            var Dec2016Profit = driverGC.FindElement(By.XPath("//*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[1]/div[4]/h1")).Text;
            Assert.AreEqual("$63,211.21", Dec2016Profit);
            Console.WriteLine("Profit for December, 2016 = " + Dec2016Profit);

            Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\Chrome\\CustomDateRange.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        //[TestMethod]
        //public void GCGroupByVendor()
        //{
        //    WebDriverWait wait = new WebDriverWait(driverGC, TimeSpan.FromSeconds(30));

        //    //Wait for Spinner Layer to go away
        //    wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

        //    //Wait for Group By drop-down to be available
        //    wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[3]/div/div[1]/span[2]/span/a/span")));

        //    driverGC.FindElement(By.XPath("//*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[3]/div/div[1]/span[2]/span/a/span")).Click();
        //    driverGC.FindElement(By.ClassName("form-control")).Click();
        //    driverGC.FindElement(By.PartialLinkText("Vendor")).Click();

        //    //Wait for Spinner Layer to go away
        //    wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));
        //    Thread.Sleep(2000);

        //    Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
        //    ss.SaveAsFile("c:\\Screenshots\\Chrome\\GroupByVendor.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        //}

        [TestMethod]
        public void GCLoadEditFilters()
        {
            WebDriverWait wait = new WebDriverWait(driverGC, TimeSpan.FromSeconds(30));

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            //Wait for "Edit Filters" button to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"report-viewer-header\"]/div/div/div[2]/div[5]/a")));

            driverGC.FindElement(By.XPath("//*[@id=\"report-viewer-header\"]/div/div/div[2]/div[5]/a")).Click();

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            //Add filter for 3D Printers
            driverGC.FindElement(By.XPath("//*[@id=\"product-types\"]/tbody/tr[3]")).Click();
            Thread.Sleep(1000);

            //Check to make sure Number of Variants = 20
            var NumVariants = driverGC.FindElement(By.XPath("//*[@id=\"product-variant-count-container\"]/div[3]/a/span")).Text;
            Assert.AreEqual("21", NumVariants);
            Console.WriteLine("Number of 3D Printer Variants = " + NumVariants);
            

            Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\Chrome\\EditFilters.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            //Click OK to go back to Dashboard
            driverGC.FindElement(By.CssSelector(".btn.btn-primary")).Click();



        }

        [TestMethod]
        public void GCLoadVendorDetailReport()
        {
            WebDriverWait wait = new WebDriverWait(driverGC, TimeSpan.FromSeconds(30));

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            //Wait for "Vendor Details" button to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[5]/div[1]/div[1]/div[2]/div[1]/a")));

            driverGC.FindElement(By.XPath("//*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[5]/div[1]/div[1]/div[2]/div[1]/a")).Click();

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));
            Thread.Sleep(2000);

            //Check to make sure Number of Ultimaker products sold in December, 2016 = 96
            var NumSold = driverGC.FindElement(By.XPath("//*[@id=\"detail-report-table\"]/tbody/tr[1]/td[2]/span")).Text;
            Assert.AreEqual("96", NumSold);
            Console.WriteLine("Number of Ultimaker sales in December, 2016 = " + NumSold);

            Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\Chrome\\VendorDetails.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            //Click Navigate Back to Summary to go back to Dashboard
            driverGC.FindElement(By.XPath("//*[@id=\"details-drillup-button\"]/a")).Click();

        }

        [TestMethod]
        public void GCLoadEditCoGS()
        {
            WebDriverWait wait = new WebDriverWait(driverGC, TimeSpan.FromSeconds(30));

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            driverGC.SwitchTo().DefaultContent();
            //Wait for Navitage To menu to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"next-popover-activator--1\"]")));

            driverGC.FindElement(By.XPath("//*[@id=\"next-popover-activator--1\"]")).Click();
            Thread.Sleep(1000);
            driverGC.FindElement(By.XPath("//*[@id=\"next-popover--1\"]/div[2]/div/div/ul/li[3]/button")).Click();
            Thread.Sleep(2000);
            driverGC.SwitchTo().Frame("app-iframe");
            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\Chrome\\EditCoGS.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            
        }

        [TestMethod]
        public void GCEditCoGSValue()
        {
            WebDriverWait wait = new WebDriverWait(driverGC, TimeSpan.FromSeconds(30));
          
            //Wait for Search Field to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.TagName("input")));

            //Search for "Ultimaker 3"
            driverGC.FindElement(By.TagName("input")).Click();
            driverGC.FindElement(By.TagName("input")).SendKeys("Ultimaker 3" + Keys.Enter);

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));
            Thread.Sleep(1000);

            //Click on the CoGS value for Ultimaker 3
            driverGC.FindElement(By.XPath("//*[@id=\"product-search-grid\"]/tbody/tr[1]/td[5]/a/span/span[1]/span")).Click();

            //Switch to the CoGS Editor popup
            Thread.Sleep(1000);
            driverGC.SwitchTo().DefaultContent();
            driverGC.SwitchTo().Frame("app-modal-iframe");

            Thread.Sleep(1000);

            //Check to make sure the CoGS edit window is on the right product
            var Title = driverGC.FindElement(By.XPath("/html/body/div[1]/form/div[1]/div[1]/span")).Text;
            Assert.AreEqual("Ultimaker 3", Title);

            //Enter new CoGS value
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".form-control.money-editor-text")));
            driverGC.FindElement(By.CssSelector(".form-control.money-editor-text")).SendKeys("2796.01" + Keys.Enter);

            //Switch back to the main app window
            Thread.Sleep(1000);
            driverGC.SwitchTo().DefaultContent();
            driverGC.SwitchTo().Frame("app-iframe");

            //Check the CoGS Value for Ultimaker 3
            var CoGSValue = driverGC.FindElement(By.XPath("//*[@id=\"product-search-grid\"]/tbody/tr[1]/td[5]/a/span/span[1]/span")).Text;
            Assert.AreEqual("$2,796.01 USD", CoGSValue);
            Console.WriteLine("CoGS for Ultimaker 3 = " + CoGSValue);

            //Take a screenshot
            Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\Chrome\\EditCoGSValue.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);


        }



        //[TestMethod]
        //public void GCTestTemplate()
        //{
        //    WebDriverWait wait = new WebDriverWait(driverGC, TimeSpan.FromSeconds(30));

        //    //Wait for Spinner Layer to go away
        //    wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

        //    //Wait for XXX to be available
        //    wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("XXX")));

        //    driverGC.FindElement(By.XPath("XXX")).Click();

        //    //Wait for Spinner Layer to go away
        //    wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));
        //    Thread.Sleep(2000);

        //    //Check a value
        //    var Value = driverGC.FindElement(By.XPath("PATH")).Text;
        //    Assert.AreEqual("Value", Value);
        //    Console.WriteLine("Value = " + Value);

        //    Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
        //    ss.SaveAsFile("c:\\Screenshots\\Chrome\\XXX.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        //}



        [ClassCleanup]
        public static void Cleanup()
        {
            Thread.Sleep(1000);
            driverGC.Quit();
        }

    }


}
