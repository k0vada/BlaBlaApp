using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word = Microsoft.Office.Interop.Word;


namespace BlaBlaApp
{
    public class WordHelper
    {
        internal static object fileNameForChart;
        private FileInfo fileInfo;

        public WordHelper(string fileName)
        {
            if (File.Exists(fileName)) // Проверка существования файла
            {
                fileInfo = new FileInfo(fileName);
            }
            else
            {
                throw new ArgumentException("File not found");
            }
        }

        internal bool Process(Dictionary<string, string> items)
        {
            Word.Application app = null;
            try
            {
                app = new Word.Application();
                Object file = fileInfo.FullName; // Подготавливаем объект для передачи в программу Word

                Object missing = Type.Missing; // Объект для передачи параметров

                app.Documents.Open(file); // Открываем документ
                foreach (var item in items)
                {
                    Word.Find find = app.Selection.Find;    //Объект для поиска
                    find.Text = item.Key;                   //Присваиваем текст, который будем искать
                    find.Replacement.Text = item.Value;     //То, на что будем менять

                    Object wrap = Word.WdFindWrap.wdFindContinue;
                    Object replace = Word.WdReplace.wdReplaceAll;

                    find.Execute(FindText: Type.Missing,
                        MatchCase: false,           // Чувтсвительность к регистру 
                        MatchWholeWord: false,      // Целостность слов 
                        MatchWildcards: false,      // Используется ли подстановочиый символ * или ?
                        MatchSoundsLike: missing,   // Звукоподобный поиск
                        MatchAllWordForms: false,   // Должен ли поиск сопоставлять все формы слова
                        Forward: true,              // Направление поиска текста
                        Wrap: wrap,                 //Продолжить поиск или остановиться 
                        Format: false,              // Форматирование при поиске
                        ReplaceWith: missing,       //Строка текста, которой заменяется найденный текст
                        Replace: replace);          //Замена
                }

                Object newFileName = Path.Combine(fileInfo.DirectoryName, DateTime.Now.ToString("yyyyMMdd HHmmss") + fileInfo.Name);
                fileNameForChart = newFileName;
                app.ActiveDocument.SaveAs2(newFileName);
                app.ActiveDocument.Close();
                app.Quit();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }
    }
}
