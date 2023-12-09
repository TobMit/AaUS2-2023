using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace PDAApplication2.MVVM.View;

public partial class FindObject : Window
{
    public FindObject(string pTitle)
    {
        InitializeComponent();
        ID = 0;
        this.Title = pTitle;
    }
    
    public int ID { get; set; }
    
    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        ID = int.Parse(xPoint.Text);
        DialogResult = true;
    }
    private void XPoint_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Core.Utils.NumaberPreviewCheck(sender, e);
    }

}