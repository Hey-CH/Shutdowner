using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shutdowner {
    public partial class ConfirmForm : Form {
        int waitTime = 20;
        public ConfirmForm() {
            InitializeComponent();

            this.DialogResult = DialogResult.Cancel;
        }

        private void button1_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void ConfirmForm_Activated(object sender, EventArgs e) {
            //コンストラクタでやるとカウントが開始されない場合がある
            //20秒待ってCancelボタンを押さなければDialogResult.OKを返す
            Task.Run(async () => {
                for (int t = waitTime; t >= 0; t--) {
                    this.Invoke((MethodInvoker)delegate () { label2.Text = t + "s"; });
                    await Task.Delay(1000);
                }
                this.Invoke((MethodInvoker)delegate () {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                });
            });
        }
    }
}
