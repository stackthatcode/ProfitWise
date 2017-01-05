using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Threading;

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
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"report-viewer-header\"]/div/div/div[2]/div[1]/a")));

            driverGC.FindElement(By.XPath("//*[@id=\"report-viewer-header\"]/div/div/div[2]/div[1]/a")).Click();
            
            //Wait for Reports drop-down list to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"report-viewer-header\"]/div/div/div[2]/div[1]/ul/li[4]/a/span[2]")));

            driverGC.FindElement(By.XPath("//*[@id=\"report-viewer-header\"]/div/div/div[2]/div[1]/ul/li[4]/a/span[2]")).Click();

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            //Wait for chart to finish loading
            Thread.Sleep(2500);

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
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"report-date-picker\"]/div/div/input")));

            driverGC.FindElement(By.XPath("//*[@id=\"report-date-picker\"]/div/div/input")).Click();

            //Wait for Date Range Picker Input Fields to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"report-date-picker\"]/div/div/div/div[1]/div[1]/input")));

            driverGC.FindElement(By.XPath("//*[@id=\"report-date-picker\"]/div/div/div/div[1]/div[1]/input")).Clear();
            driverGC.FindElement(By.XPath("//*[@id=\"report-date-picker\"]/div/div/div/div[1]/div[1]/input")).SendKeys("12\\1\\2016");

            driverGC.FindElement(By.XPath("//*[@id=\"report-date-picker\"]/div/div/div/div[2]/div[1]/input")).Clear();
            driverGC.FindElement(By.XPath("//*[@id=\"report-date-picker\"]/div/div/div/div[2]/div[1]/input")).SendKeys("12\\31\\2016");

            driverGC.FindElement(By.XPath("//*[@id=\"report-date-picker\"]/div/div/div/div[3]/div/button[1]")).Click();

            
            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));
            Thread.Sleep(2500);

            Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\CustomDateRange.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        [TestMethod]
        public void GroupByVendor()
        {
            WebDriverWait wait = new WebDriverWait(driverGC, TimeSpan.FromSeconds(30));

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));

            //Wait for XXX to be available
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("XXX")));

            driverGC.FindElement(By.XPath("XXX")).Click();

            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));
            Thread.Sleep(2000);

            Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\XXX.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }
        //*[@id=\"report-viewer-body\"]/div/div/div/div[2]/div/div[3]/div/div[1]/span[2]/span/a/span

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

            Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\EditFilters.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            //Click OK to go back to Dashboard
            driverGC.FindElement(By.XPath("//*[@id=\"report-editor-header-top\"]/div/div/div[2]/div/a")).Click();



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
            driverGC.FindElement(By.XPath("//*[@id=\"next-popover--1\"]/div[2]/div/div/ul/li[5]/button")).Click();
            Thread.Sleep(1000);
            driverGC.SwitchTo().Frame("app-iframe");
            //Wait for Spinner Layer to go away
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("spinner-layer")));
            Thread.Sleep(2000);

            Screenshot ss = ((ITakesScreenshot)driverGC).GetScreenshot();
            ss.SaveAsFile("c:\\Screenshots\\EditCoGS.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

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

            Thread.Sleep(5000);
            driverGC.Quit();
        }

    }


}
