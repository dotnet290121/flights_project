using DbGenerator.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DbGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string countriesInput = this.Countries_Text_Box.Text.Trim();

            if (!int.TryParse(countriesInput, out int countries_count))
            {
                string[] rangeStr = countriesInput.Split('-');
                if (rangeStr.Length != 2)
                    throw new InvalidInputException("Countries Input Not Valid");

                int[] range = new int[2];
                foreach (var item in rangeStr)
                {
                   if (!int.TryParse(item, out int num_in_range))
                    { }
                }
            }

        }
    }
}
