using HltvRss.GCM;
using HltvRss.GCM.Class;
using System;
using System.Windows;
using System.Windows.Controls;

namespace HltvRss
{
    /// <summary>
    /// Interaction logic for GcmWindow.xaml
    /// </summary>
    public partial class GcmWindow : Window
    {
        public GcmWindow()
        {
            InitializeComponent();
        }

        private void sendBtn_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(((ComboBoxItem)comboBox.SelectedValue).Content);
            if (tbMsg.Text.Length < 1 || tbTitle.Text.Length < 1)
            {
                MessageBox.Show("You forgot the enter both title and message");
                return;
            }
            String topic = (String)((ComboBoxItem)comboBox.SelectedValue).Content;

            GCMObject obj = new GCMObject();
            obj.data.title = tbTitle.Text;
            obj.data.message = tbMsg.Text;
            obj.data.type = 4;
            GCMService.SendToTopic(obj, topic);
        }
    }
}
