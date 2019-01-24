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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views.UserControls
{
    /// <summary>
    /// Interaction logic for RegisterView.xaml
    /// </summary>
    public partial class RegisterView : UserControl
    {
        public RegisterView()
        {
            InitializeComponent();
        }

        private void PasswordTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            RegisterButton.IsEnabled = CheckPassword();
        }

        private void PasswordTextBox2_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            RegisterButton.IsEnabled = CheckPassword();
        }

        private bool CheckPassword() => PasswordTextBox.Text == PasswordTextBox2.Text && PasswordTextBox.Text.Length > 7;
    }
}
