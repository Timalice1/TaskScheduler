using Microsoft.Win32.TaskScheduler;
using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using System.Windows;

namespace TaskScheduler {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

        }

        /// <summary>
        /// Get all task from Windows using Microsoft.Win32.TaskSheduler.TaskService
        /// & load it to TaskList
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            try {
                var folder = TaskService.Instance.RootFolder;
                foreach (var task in folder.Tasks) {
                    MyTask t = new MyTask() {
                        Name = task.Name,
                        LastRunTime = task.LastRunTime,
                        NextRunTime = task.NextRunTime,
                        File = task.Definition.Actions[0].ToString(),
                    };
                    if (task.Definition.Triggers.Count > 1) {
                        StringBuilder sb = new StringBuilder();
                        foreach(var trigger in task.Definition.Triggers) 
                            sb.Append(trigger.ToString() + "; ");
                        t.Trigger = sb.ToString();

                    } else if (task.Definition.Triggers.Count == 1)
                        t.Trigger = task.Definition.Triggers[0].ToString();
                    TaskList.Items.Add(t);
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Create new object for MyTask using NewTask form
        /// Add it to TaskList & register by using StartTask method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddTask_Click(object sender, RoutedEventArgs e) {
            NewTask newTask = new NewTask();
            var res = newTask.ShowDialog();

            if (res == true) {
                //Create new task object
                MyTask task = new MyTask() {
                    Name = newTask.Name,
                    File = newTask.RunFile,
                    StartDateTime = newTask.StartDateTime,
                    Trigger = newTask.Trigger,
                    Interval = newTask.Interval,
                    Arguments = newTask.Arguments
                };

                //Run Task
                var t = StartTask(task);
                task.LastRunTime = t.LastRunTime;
                task.NextRunTime = t.NextRunTime;

                //Add task to TaskList
                TaskList.Items.Add(task);
            }

        }

        /// <summary>
        /// Create & execute the task using Microsoft.Win32.TaskSheduler namespace
        /// </summary>
        /// <param name="task">Task definition</param>
        private Task StartTask(MyTask task) {
            // Get the service on the local machine
            using (TaskService ts = new TaskService()) {
                // Create a new task definition and assign properties
                TaskDefinition td = ts.NewTask();
                td.Actions.PowerShellConversion = PowerShellActionPlatformOption.All;
                td.RegistrationInfo.Description = "Does something";

                // Create a defined triggers for task
                if (task.Trigger == "Once")
                    td.Triggers.Add(new TimeTrigger() { StartBoundary = task.StartDateTime });
                else {
                    td.Triggers.Add(new DailyTrigger() { DaysInterval = task.Interval, StartBoundary = task.StartDateTime });
                }
                // Create an action for task
                td.Actions.Add(task.File);

                //Run process even if laptop not charging
                td.Settings.DisallowStartIfOnBatteries = false;

                // Register the task in the root folder
                ts.RootFolder.RegisterTaskDefinition($@"{task.Name}", td);

                return ts.GetTask(task.Name);
            }

        }

        private void TaskList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            if(TaskList.SelectedItem == null) {
                btnDeleteTask.IsEnabled = false;
                btnRunNow.IsEnabled = false;
            }
            btnDeleteTask.IsEnabled = true;
            btnRunNow.IsEnabled = true;
        }

        //Execute task immediately
        private void btnRunNow_Click(object sender, RoutedEventArgs e) {
            var task = TaskList.SelectedItem as MyTask;
            TaskService.Instance.GetTask(task.Name).Run();
        }

        //Delete task from list & root folder
        private void btnDeleteTask_Click(object sender, RoutedEventArgs e) {
            try {
                //Get task from instance
                var task = TaskList.SelectedItem as MyTask;
                TaskService ts = new TaskService();

                // Check to make sure account privileges allow task deletion
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                    throw new Exception($"Cannot delete task with your current identity '{identity.Name}' permissions level." +
                    "You likely need to run this application 'as administrator' even if you are using an administrator account.");

                //Delete task
                ts.RootFolder.DeleteTask(task.Name);
                TaskList.Items.RemoveAt(TaskList.SelectedIndex);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

    }

    /// <summary>
    /// Describe the Task object
    /// </summary>
    class MyTask {
        public string Name { get; set; }
        public string Description { get; set; }
        public string File { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime LastRunTime { get; set; }
        public DateTime NextRunTime { get; set; }
        public string Trigger { get; set; }
        public short Interval { get; set; }
        public string Arguments { get; set; }
    }
}
