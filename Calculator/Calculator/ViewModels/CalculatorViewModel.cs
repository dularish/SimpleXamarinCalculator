using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace Calculator.ViewModels
{
    internal class CalculatorViewModel : INotifyPropertyChanged
    {
        string _cachedUserInput = string.Empty;
        public CalculatorViewModel(Label calculatorLabel)
        {
            _calculatorLabel = calculatorLabel;
        }

        private bool _displayScientificPart = false;
        private bool _isInEditMode = true;
        private ICommand _streamAppendCommand;
        private Label _calculatorLabel;
        private EvaluateCommand _evaluateCommand;
        private BackspaceCommand _backspaceCommand;
        private ClearAllComand _clearAllCommand;
        private BackCommand _backCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _canStreamBeEditedOrEvaluated()
        {
            return IsInEditMode;
        }

        public ICommand StreamAppendCommand
        {
            get
            {
                if(_streamAppendCommand == null)
                {
                    _streamAppendCommand = new StreamUpdator(_calculatorLabel, new Func<bool>(() =>
                                                                                {
                                                                                    return _canStreamBeEditedOrEvaluated();
                                                                                }));
                }
                return _streamAppendCommand;
            }
        }

        public ICommand EvaluateCommand
        {
            get
            {
                if(_evaluateCommand == null)
                {
                    Action<string> callBackBeforeEvaluation = new Action<string>((input) => { _cachedUserInput = input; });
                    Func<bool> predicateToCheckBeforeEvalution = new Func<bool>(() =>
                    {
                        return _canStreamBeEditedOrEvaluated();
                    });
                    Action callBackToAlterEditMode = new Action(() =>
                    {
                        IsInEditMode = false;
                    });

                    _evaluateCommand = new EvaluateCommand(_calculatorLabel, callBackBeforeEvaluation, predicateToCheckBeforeEvalution, callBackToAlterEditMode);
                }
                return _evaluateCommand;
            }
        }

        public ICommand BackspaceCommand
        {
            get
            {
                if (_backspaceCommand == null)
                {
                    _backspaceCommand = new BackspaceCommand(_calculatorLabel, new Func<bool>(() =>
                                                                                {
                                                                                    return _canStreamBeEditedOrEvaluated();
                                                                                }));
                }
                return _backspaceCommand;
            }
        }

        public ICommand ClearAllCommand
        {
            get
            {
                if (_clearAllCommand == null)
                {
                    _clearAllCommand = new ClearAllComand(_calculatorLabel, new Action(() =>
                    {
                        IsInEditMode = true;
                    }));
                }
                return _clearAllCommand;
            }
        }

        public ICommand BackCommand
        {
            get
            {
                if (_backCommand == null)
                {
                    _backCommand = new BackCommand(_calculatorLabel,this, new Action(() =>
                    {
                        IsInEditMode = true;
                    }));
                }
                return _backCommand;
            }
        }

        public string CachedUserInput { get => _cachedUserInput; }
        public bool DisplayScientificPart
        {
            get { return _displayScientificPart; }
            set
            {
                if(_displayScientificPart != value)
                {
                    _displayScientificPart = value;
                    onPropertyChanged();
                }
                
            }
        }

        public bool IsInEditMode
        {
            get { return _isInEditMode; }
            set
            {
                if(_isInEditMode != value)
                {
                    _isInEditMode = value;
                    onPropertyChanged();
                }
            }
        }
    }



    internal class StreamUpdator : ICommand
    {
        private Label _label;
        private Func<bool> _canStreamBeEdited;

        public StreamUpdator(Label label, Func<bool> canStreamBeEditedOrEvaluated)
        {
            _label = label;
            _canStreamBeEdited = canStreamBeEditedOrEvaluated;
        }
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_canStreamBeEdited())
            {
                string charsToAppend = (string)parameter;
                _label.Text = _label.Text + charsToAppend;
            }
        }
    }

    internal class EvaluateCommand : ICommand
    {
        private Label _label;
        private Action<string> _callbackBeforeEvaluation;
        private Func<bool> _canEvaluationBeDone;
        private Action _callBackToAlterEditModeAfterEvaluation;

        public EvaluateCommand(Label label, Action<string> callBackBeforeEvaluation, Func<bool> canEvaluationBeDone, Action callBackToAlterEditMode)
        {
            _label = label;
            _callbackBeforeEvaluation = callBackBeforeEvaluation;
            _canEvaluationBeDone = canEvaluationBeDone;
            _callBackToAlterEditModeAfterEvaluation = callBackToAlterEditMode;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_canEvaluationBeDone())
            {
                var output = RefractoredImpl.parseAndEvaluateExpressionExpressively(_label.Text, new Dictionary<string, string>(), "calculatorInput");

                _callbackBeforeEvaluation(_label.Text);

                if (output.Item1.IsEvaluationSuccess)
                {
                    var evaluatedResultItem = (output.Item1 as MathematicalExpressionParser.ExpressionEvaluationResult.EvaluationSuccess).Item;

                    string evaluatedResultString = string.Empty;

                    if (evaluatedResultItem.IsDouble)
                    {
                        evaluatedResultString = (evaluatedResultItem as MathematicalExpressionParser.AllowedEvaluationResultTypes.Double).Item.ToString();
                    }
                    else if (evaluatedResultItem.IsString)
                    {
                        evaluatedResultString = (evaluatedResultItem as MathematicalExpressionParser.AllowedEvaluationResultTypes.String).Item;
                    }
                    else
                    {
                        //This shouldn't be thrown at any cases as per current implementation
                        throw new NotImplementedException();
                    }

                    _label.Text = "Evaluated result : " + evaluatedResultString;
                }
                else
                {
                    string evaluationErrorMessage = (output.Item1 as MathematicalExpressionParser.ExpressionEvaluationResult.EvaluationFailure).Item.ToString();

                    _label.Text = evaluationErrorMessage;
                }
                _callBackToAlterEditModeAfterEvaluation();
            }
        }
    }

    internal class BackspaceCommand : ICommand
    {
        private Label _label;
        private Func<bool> _canStreamBeEdited;

        public BackspaceCommand(Label label, Func<bool> canStreamBeEdited)
        {
            _label = label;
            _canStreamBeEdited = canStreamBeEdited;
        }
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_canStreamBeEdited())
            {
                string labelText = _label.Text;
                if (labelText.Length > 0)
                {
                    _label.Text = labelText.Substring(0, labelText.Length - 1);
                }
            }
        }
    }

    internal class ClearAllComand : ICommand
    {
        private Label _label;
        private Action _callBackToAlterEditMode;

        public ClearAllComand(Label label, Action callBackToAlterEditMode)
        {
            _label = label;
            _callBackToAlterEditMode = callBackToAlterEditMode;
        }
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _label.Text = string.Empty;
            _callBackToAlterEditMode();
        }
    }

    internal class BackCommand : ICommand
    {
        private Label _label;
        private CalculatorViewModel _viewModel;
        private Action _resetEditMode;

        public BackCommand(Label label, CalculatorViewModel viewModel, Action resetEditMode)
        {
            _label = label;
            _viewModel = viewModel;
            _resetEditMode = resetEditMode;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _label.Text = _viewModel.CachedUserInput;
            _resetEditMode();
        }
    }
}
