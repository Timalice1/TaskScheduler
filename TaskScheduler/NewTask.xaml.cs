using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;


namespace TaskScheduler {
    /// <summary>
    /// Логика взаимодействия для NewTask.xaml
    /// </summary>
    public partial class NewTask : Window {

        #region Properties
        public new string Name { get; private set; }
        public string Description { get; private set; }
        public string RunFile { get; private set; }
        public DateTime StartDateTime { get; private set; }
        public string Trigger { get; private set; }
        public short Interval { get; private set; }
        public string Arguments { get; private set; }
        #endregion

        public NewTask() {
            InitializeComponent();
            DatePickerOnce.DisplayDateStart = DateTime.Now;
            DatePickerDaily.DisplayDateStart = DateTime.Now;
            DatePickerOnce.SelectedDate = DateTime.Now;
            DatePickerDaily.SelectedDate = DateTime.Now;
            for (int i = 0; i < 24; i++) {
                if (i < 10) {

                    cbHoursTriggerOnce.Items.Add("0" + i);
                    cbHoursTriggerDaily.Items.Add("0" + i);
                }
                else {

                    cbHoursTriggerOnce.Items.Add(i);
                    cbHoursTriggerDaily.Items.Add(i);
                }
            }
            for (int i = 0; i < 60; i++) {
                if (i < 10) {
                    cbMinutesTriggerOnce.Items.Add("0" + i);
                    cbMinutesTriggerDaily.Items.Add("0" + i);
                }
                else {
                    cbMinutesTriggerOnce.Items.Add(i);
                    cbMinutesTriggerDaily.Items.Add(i);
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            try {
                CheckData();
                Name = tbName.Text;
                RunFile = tbFile.Text;
                Arguments = tbArguments.Text;

                //Triggers for once
                if (rbOnce.IsChecked == true) {
                    string startDate = DatePickerOnce.Text;
                    string startTime = $"{cbHoursTriggerOnce.Text}:{cbMinutesTriggerOnce.Text}";
                    StartDateTime = DateTime.Parse(startDate + " " + startTime);
                    Trigger = "Once";
                }

                //Triggers for daily
                if (rbDaily.IsChecked == true) {
                    string startDate = DatePickerDaily.Text;
                    string startTime = $"{cbHoursTriggerDaily.Text}:{cbMinutesTriggerDaily.Text}";
                    StartDateTime = DateTime.Parse(startDate + " " + startTime);
                    Trigger = $"Every {tbRepeatDaily.Text} days";
                    Interval = short.Parse(tbRepeatDaily.Text);
                }

                DialogResult = true;

            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void CheckData() {
            //Check for empty fields
            if (string.IsNullOrEmpty(tbName.Text) |
                string.IsNullOrEmpty(tbFile.Text))
                throw new Exception("Please, fill all required fields");

            //Check for existing file
            if (!File.Exists(tbFile.Text))
                throw new Exception("File not exist...");

            //Check for selecting event time
            if (rbDaily.IsChecked != true & rbOnce.IsChecked != true)
                throw new Exception("Please, select the event time");

            //Check data in selected tab
            if (rbOnce.IsChecked == true) {
                if (DatePickerOnce.Text == "" | cbHoursTriggerOnce.Text == "" | cbMinutesTriggerOnce.Text == "")
                    throw new Exception("Please, select time & date");
                return;
            }

            if (rbDaily.IsChecked == true) {
                if (DatePickerDaily.Text == "" | cbHoursTriggerDaily.Text == "" | cbMinutesTriggerDaily.Text == "")
                    throw new Exception("Please, select time & date");
                if (string.IsNullOrEmpty(tbRepeatDaily.Text))
                    throw new Exception("Please, select repeat time");

                return;
            }

        }

        #region
        private void rbOnce_Checked(object sender, RoutedEventArgs e) {
            tabControl.SelectedIndex = 0;
        }

        private void rbDaily_Checked(object sender, RoutedEventArgs e) {
            tabControl.SelectedIndex = 1;
        }

        private void rbWeekly_Checked(object sender, RoutedEventArgs e) {
            tabControl.SelectedIndex = 2;
        }
        #endregion

        private void tbRepeatDaily_TextChanged(object sender, TextChangedEventArgs e) {
            var regex = new Regex(@"\d+");
            if (!regex.IsMatch(tbRepeatDaily.Text)) {
                tbRepeatDaily.Text = "";
                return;
            }
            if (int.Parse(tbRepeatDaily.Text) <= 0) {
                tbRepeatDaily.Text = "";
                return;
            }
        }

        private void btnExplore_Click(object sender, RoutedEventArgs e) {
            try {
                OpenFileDialog ofd = new OpenFileDialog();
                var res = ofd.ShowDialog();

                if (res == true) {
                    tbFile.Text = ofd.FileName;
                }

            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
