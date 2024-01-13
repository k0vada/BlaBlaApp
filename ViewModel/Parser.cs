using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlaBlaApp.ViewModel
{
    public class Parser
    {
        private IWebDriver driver;// = new ChromeDriver();
        private Dictionary<string, string> _dict;

        public Parser()
        {
            _dict = new Dictionary<string, string>();
            InitializeWebDriver();
        }
        private void InitializeWebDriver()
        {
            driver = new ChromeDriver();
           // driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            driver.Url = @"https://bsr.sudrf.ru/bigs/portal.html";

        }

        public async Task ParseData(DateTime dateFrom, DateTime dateTo, string article)
        {
            await Task.Delay(3000);

            for (int i = 0; i < 50; i++)
            {
                try
                {
                    driver.FindElement(By.CssSelector(".date-filter-from"));
                    await Task.Delay(2000);
                    break;
                }
                catch (Exception)
                {
                    await Task.Delay(2000);
                }
            }
            await Task.Delay(2000);


            driver.FindElement(By.CssSelector(".date-filter-from")).SendKeys(dateFrom.ToString("d"));

            await Task.Delay(500);

            driver.FindElement(By.CssSelector(".date-filter-to")).SendKeys(dateTo.ToString("d"));

            await Task.Delay(500);

            driver.FindElement(By.XPath("//input[@placeholder='Введите статью или категорию дела']")).SendKeys(article);

            await Task.Delay(500);

            driver.FindElement(By.XPath("//input[@id='searchFormButton']")).Click();

            await Task.Delay(4000);

            driver.FindElement(By.XPath("//a[contains(.,'Уголовное дело')]")).Click();

            await Task.Delay(1000);

            var oldNumber = string.Empty;
            var counter = 0;
            while (true)
            {
                await Task.Delay(3000);
                counter++;
                if (counter == 8) break;

                try
                {
                    driver.FindElement(By.XPath("//label[contains(.,'Дело')]")).Click();
                }
                catch (Exception)
                {
                }

                await Task.Delay(500);

                var number = driver.FindElement(By.XPath("(//span[@data-pos='0'])[1]")).GetAttribute("textContent");
                var type = driver.FindElement(By.XPath("(//span[@data-pos='0'])[3]")).GetAttribute("textContent");
                var instance = driver.FindElement(By.XPath("(//span[@data-pos='0'])[4]")).GetAttribute("textContent");
                var articles = driver.FindElement(By.XPath("(//span[@data-pos='0'])[5]")).GetAttribute("textContent");
                var subject = driver.FindElement(By.XPath("(//span[@data-pos='0'])[6]")).GetAttribute("textContent");
                var result = driver.FindElement(By.XPath("(//span[@data-pos='0'])[8]")).GetAttribute("textContent");
                var court = driver.FindElement(By.XPath("(//span[@data-pos='0'])[7]")).GetAttribute("textContent");
                var judge = driver.FindElement(By.XPath("(//a[@data-pos='0'])[2]")).GetAttribute("textContent");

                if (number == oldNumber) break;

                _dict[number] = subject + type + instance + articles + subject + result + court + judge;

                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        driver.FindElement(By.XPath("(//span[@title='Вперед'])[3]")).Click();
                        await Task.Delay(2000);
                        break;
                    }
                    catch (Exception)
                    {
                        await Task.Delay(2000);
                    }
                }
            }
            driver.Quit();
        }
        public IReadOnlyDictionary<string, string> GetResults()
        {
            return _dict;
        }
    }
}
