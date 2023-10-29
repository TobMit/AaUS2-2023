using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace PDAAplication.MVVM.View;

public partial class FindObjects : Window
{
    private readonly Regex decimalNumber;
    public FindObjects(string title)
    {
        InitializeComponent();
        decimalNumber = new Regex(@"^-?\d+\.\d{6}$");
        this.Title = title;
        x = 0;
        y = 0;
        x2 = 0;
        y2 = 0;
    }
    
    public double x { get; set; }
    public double y { get; set; }
    public double x2 { get; set; }
    public double y2 { get; set; }
    
    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        x = double.Parse(xPoint.Text);
        y = double.Parse(yPoint.Text);
        x2 = double.Parse(x2Point.Text);
        y2 = double.Parse(y2Point.Text);
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