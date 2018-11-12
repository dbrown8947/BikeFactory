/*
 * File          : MainWindow.xaml.cs
 * Project       : BikeFactory
 * Programmer    : Dustin Brown
 * First Version : November 2018
 * Description   : A simple interface to setup Workstations for BikeFactory
 */
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WorkstationManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // METHOD        : MainWindow
        // DESCRIPTION   : Initializes the MainWindow
        public MainWindow()
        {
            //setup sql
            InitializeComponent();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["BikeFactory"].ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            //get current counts of the workstations
            cmd.CommandText = "SELECT COUNT(*) FROM WorkStation WHERE [Type] = 1;";
            con.Open();
            TBMetalWorkstations.Text = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "SELECT COUNT(*) FROM WorkStation WHERE [Type] = 2;";
            TBPaintWorkstations.Text = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "SELECT COUNT(*) FROM WorkStation WHERE [Type] = 3;";
            TBAssemblyWorkstations.Text = cmd.ExecuteScalar().ToString();

            con.Close();
        }

        // METHOD        : Button_Click
        // DESCRIPTION   : Updates the number of Workstations on click
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //get values from ui
            Int32.TryParse(TBMetalWorkstations.Text, out int numOfMetalWorkstations);
            Int32.TryParse(TBPaintWorkstations.Text, out int numOfPaintWorkstations);
            Int32.TryParse(TBAssemblyWorkstations.Text, out int numOfAssemblyWorkstations);
            //setup sql
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["BikeFactory"].ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            //delete existing workstations
            cmd.CommandText = "DELETE FROM Workstation;";
            con.Open();
            cmd.ExecuteScalar();
            //create new workstations
            cmd.CommandText = "INSERT INTO Workstation ([Type]) VALUES (1);";
            for(int i = 0; i < numOfMetalWorkstations; i++)
            {
                cmd.ExecuteScalar();
            }
            cmd.CommandText = "INSERT INTO Workstation ([Type]) VALUES (2);";
            for (int i = 0; i < numOfPaintWorkstations; i++)
            {
                cmd.ExecuteScalar();
            }
            cmd.CommandText = "INSERT INTO Workstation ([Type]) VALUES (3);";
            for (int i = 0; i < numOfAssemblyWorkstations; i++)
            {
                cmd.ExecuteScalar();
            }
            con.Close();
            MessageBox.Show("The number of workstations has been updated.", "Workstations Update", MessageBoxButton.OK,MessageBoxImage.Information);
            Environment.Exit(0);
        }

        // METHOD        : NumberValidationTextBox
        // DESCRIPTION   : Validates input to only accept numeric values
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

    }
}
