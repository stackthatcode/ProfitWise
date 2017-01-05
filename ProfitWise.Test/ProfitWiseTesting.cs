using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using System.Drawing.Imaging;

namespace ProfitWise.Test
{
    [TestClass]
    public class ProfitWiseTesting
    {

        static IWebDriver driverFF;
        static IWebDriver driverGC;
        static IWebDriver driverIE;

        [AssemblyInitialize]
        public static void SetUp(TestContext context)
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            driverGC = new ChromeDriver(options);

            //driverFF = new FirefoxDriver();

            //driverIE = new InternetExplorerDriver();
        }

        [TestMethod]
        public void LoginToProfitWise()
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
            ss.SaveAsFile("c:\\Screenshots\\Dashboard.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        [TestMethod]
        public void LoadOverallProfitability2016Report()
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
            Assert.AreEqual("$2,656,800.96", Total2016Revenues);
            Console.WriteLine("Total Revenues for 2016 = " + Total2016Revenues);

            //Check 2016 Profits
            var Total2016Profit = driverGC.FindElement(By.XPath("//*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[1]/div[4]/h1")).Text;
            Assert.AreEqual("$584,770.58", Total2016Profit);
            Console.WriteLine("Total Profit for 2016 = " + Total2016Profit);

            Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\OverallProfitability2016.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        [TestMethod]
        public void TestDrillDown()
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
            ss.SaveAsFile("c:\\Screenshots\\DrillDown.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        [TestMethod]
        public void SetCustomDateRange()
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
            Assert.AreEqual("$296,613.65", Dec2016Revenues);
            Console.WriteLine("Revenues for December, 2016 = " + Dec2016Revenues);

            //Check December, 2016 Profits
            var Dec2016Profit = driverGC.FindElement(By.XPath("//*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[1]/div[4]/h1")).Text;
            Assert.AreEqual("$63,400.84", Dec2016Profit);
            Console.WriteLine("Profit for December, 2016 = " + Dec2016Profit);

            Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\CustomDateRange.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        //[TestMethod]
        //public void GroupByVendor()
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
        //    ss.SaveAsFile("c:\\Screenshots\\GroupByVendor.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        //}

        [TestMethod]
        public void LoadEditFilters()
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
            Assert.AreEqual("20", NumVariants);
            Console.WriteLine("Number of 3D Printer Variants = " + NumVariants);
            

            Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\EditFilters.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            //Click OK to go back to Dashboard
            driverGC.FindElement(By.CssSelector(".btn.btn-primary")).Click();



        }

        [TestMethod]
        public void LoadVendorDetailReport()
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

            Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\VendorDetails.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            //Click Navigate Back to Summary to go back to Dashboard
            driverGC.FindElement(By.XPath("//*[@id=\"details-drillup-button\"]/a")).Click();

        }

        [TestMethod]
        public void LoadEditCoGS()
        {
            WebDriverWait wait = new WebDriverWait(driverGC, TimeSpan.FromSeconds(30));

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            driverGC.SwitchTo().DefaultContent();
            //Wait for Navitage To menu to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"next-popover-activator--1\"]")));

            driverGC.FindElement(By.XPath("//*[@id=\"next-popover-activator--1\"]")).Click();
            Thread.Sleep(1000);
            driverGC.FindElement(By.XPath("//*[@id=\"next-popover--1\"]/div[2]/div/div/ul/li[5]/button")).Click();
            Thread.Sleep(2000);
            driverGC.SwitchTo().Frame("app-iframe");
            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\EditCoGS.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        [TestMethod]
        public void EditCoGSValue()
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

            //Enter new value for CoGS
            Thread.Sleep(1000);
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".form-control.money-editor-text")));
            //driverGC.FindElement(By.CssSelector(".form-control.money-editor-text")).Clear();
            driverGC.FindElement(By.CssSelector(".form-control.money-editor-text")).SendKeys("2796.01" + Keys.Enter);

            //Switch back to the main app window
            Thread.Sleep(1000);
            driverGC.SwitchTo().DefaultContent();
            driverGC.SwitchTo().Frame("app-iframe");

            var CoGSValue = driverGC.FindElement(By.XPath("//*[@id=\"product-search-grid\"]/tbody/tr[1]/td[5]/a/span/span[1]/span")).Text;
            Assert.AreEqual(CoGSValue, "$2,796.01 USD");
            Console.WriteLine("CoGS for Ultimaker 3 = " + CoGSValue);



            //Take a screenshot
            Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\EditCoGSValue.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }


        //[TestMethod]
        //public void TestTemplate()
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

        //    Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
        //    ss.SaveAsFile("c:\\Screenshots\\XXX.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        //}


        [ClassCleanup]
        public static void Cleanup()
        {

            Thread.Sleep(1000);
            driverGC.Quit();
        }

    }


}
