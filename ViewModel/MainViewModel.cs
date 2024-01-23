using BlaBlaApp.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Data.Entity;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;

namespace BlaBlaApp.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Parser parser;

        private DateTime _dateFrom;
        public DateTime DateFrom
        {
            get { return _dateFrom; }
            set
            {
                if (_dateFrom != value)
                {
                    _dateFrom = value;
                    OnPropertyChanged(nameof(DateFrom));
                }
            }
        }

        private DateTime _dateTo;
        public DateTime DateTo
        {
            get { return _dateTo; }
            set
            {
                if (_dateTo != value)
                {
                    _dateTo = value;
                    OnPropertyChanged(nameof(DateTo));
                }
            }
        }

        private string _article;
        public string Article
        {
            get { return _article; }
            set
            {
                if (_article != value)
                {
                    _article = value;
                    OnPropertyChanged(nameof(Article));
                }
            }
        }

        private List<Case> caseList;
        public List<Case> CaseList
        {
            get => caseList;
            set
            {
                caseList = value;
                OnPropertyChanged("CaseList");
            }
        }
        #region Парсинг
        private ICommand _startParsingCommand;
        public ICommand StartParsingCommand
        {
            get
            {
                if (_startParsingCommand == null)
                {
                    _startParsingCommand = new Command(async param =>  await StartParsing(), param => CanStartParsing());
                }
                return _startParsingCommand;
            }
        }

       

        private Command getCases;
        public Command GetCases
        {
            get
            {
                getCases = new Command(obj =>
                {
                    using (var db = new dbContext())
                    {
                        var CasesInDB = db.Cases.OrderBy(x => x.Number).ToList(); // получаем данные из бд

                        CaseList = CasesInDB.Select(x => new Case // создаем новый список
                        {
                            Number = x.Number,
                            Type = x.Type,
                            Instance = x.Instance,
                            Subject = x.Subject,
                            Result = x.Result,
                            Court = x.Court,
                            Articles = x.Articles
                        }).ToList();
                    }
                });
                return getCases;
            }
        }


        private async Task StartParsing()
        {
            //if (parser == null)
            parser = new Parser();
            await parser.ParseData(DateFrom, DateTo, Article);
            GetCases.Execute(null);
            IsDataLoaded = true;
        }

        private bool CanStartParsing()
        {
            return DateFrom != null && DateTo != null && !string.IsNullOrEmpty(Article);
        }

        #endregion

        #region Работа с данными

        private Case _selectedCase;
        public Case SelectedCase
        {
            get { return _selectedCase; }
            set
            {
                _selectedCase = value;
                OnPropertyChanged(nameof(SelectedCase));
            }
        }


        private ICommand _deleteSelectedCommand;
        public ICommand DeleteSelectedCommand
        {
            get
            {
                if (_deleteSelectedCommand == null)
                    _deleteSelectedCommand = new Command(param => DeleteSelected(), param => CanDeleteSelected());
                
                return _deleteSelectedCommand;
            }
        }

        private void DeleteSelected()
        {
            using (var db = new dbContext())
            {
                var caseToDelete = db.Cases.FirstOrDefault(c => c.Number == SelectedCase.Number);
                if (caseToDelete != null)
                {
                    db.Cases.Remove(caseToDelete);
                    db.SaveChanges();  // try catch
                }
            }
            GetCases.Execute(null);
        }

        private bool CanDeleteSelected()
        {
            return SelectedCase != null;
        }

        private ICommand _deleteAllCommand;
        public ICommand DeleteAllCommand
        {
            get
            {
                if (_deleteAllCommand == null)
                    _deleteAllCommand = new Command(param => DeleteAll(), param => CanDeleteAll());
                return _deleteAllCommand;
            }
        }

        private void DeleteAll()
        {
            using (var db = new dbContext())
            {
                db.Cases.RemoveRange(db.Cases);
                db.Courts.RemoveRange(db.Courts);
                db.Articles.RemoveRange(db.Articles);
                db.SaveChanges();
            }
            GetCases.Execute(null);
            MessageBox.Show("Все данные успешно удалены.");

        }

        private bool CanDeleteAll()
        {
            return CaseList != null && CaseList.Count > 0;
        }

        private ICommand _editCaseCommand;
        public ICommand EditCaseCommand
        {
            get
            {
                if (_editCaseCommand == null)
                {
                    _editCaseCommand = new Command(obj =>
                    {
                        try
                        {
                            using (var db = new dbContext())
                            {
                                var caseToEdit = db.Cases // получаем существующий объект из контекста
                                    .Include(c => c.Court)
                                    .Include(c => c.Articles)
                                    .FirstOrDefault(c => c.Number == SelectedCase.Number);

                                if (caseToEdit != null)
                                {
                                    db.Entry(caseToEdit).CurrentValues.SetValues(SelectedCase); // обновляем свойства caseToEdit на основе SelectedCase...
                                    db.Entry(caseToEdit.Court).CurrentValues.SetValues(SelectedCase.Court); // обновляем данные в таблице Courts
                                    caseToEdit.Articles.Clear(); // обновляем данные в таблице Articles
                                    foreach (var selectedArticle in SelectedCase.Articles)
                                    {
                                        var existingArticle = db.Articles.Find(selectedArticle.ArticleId);
                                        if (existingArticle != null)
                                            caseToEdit.Articles.Add(existingArticle);
                                    }

                                    db.SaveChanges();
                                }
                            }
                        }
                        catch
                        {
                            MessageBox.Show("Некорректный ввод! Повторите попытку.");
                        }
                    });
                }
                return _editCaseCommand;
            }
        }

        private bool _isDataLoaded = false;
        public bool IsDataLoaded
        {
            get { return _isDataLoaded; }
            set
            {
                _isDataLoaded = value;
                OnPropertyChanged(nameof(IsDataLoaded));
            }
        }

        private ICommand _updateCommand;
        public ICommand UpdateCommand
        {
            get
            {
                if (_updateCommand == null)
                {
                    _updateCommand = new Command(async param =>
                    {
                        DeleteAll();
                        try
                        {
                            await StartParsing();
                        }
                        catch
                        {
                            await parser.ParseData(DateFrom, DateTo, Article);
                        }
                        GetCases.Execute(null);
                    });
                }
                return _updateCommand;
            }
        }
        #endregion

        #region Генератор отчета

        private ICommand _generateReportCommand;
        public ICommand GenerateReportCommand
        {
            get
            {
                if (_generateReportCommand == null)
                {
                    _generateReportCommand = new Command(
                        param => GenerateReport(),
                        param => SelectedCase != null
                    );
                }
                return _generateReportCommand;
            }
        }

        private void GenerateReport()
        {
            var wordHelper = new WordHelper("otchet_o_dele_template.doc");

            var items = new Dictionary<string, string>
            {
                {"<SUB>", SelectedCase.Subject },
                {"<COURT>", SelectedCase.Court.Name },
                {"<TYPE>", SelectedCase.Type },
                {"<NUM>", SelectedCase.Number },
                {"<INST>", SelectedCase.Instance },
                {"<ART>", string.Join(", ", SelectedCase.Articles.Select(a => a.Name)) },
                {"<RES>", SelectedCase.Result },
                {"<JUD>", SelectedCase.Court.Judge }
            };
            wordHelper.Process(items);
        }

        private ICommand _generateChartCommand;
        public ICommand GenerateChartCommand
        {
            get
            {
                if (_generateChartCommand == null)
                {
                    _generateChartCommand = new Command(
                        param => GenerateChart(),
                        param => IsDataLoaded
                    );
                }
                return _generateChartCommand;
            }
        }

        private void GenerateChart()
        {
            var excelHelper = new ExcelHelper();
            excelHelper.GenerateChart(CaseList);
            try
            {
                excelHelper.AddChartToReport();
            }
            catch
            {
                MessageBox.Show("Чтобы добавить диаграмму в отчет, сначала сформируйте его!");
            }
        }
        #endregion
    }
}
