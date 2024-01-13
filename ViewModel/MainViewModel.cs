﻿using BlaBlaApp.Model;
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

namespace BlaBlaApp.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /* private ObservableCollection<Case> cases;
         public ObservableCollection<Case> Cases
         {
             get => cases;
             set
             {
                 cases = value;
                 OnPropertyChanged("Cases");
             }
         }
        */
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

        private ICommand _startParsingCommand;
        public ICommand StartParsingCommand
        {
            get
            {
                if (_startParsingCommand == null)
                {
                    _startParsingCommand = new Command(async param => await StartParsing(), param => CanStartParsing());
                }
                return _startParsingCommand;
            }
        }

        private async Task StartParsing()
        {
            if (parser == null)
            {
                parser = new Parser();
            }

            await parser.ParseData(DateFrom, DateTo, Article);

            var result = string.Empty;
            foreach (var pair in parser.GetResults())
            {
                result += $"{pair.Key}={pair.Value}\r\n";
            }

            MessageBox.Show(result);
        }

        private bool CanStartParsing()
        {
            return DateFrom != null && DateTo != null && !string.IsNullOrEmpty(Article);
        }

    }
}
