using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace PDAAplication.MVVM.View;

public partial class AddNehnutelnost : Window
{
    public AddNehnutelnost()
    {
        InitializeComponent();
    }
    
    public double x { get; set; }
    public double y { get; set; }
    public double x2 { get; set; }
    public double y2 { get; set; }
    public string popis { get; set; }
    public int supisneCislo { get; set; }
    
    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            x = double.Parse(xPoint.Text);
            y = double.Parse(yPoint.Text);
            x2 = double.Parse(x2Point.Text);
            y2 = double.Parse(y2Point.Text);
            popis = Popis.Text;
            supisneCislo = int.Parse(SupisneCislo.Text);

            DialogResult = true;
        }
        catch (Exception exception)
        {
            DialogResult = false;
            MessageBox.Show("Zle zadaný vstup", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void XPoint_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Core.Utils.DoublePreviewCheck(sender, e);
    }


    private void YPoint_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Core.Utils.DoublePreviewCheck(sender, e);
    }
    
    private void UIElement_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Core.Utils.NumaberPreviewCheck(sender, e);
    }

}