﻿using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using PDAAplication.Core;

namespace PDAAplication.MVVM.View;

public partial class AddObject : Window
{
    public AddObject(Core.ObjectType type)
    {
        InitializeComponent();
        if (type == Core.ObjectType.Nehnutelnost)
        {
            this.Title = "Pridanie nehnuteľnosti";
            this.LabelID.Content = "Súpisné číslo";
            this.Popis.Text = "Nehnuteľnosť";
        }
        else
        {
            this.Title = "Pridanie parcely";
            this.LabelID.Content = "Číslo parcely";
            this.Popis.Text = "Parcela";
        }
    }

    public double x { get; set; }
    public char xOz { get; set; }
    public double y { get; set; }
    public char yOz { get; set; }
    public double x2 { get; set; }
    public char x2Oz { get; set; }
    public double y2 { get; set; }
    public char y2Oz { get; set; }
    public string popis { get; set; }
    public int IdCislo { get; set; }
    
    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            xOz = char.Parse(xOznacenie.Text);
            yOz = char.Parse(yOznacenie.Text);
            x2Oz = char.Parse(x2Oznacenie.Text);
            y2Oz = char.Parse(y2Oznacenie.Text);
            x = double.Parse(xPoint.Text);
            y = double.Parse(yPoint.Text);
            x2 = double.Parse(x2Point.Text);
            y2 = double.Parse(y2Point.Text);
            popis = Popis.Text;
            IdCislo = int.Parse(idCislo.Text);

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

    private void XOznacenie_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Core.Utils.xPrewiewCheck(sender, e);
    }

    private void YOznacenie_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Core.Utils.yPrewiewCheck(sender, e);
    }
}