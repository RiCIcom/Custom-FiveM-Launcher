using FusionMods.src;
using System.Windows;
using System.Windows.Input;

//CMD:
//FusionMods.exe --devmode=boolean

namespace FusionMods
{
    public partial class MainWindow : Window
    {
        private bool settingsVisible = false;
        public MainWindow()
        {
            InitializeComponent();
            LoadUserSettings();
            DragPanel.MouseDown += DockPanel_MouseDown;
        }

        private void LoadUserSettings()
        {
            ServerIpTextBox.Text = Properties.Settings.Default.ServerIP;
            TeamSpeakIpTextBox.Text = Properties.Settings.Default.TeamSpeakIP;
            OpenTeamSpeakCheckbox.IsChecked = Properties.Settings.Default.AutoTeamSpeak;
            EnablePureModeCheckbox.IsChecked = Properties.Settings.Default.PureMode;
            WaitingTimeTextBox.Text = Properties.Settings.Default.WaitingTime;


            ServerIpTextBox.TextChanged += SaveSettings;
            TeamSpeakIpTextBox.TextChanged += SaveSettings;
            OpenTeamSpeakCheckbox.Checked += SaveSettings;
            OpenTeamSpeakCheckbox.Unchecked += SaveSettings;
            EnablePureModeCheckbox.Checked += SaveSettings;
            EnablePureModeCheckbox.Unchecked += SaveSettings;
            WaitingTimeTextBox.TextChanged += SaveSettings;
        }


        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ServerIP = ServerIpTextBox.Text.Trim();
            Properties.Settings.Default.TeamSpeakIP = TeamSpeakIpTextBox.Text.Trim();
            Properties.Settings.Default.WaitingTime = WaitingTimeTextBox.Text.Trim();
            Properties.Settings.Default.AutoTeamSpeak = OpenTeamSpeakCheckbox.IsChecked == true;
            Properties.Settings.Default.PureMode = EnablePureModeCheckbox.IsChecked == true;
            Properties.Settings.Default.Save();
        }

        private void ToggleSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.DevMode) { WaitingTimeTextBox.Visibility = Waitingtitle.Visibility = Visibility.Collapsed; }
            settingsVisible = !settingsVisible;
            SettingsPanel.Visibility = settingsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Backend.StartLoader();
        }

        private void DockPanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Closebtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}