using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shutdowner {
    public partial class SettingForm : Form {
        public SettingForm() {
            InitializeComponent();

            textBox1.Text = Properties.Settings.Default.Hours;
            textBox2.Text = Properties.Settings.Default.Minutes;
        }

        private void button1_Click(object sender, EventArgs e) {
            var t0 = DateTime.Now;
            try {
                var t1 = new DateTime(t0.Year, t0.Month, t0.Day, int.Parse(textBox1.Text), int.Parse(textBox2.Text), 0);
                Properties.Settings.Default.Hours = textBox1.Text;
                Properties.Settings.Default.Minutes = textBox2.Text;
                Properties.Settings.Default.SettingTime = DateTime.Now;
                Properties.Settings.Default.Save();
                this.Close();
            } catch {
                MessageBox.Show("Could not set time.");
            }
        }

        Regex r = new Regex("[^\\d]");
        private void textBox1_TextChanged(object sender, EventArgs e) {
            if (r.IsMatch(textBox1.Text)) textBox1.Text = r.Replace(textBox1.Text, "");
            if (string.IsNullOrEmpty(textBox1.Text)) textBox1.Text = "0";
            var val = int.Parse(textBox1.Text);
            if (val < 0) val = 0;
            else if (val > 23) val = 23;
            textBox1.Text = val.ToString();
        }

        private void textBox2_TextChanged(object sender, EventArgs e) {
            if (r.IsMatch(textBox2.Text)) textBox2.Text = r.Replace(textBox2.Text, "");
            if (string.IsNullOrEmpty(textBox2.Text)) textBox2.Text = "0";
            var val = int.Parse(textBox2.Text);
            if (val < 0) val = 0;
            else if (val > 59) val = 59;
            textBox2.Text = val.ToString();
        }
    }
}
