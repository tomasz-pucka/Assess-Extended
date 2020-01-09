﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Input;
using UTA.Annotations;

namespace UTA.Models.DataBase
{
    public class Criteria : INotifyPropertyChanged
    {
        private ObservableCollection<Criterion> _criteriaCollection;


        public Criteria()
        {
            CriteriaCollection = new ObservableCollection<Criterion>();
        }


        public ObservableCollection<Criterion> CriteriaCollection
        {
            get => _criteriaCollection;
            set
            {
                if (Equals(value, _criteriaCollection)) return;
                _criteriaCollection = value;
                OnPropertyChanged(nameof(CriteriaCollection));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public void Reset()
        {
            CriteriaCollection.Clear();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}