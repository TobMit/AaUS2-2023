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
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PDAAplication.MVVM.View
{
    /// <summary>
    /// Interaction logic for DataGeneratorDialog.xaml
    /// </summary>
    public partial class DataGeneratorDialog : Window
    {
        public DataGeneratorDialog()
        {
            InitializeComponent();
            PocetNehnutelnosti = 100;
            PocetParciel = 100;
            
        }

        public int PocetNehnutelnosti { get; set; }
        public int PocetParciel { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public int sirka { get; set; }
        public int dlzka { get; set; }
        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                PocetNehnutelnosti = int.Parse(Nehnutelnosti.Text);
                PocetParciel = int.Parse(Parcely.Text);
                x = double.Parse(xPoint.Text);
                y = double.Parse(yPoint.Text);
                sirka = int.Parse(Sikra.Text);
                dlzka = int.Parse(Dlzka.Text);
                DialogResult = true;
            }
            catch (Exception exception)
            {
                DialogResult = false;
                MessageBox.Show("Zle zadaný vstup", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UIElement_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Core.Utils.NumaberPreviewCheck(sender, e);
        }

        private void XPoint_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Core.Utils.DoublePreviewCheck(sender, e);
        }


        private void YPoint_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Core.Utils.DoublePreviewCheck(sender, e);
        }
    }
}
