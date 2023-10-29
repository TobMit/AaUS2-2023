using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace PDAAplication.MVVM.View;

public partial class AddNehnutelnost : Window
{
    private readonly Regex number;
    private readonly Regex decimalNumber;
    
    public AddNehnutelnost()
    {
        InitializeComponent();
        number = new Regex("[^0-9]+");
        decimalNumber = new Regex(@"^-?\d+\.\d{6}$");
    }
    
    public double x { get; set; }
    public double y { get; set; }
    public double x2 { get; set; }
    public double y2 { get; set; }
    public string popis { get; set; }
    public int supisneCislo { get; set; }
    
    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        x = double.Parse(xPoint.Text);
        y = double.Parse(yPoint.Text);
        x2 = double.Parse(x2Point.Text);
        y2 = double.Parse(y2Point.Text);
        popis = Popis.Text;
        supisneCislo = int.Parse(SupisneCislo.Text);
        
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
    
    private void UIElement_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = number.IsMatch(e.Text);
    }

}