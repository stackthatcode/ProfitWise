using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Support;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System.Threading;

namespace ProfitWise.Test
{
    [TestClass]
    public class UnitTest1
    {

        static IWebDriver driverFF;
        static IWebDriver driverGC;
        static IWebDriver driverIE;

        [AssemblyInitialize]
        public static void SetUp(TestContext context)
        {
            //driverFF = new FirefoxDriver();

            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            driverGC = new ChromeDriver(options);
            //driverIE = new InternetExplorerDriver();
        }


        //[TestMethod]
        //public void TestFirefoxDriver()
        //{
        //    driverFF.Navigate().GoToUrl("http://www.google.com");
        //    driverFF.FindElement(By.Id("lst-ib")).SendKeys("Selenium");
        //    driverFF.FindElement(By.Id("lst-ib")).SendKeys(Keys.Enter);
        //}

        //[TestMethod]
        //public void TestChromeDriver()
        //{
        //    driverGC.Navigate().GoToUrl("http://www.google.com");
        //    driverGC.FindElement(By.Id("lst-ib")).SendKeys("Selenium");
        //    driverGC.FindElement(By.Id("lst-ib")).SendKeys(Keys.Enter);
        //}

        //[TestMethod]
        //public void TestIEDriver()
        //{
        //    driverIE.Navigate().GoToUrl("http://www.google.com");
        //    driverIE.FindElement(By.Id("lst-ib")).SendKeys("Selenium");
        //    driverIE.FindElement(By.Id("lst-ib")).SendKeys(Keys.Enter);
        //}


        [TestMethod]
        public void TestProfitWiseOnChrome()
        {
            driverGC.Navigate().GoToUrl("https://3duniverse.myshopify.com/admin/apps/50d69dbaf54ee35929a946790d5884e4");

            WebDriverWait wait = new WebDriverWait(driverGC, TimeSpan.FromSeconds(30));
            wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("login-input")));

            driverGC.FindElement(By.Id("login-input")).SendKeys("jeremy@3duniverse.org");
            driverGC.FindElement(By.Id("password")).SendKeys("Pi3141592654" + Keys.Enter);


            wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("EmbeddedApp")));

            driverGC.SwitchTo().Frame("app-iframe");

            Thread.Sleep(5000);

            //driverGC.FindElement(By.PartialLinkText("Edit Filters")).Click();

            //driverGC.FindElement(By.Id("Report-Viewer-Report-Selector"));

            //driverGC.FindElement(By.ClassName("date-ranger-picker")).Click();
            //Thread.Sleep(500);
            //driverGC.FindElement(By.PartialLinkText("This Year")).Click();
            //Thread.Sleep(5000);

            driverGC.FindElement(By.PartialLinkText("Select Report")).Click();
            Thread.Sleep(200);
            driverGC.FindElement(By.PartialLinkText("Overall Profitability 2016")).Click();
            Thread.Sleep(5000);
            driverGC.FindElement(By.PartialLinkText("Edit Filters")).Click();


            //SelectElement mydropdownElement = new SelectElement(driverGC.FindElement(By.Id("Report-Viewer-Report-Selector")));
            //mydropdownElement.SelectByText("Overall Profitability 2016");

            //IWebElement reportSelector = driverGC.FindElement(By.Id("Report-Viewer-Report-Selector"));
            //IList<IWebElement> AllDropDownList = reportSelector.FindElements(By.XPath("option"));
            //int DpListCount = AllDropDownList.Count;
            //for (int i = 0; i < DpListCount; i++)
            //{
            //    if (AllDropDownList[i].Text == "Overall Profitability 2016")
            //    {
            //        AllDropDownList[i].Click();
            //    }
            //}
            //Console.WriteLine("Number of elements detected in dropdown = " + DpListCount);
            //Console.ReadLine();



        }

        [TestCleanup]
        public void Cleanup()
        {
            //driverGC.Quit();
        }






    }
}
