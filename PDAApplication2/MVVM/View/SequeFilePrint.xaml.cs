﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PDAApplication2.MVVM.View
{
    /// <summary>
    /// Interaction logic for SqueFilePrint.xaml
    /// </summary>
    public partial class SequeFilePrint : Window
    {
        public SequeFilePrint(string pTitle, string pPrimary, string pPrepln)
        {
            InitializeComponent();
            Title = pTitle;
            Primary.Text = pPrimary;
            Prepln.Text = pPrepln;
        }
    }
}
