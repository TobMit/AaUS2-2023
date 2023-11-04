using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace PDAAplication.MVVM.View;

public partial class FindObject : Window
{
    public FindObject(string pTitle)
    {
        InitializeComponent();
        x = 0;
        y = 0;
        this.Title = pTitle;
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
        Core.Utils.DoublePreviewCheck(sender, e);
    }


    private void YPoint_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Core.Utils.DoublePreviewCheck(sender, e);
    }
}