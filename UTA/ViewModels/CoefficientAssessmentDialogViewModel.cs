﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CalculationsEngine.Assess.Assess;
using DataModel.Input;
using MahApps.Metro.Controls.Dialogs;
using UTA.Annotations;
using UTA.Interactivity;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
    public class CoefficientAssessmentDialogViewModel : Tab, INotifyPropertyChanged
    {
        private readonly List<Criterion> _criteriaCollection;
        private string _textOptionSure;
        private string _textOptionLottery;
        private int _index;
        private readonly CoefficientsDialog _dialog;
        private Criterion _currentCriterion;
        private bool _closeDialog;

        public CoefficientAssessmentDialogViewModel(List<Criterion> criteriaCollection)
        {
            _criteriaCollection = criteriaCollection;
            Title = "Scaling coefficients";
            //todo fill it
            CriteriaCoefficientsList = new List<CriterionCoefficient>();
            _dialog = new CoefficientsDialog(criteriaCollection);
            _index = 0;
            SetupCriterionAssessment();

            TakeSureCommand = new RelayCommand(_ =>
            {
                _dialog.ProcessDialog(1);
                SetCoefficientsTextBlocks(_dialog);
            });
            TakeLotteryCommand = new RelayCommand(_ =>
            {
                _dialog.ProcessDialog(2);
                SetCoefficientsTextBlocks(_dialog);
            });
            TakeIndifferentCommand = new RelayCommand(_ =>
            {
                _dialog.ProcessDialog(3);
                _dialog.GetCoefficientsForCriterion(_currentCriterion);
                if (_index < _criteriaCollection.Count - 1)
                {
                    _index++;
                    SetupCriterionAssessment();
                }
                else
                    CloseDialog = true;
            });
        }

        private void SetupCriterionAssessment()
        {
            _currentCriterion = _criteriaCollection[_index];
            _dialog.SetInitialValues(_currentCriterion);
            SetCoefficientsTextBlocks(_dialog);
        }

        public List<CriterionCoefficient> CriteriaCoefficientsList { get; set; }

        public string Title { get; set; }
        public RelayCommand TakeSureCommand { get; }
        public RelayCommand TakeLotteryCommand { get; }
        public RelayCommand TakeIndifferentCommand { get; }
        public IDialogCoordinator DialogCoordinator { get; set; }
        public bool CloseDialog
        {
            get => _closeDialog;
            set
            {
                _closeDialog = value;
                OnPropertyChanged(nameof(CloseDialog));
            }
        }

        public string TextOptionSure
        {
            get => _textOptionSure;
            set
            {
                _textOptionSure = value;
                OnPropertyChanged(nameof(TextOptionSure));
            }
        }
        public string TextOptionLottery
        {
            get => _textOptionLottery;
            set
            {
                _textOptionLottery = value;
                OnPropertyChanged(nameof(TextOptionLottery));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;


        private void SetCoefficientsTextBlocks(CoefficientsDialog dialog)
        {
            TextOptionSure = "Click 'Sure' if you prefer to have for sure:\n";
            for (var i = 0; i < dialog.DisplayObject.CriterionNames.Length; i++)
                TextOptionSure += dialog.DisplayObject.CriterionNames[i] + " = " + dialog.DisplayObject.ValuesToCompare[i] + "\n";

            TextOptionLottery += "\nClick 'Lottery' if you prefer to have with probability " + dialog.DisplayObject.P + " these values:\n";

            for (var i = 0; i < dialog.DisplayObject.CriterionNames.Length; i++)
                TextOptionLottery += dialog.DisplayObject.CriterionNames[i] + " = " + dialog.DisplayObject.BestValues[i] + "\n";

            TextOptionLottery += "\nOR with probability " + dialog.DisplayObject.P + " these values:\n";

            for (var i = 0; i < dialog.DisplayObject.CriterionNames.Length; i++)
                TextOptionLottery += dialog.DisplayObject.CriterionNames[i] + " = " + dialog.DisplayObject.WorstValues[i] + "\n";
        }

        //todo bind directly in xaml?
        //todo allow exiting dialog?
        public async void DialogClosed(object sender, CancelEventArgs e)
        {
            // cancel close, because window doesn't wait for async function and closes anyway
            e.Cancel = !CloseDialog;

            var dialogResult = await DialogCoordinator.ShowMessageAsync(this,
                "Terminating calculation",
                "If you do not finish coefficients assessment, current calculation will not proceed. Do you want to close dialog and terminate calculation?",
                MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "Yes",
                    NegativeButtonText = "No",
                    DefaultButtonFocus = MessageDialogResult.Negative,
                    AnimateShow = false,
                    AnimateHide = false
                });

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                CriteriaCoefficientsList = null;
                CloseDialog = true;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}