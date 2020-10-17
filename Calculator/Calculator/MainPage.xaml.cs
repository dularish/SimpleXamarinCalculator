using Calculator.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Calculator
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            CalculatorViewModel calculatorViewModel = new CalculatorViewModel(_mainLabel);
            BindingContext = calculatorViewModel;
        }
    }
    
    public class BooleanToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string gridLengthValue = "*";
            if (int.TryParse(parameter.ToString(), out int parsedInt))
            {
                gridLengthValue = parsedInt.ToString() + gridLengthValue;
            }

            return (bool)value ? new GridLength(parsedInt, GridUnitType.Star) : new GridLength(0);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
