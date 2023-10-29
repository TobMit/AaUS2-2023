using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace PDAAplication.MVVM.View;

public partial class FindBuilding : Window
{
    private readonly Regex decimalNumber;
    public FindBuilding()
    {
        InitializeComponent();
        decimalNumber = new Regex(@"^-?\d+\.\d{6}$");
        x = 0;
        y = 0;
    }
    
    public double x { get; set; }
    public double y { get; set; }
    
    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        x = double.Parse(xPoint.Text);
        y = double.Parse(yPoint.Text);
        DialogResult = true;
    }
    private void XPoint_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        //todo add kontrola ci je v rozsahu
        e.Handled = decimalNumber.IsMatch(e.Text);
    }


    private void YPoint_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        //todo add kontrola ci je v rozsahu
        e.Handled = decimalNumber.IsMatch(e.Text);
    }
}