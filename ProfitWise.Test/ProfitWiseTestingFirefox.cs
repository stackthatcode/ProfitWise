using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using System.Drawing.Imaging;

namespace ProfitWise.Test.Firefox
{
    [TestClass]
    public class ProfitWiseTestingFirefox
    {

        static IWebDriver driverFF;
        //static IWebDriver driverFF;
        //static IWebDriver driverIE;

        [ClassInitialize]
        public static void SetUp(TestContext context)
        {



            var allProfiles = new FirefoxProfileManager();

            if (!allProfiles.ExistingProfiles.Contains("Selenium"))
            {
                throw new Exception("Selenium Firefox profile does not exist, please create it first.");
            }
            var profile = allProfiles.GetProfile("Selenium");

            profile.SetPreference("webdriver.firefox.profile", "Selenium");

            driverFF = new FirefoxDriver(profile);


            //driverIE = new InternetExplorerDriver();
        }


        //Firefox test procedures

        [TestMethod]
        public void FFLoginToProfitWise()
        {
            driverFF.Navigate().GoToUrl("https://3duniverse.myshopify.com/admin/apps/50d69dbaf54ee35929a946790d5884e4");

            WebDriverWait wait = new WebDriverWait(driverFF, TimeSpan.FromSeconds(90));
            wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("login-input")));

            driverFF.FindElement(By.Id("login-input")).SendKeys("jeremy@3duniverse.org");
            driverFF.FindElement(By.Id("password")).SendKeys("Pi3141592654" + Keys.Enter);


            wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("EmbeddedApp")));

            driverFF.SwitchTo().Frame("app-iframe");

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            Screenshot ss = ((ITakesScreenshot)driverFF).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\Firefox\\Dashboard.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        [TestMethod]
        public void FFLoadOverallProfitability2016Report()
        {


            //Selecting elements by Partial Link Text
            //driverFF.FindElement(By.PartialLinkText("Select Report")).Click();


            WebDriverWait wait = new WebDriverWait(driverFF, TimeSpan.FromSeconds(30));

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            //Wait for "Select Report" drop-down to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.PartialLinkText("Select Report")));

            driverFF.FindElement(By.PartialLinkText("Select Report")).Click();

            //Wait for Reports drop-down list to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.PartialLinkText("Overall Profitability 2016")));

            driverFF.FindElement(By.PartialLinkText("Overall Profitability 2016")).Click();

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            //Wait for chart to finish loading
            Thread.Sleep(2500);

            //Check 2016 Revenues
            var Total2016Revenues = driverFF.FindElement(By.XPath("//*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[1]/div[2]/h1")).Text;
            Assert.AreEqual("$2,656,800.96", Total2016Revenues);
            Console.WriteLine("Total Revenues for 2016 = " + Total2016Revenues);

            //Check 2016 Profits
            var Total2016Profit = driverFF.FindElement(By.XPath("//*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[1]/div[4]/h1")).Text;
            Assert.AreEqual("$584,770.58", Total2016Profit);
            Console.WriteLine("Total Profit for 2016 = " + Total2016Profit);

            Screenshot ss = ((ITakesScreenshot)driverFF).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\Firefox\\OverallProfitability2016.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        [TestMethod]
        public void FFTestDrillDown()
        {


            WebDriverWait wait = new WebDriverWait(driverFF, TimeSpan.FromSeconds(30));

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            driverFF.FindElement(By.ClassName("highcharts-drilldown-point")).Click();

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            //Wait for chart to finish loading
            Thread.Sleep(1000);

            Screenshot ss = ((ITakesScreenshot)driverFF).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\Firefox\\DrillDown.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        [TestMethod]
        public void FFSetCustomDateRange()
        {

            WebDriverWait wait = new WebDriverWait(driverFF, TimeSpan.FromSeconds(30));

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            //Wait for Date Range Picker to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".date-ranger-picker.form-control")));

            driverFF.FindElement(By.CssSelector(".date-ranger-picker.form-control")).Click();

            //Wait for Date Range Picker Input Fields to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.Name("daterangepicker_start")));

            driverFF.FindElement(By.Name("daterangepicker_start")).Clear();
            driverFF.FindElement(By.Name("daterangepicker_start")).SendKeys("12\\1\\2016");

            driverFF.FindElement(By.Name("daterangepicker_end")).Clear();
            driverFF.FindElement(By.Name("daterangepicker_end")).SendKeys("12\\31\\2016");

            driverFF.FindElement(By.CssSelector(".applyBtn.btn.btn-sm.btn-primary")).Click();


            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));
            Thread.Sleep(2500);

            //Check December, 2016 Revenues
            var Dec2016Revenues = driverFF.FindElement(By.XPath("//*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[1]/div[2]/h1")).Text;
            Assert.AreEqual("$296,613.65", Dec2016Revenues);
            Console.WriteLine("Revenues for December, 2016 = " + Dec2016Revenues);

            //Check December, 2016 Profits
            var Dec2016Profit = driverFF.FindElement(By.XPath("//*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[1]/div[4]/h1")).Text;
            Assert.AreEqual("$63,400.84", Dec2016Profit);
            Console.WriteLine("Profit for December, 2016 = " + Dec2016Profit);

            Screenshot ss = ((ITakesScreenshot)driverFF).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\Firefox\\CustomDateRange.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        //[TestMethod]
        //public void FFGroupByVendor()
        //{
        //    WebDriverWait wait = new WebDriverWait(driverFF, TimeSpan.FromSeconds(30));

        //    //Wait for Spinner Layer to go away
        //    wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

        //    //Wait for Group By drop-down to be available
        //    wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[3]/div/div[1]/span[2]/span/a/span")));

        //    driverFF.FindElement(By.XPath("//*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[3]/div/div[1]/span[2]/span/a/span")).Click();
        //    driverFF.FindElement(By.ClassName("form-control")).Click();
        //    driverFF.FindElement(By.PartialLinkText("Vendor")).Click();

        //    //Wait for Spinner Layer to go away
        //    wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));
        //    Thread.Sleep(2000);

        //    Screenshot ss = ((ITakesScreenshot)driverFF).GetScreenshot();
        //    ss.SaveAsFile("c:\\Screenshots\\Firefox\\GroupByVendor.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        //}

        [TestMethod]
        public void FFLoadEditFilters()
        {
            WebDriverWait wait = new WebDriverWait(driverFF, TimeSpan.FromSeconds(30));

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            //Wait for "Edit Filters" button to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"report-viewer-header\"]/div/div/div[2]/div[5]/a")));

            driverFF.FindElement(By.XPath("//*[@id=\"report-viewer-header\"]/div/div/div[2]/div[5]/a")).Click();

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            //Add filter for 3D Printers
            driverFF.FindElement(By.XPath("//*[@id=\"product-types\"]/tbody/tr[3]")).Click();
            Thread.Sleep(1000);

            //Check to make sure Number of Variants = 20
            var NumVariants = driverFF.FindElement(By.XPath("//*[@id=\"product-variant-count-container\"]/div[3]/a/span")).Text;
            Assert.AreEqual("20", NumVariants);
            Console.WriteLine("Number of 3D Printer Variants = " + NumVariants);
            

            Screenshot ss = ((ITakesScreenshot)driverFF).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\Firefox\\EditFilters.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            //Click OK to go back to Dashboard
            driverFF.FindElement(By.CssSelector(".btn.btn-primary")).Click();



        }

        [TestMethod]
        public void FFLoadVendorDetailReport()
        {
            WebDriverWait wait = new WebDriverWait(driverFF, TimeSpan.FromSeconds(30));

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            //Wait for "Vendor Details" button to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[5]/div[1]/div[1]/div[2]/div[1]/a")));

            driverFF.FindElement(By.XPath("//*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[5]/div[1]/div[1]/div[2]/div[1]/a")).Click();

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));
            Thread.Sleep(2000);

            //Check to make sure Number of Ultimaker products sold in December, 2016 = 96
            var NumSold = driverFF.FindElement(By.XPath("//*[@id=\"detail-report-table\"]/tbody/tr[1]/td[2]/span")).Text;
            Assert.AreEqual("96", NumSold);
            Console.WriteLine("Number of Ultimaker sales in December, 2016 = " + NumSold);

            Screenshot ss = ((ITakesScreenshot)driverFF).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\Firefox\\VendorDetails.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            //Click Navigate Back to Summary to go back to Dashboard
            driverFF.FindElement(By.XPath("//*[@id=\"details-drillup-button\"]/a")).Click();

        }

        [TestMethod]
        public void FFLoadEditCoGS()
        {
            WebDriverWait wait = new WebDriverWait(driverFF, TimeSpan.FromSeconds(30));

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            driverFF.SwitchTo().DefaultContent();
            //Wait for Navitage To menu to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"next-popover-activator--1\"]")));

            driverFF.FindElement(By.XPath("//*[@id=\"next-popover-activator--1\"]")).Click();
            Thread.Sleep(1000);
            driverFF.FindElement(By.XPath("//*[@id=\"next-popover--1\"]/div[2]/div/div/ul/li[5]/button")).Click();
            Thread.Sleep(2000);
            driverFF.SwitchTo().Frame("app-iframe");
            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            Screenshot ss = ((ITakesScreenshot)driverFF).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\Firefox\\EditCoGS.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        [TestMethod]
        public void FFEditCoGSValue()
        {
            WebDriverWait wait = new WebDriverWait(driverFF, TimeSpan.FromSeconds(30));
          
            //Wait for Search Field to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.TagName("input")));

            //Search for "Ultimaker 3"
            driverFF.FindElement(By.TagName("input")).Click();
            driverFF.FindElement(By.TagName("input")).SendKeys("Ultimaker 3" + Keys.Enter);

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));
            Thread.Sleep(1000);

            //Click on the CoGS value for Ultimaker 3
            driverFF.FindElement(By.XPath("//*[@id=\"product-search-grid\"]/tbody/tr[1]/td[5]/a/span/span[1]/span")).Click();

            //Switch to the CoGS Editor popup
            Thread.Sleep(1000);
            driverFF.SwitchTo().DefaultContent();
            driverFF.SwitchTo().Frame("app-modal-iframe");

            //Enter new value for CoGS
            Thread.Sleep(1000);
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".form-control.money-editor-text")));
            //driverFF.FindElement(By.CssSelector(".form-control.money-editor-text")).Clear();
            driverFF.FindElement(By.CssSelector(".form-control.money-editor-text")).SendKeys("2796.01" + Keys.Enter);

            //Switch back to the main app window
            Thread.Sleep(1000);
            driverFF.SwitchTo().DefaultContent();
            driverFF.SwitchTo().Frame("app-iframe");

            //Check the CoGS Value for Ultimaker 3
            var CoGSValue = driverFF.FindElement(By.XPath("//*[@id=\"product-search-grid\"]/tbody/tr[1]/td[5]/a/span/span[1]/span")).Text;
            Assert.AreEqual("$2,796.01 USD", CoGSValue);
            Console.WriteLine("CoGS for Ultimaker 3 = " + CoGSValue);



            //Take a screenshot
            Screenshot ss = ((ITakesScreenshot)driverFF).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\Firefox\\EditCoGSValue.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);


        }



        //[TestMethod]
        //public void FFTestTemplate()
        //{
        //    WebDriverWait wait = new WebDriverWait(driverFF, TimeSpan.FromSeconds(30));

        //    //Wait for Spinner Layer to go away
        //    wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

        //    //Wait for XXX to be available
        //    wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("XXX")));

        //    driverFF.FindElement(By.XPath("XXX")).Click();

        //    //Wait for Spinner Layer to go away
        //    wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));
        //    Thread.Sleep(2000);

        //    //Check a value
        //    var Value = driverFF.FindElement(By.XPath("PATH")).Text;
        //    Assert.AreEqual("Value", Value);
        //    Console.WriteLine("Value = " + Value);

        //    Screenshot ss = ((ITakesScreenshot)driverFF).GetScreenshot();
        //    ss.SaveAsFile("c:\\Screenshots\\Firefox\\XXX.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        //}




        [ClassCleanup]
        public static void Cleanup()
        {
            Thread.Sleep(1000);
            driverFF.Quit();
        }

    }


}
