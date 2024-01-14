using BlaBlaApp.Model;
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
        private IWebDriver driver;

        public Parser()
        {
            InitializeWebDriver();
        }
        private void InitializeWebDriver()
        {
            driver = new ChromeDriver();
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

                var _number = driver.FindElement(By.XPath("(//span[@data-pos='0'])[1]")).GetAttribute("textContent");
                var _type = driver.FindElement(By.XPath("(//span[@data-pos='0'])[3]")).GetAttribute("textContent");
                var _instance = driver.FindElement(By.XPath("(//span[@data-pos='0'])[4]")).GetAttribute("textContent");
                var _articles = driver.FindElement(By.XPath("(//span[@data-pos='0'])[5]")).GetAttribute("textContent");
                var _subject = driver.FindElement(By.XPath("(//span[@data-pos='0'])[6]")).GetAttribute("textContent");
                var _result = driver.FindElement(By.XPath("(//span[@data-pos='0'])[7]")).GetAttribute("textContent");
                var _court = driver.FindElement(By.XPath("(//span[@data-pos='0'])[8]")).GetAttribute("textContent");
                if(_court == "Не заполнено")
                    _court = driver.FindElement(By.XPath("(//a[@data-pos='0'])[1]")).GetAttribute("textContent");
                var _judge = driver.FindElement(By.XPath("(//a[@data-pos='0'])[2]")).GetAttribute("textContent");
                using (var context = new dbContext())
                {
                    // Создание нового объекта Court
                    Court Court = new Court()
                    {
                        Name = _court,
                        Judge = _judge
                    };
                    context.Courts.Add(Court);

                    // Проверка, существует ли уже дело с таким номером
                    var existingCase = context.Cases.FirstOrDefault(c => c.Number == _number);

                    if (existingCase == null)
                    {
                        // Создание нового объекта Case
                        existingCase = new Case()
                        {
                            Number = _number,
                            Type = _type,
                            Instance = _instance,
                            Subject = _subject,
                            Result = _result,
                            Court = Court
                        };
                        context.Cases.Add(existingCase);
                    }
                    else
                    {
                        // Обновление существующего дела
                        existingCase.Type = _type;
                        existingCase.Instance = _instance;
                        existingCase.Subject = _subject;
                        existingCase.Result = _result;
                        existingCase.Court = Court;
                    }

                    // Разбивка строки articles на отдельные статьи
                    var articleNames = _articles.Split(';');

                    // Для каждой статьи в articleNames
                    foreach (var articleName in articleNames)
                    {
                        // Поиск существующей статьи в базе данных
                        var Article = context.Articles.FirstOrDefault(a => a.Name == articleName.Trim());

                        // Если статья не найдена, создание новой статьи
                        if (Article == null)
                        {
                            Article = new Article()
                            {
                                Name = articleName.Trim()
                            };
                            context.Articles.Add(Article);
                        }

                        // Добавление статьи к делу
                        existingCase.Articles.Add(Article);
                    }
                    context.SaveChanges();
                }

                if (_number == oldNumber) break;


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
    }
}
