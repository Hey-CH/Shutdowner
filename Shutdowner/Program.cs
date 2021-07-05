using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shutdowner {
    static class Program {
        static NotifyIcon nicon;
        static SettingForm sform;
        static bool survive = true;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            nicon = new NotifyIcon();
            nicon.Icon = Shutdowner.Properties.Resources.Shutdowner;
            nicon.Text = "Shutdowner";
            nicon.Visible = true;
            //�R���e�L�X�g���j���[�N���b�N�����Ƃ��ɂ��\�����ꂽ�̂Ŏ~��
            //nicon.MouseClick += (s,e) => {
            //    if (e.Button == MouseButtons.Left) {
            //        if (sform == null) {//form1��close���Ă�null�ɂ͂Ȃ�Ȃ��̂ŃC�x���g��null�ɂ��Ă��
            //            sform = new SettingForm();
            //            sform.FormClosed += (s, e) => { sform = null; };
            //        }
            //        sform.Show();
            //    }
            //};

            //�E�N���b�N���j���[
            ContextMenuStrip cms = new ContextMenuStrip();
            ToolStripMenuItem tsmi1 = new ToolStripMenuItem("Exit");//�I��
            tsmi1.Click += (s, e) => {
                survive = false;
                nicon.Dispose();
                Application.Exit();
            };
            ToolStripMenuItem tsmi2 = new ToolStripMenuItem("Shutdown now!");//�������V���b�g�_�E��
            tsmi2.Click += (s, e) => {
                executeShutdownProcess();
            };
            ToolStripMenuItem tsmi3 = new ToolStripMenuItem("Setting");//�ݒ���
            tsmi3.Click += (s, e) => {
                if (sform == null) {
                    sform = new SettingForm();
                    sform.FormClosed += (s, e) => { sform = null; };
                }
                sform.Show();
            };
            cms.Items.Add(tsmi3);
            cms.Items.Add(tsmi2);
            cms.Items.Add(tsmi1);
            nicon.ContextMenuStrip = cms;

            Task.Run(async () => {
                var t0 = DateTime.Now;
                DateTime t1 = new(0);
                while (t1.Year < t0.Year) {
                    try {
                        t1 = new DateTime(t0.Year, t0.Month, t0.Day, int.Parse(Properties.Settings.Default.Hours), int.Parse(Properties.Settings.Default.Minutes), 0);
                    } catch {
                        //�ݒ莞�ԂŃG���[���o����ݒ��ʂ��o���i�ݒ�t�@�C���𒼐ڂ��������肵���Ƃ��p�j
                        if (sform == null) {
                            sform = new SettingForm();
                            sform.FormClosed += (s, e) => { sform = null; };
                        }
                        sform.ShowDialog();
                    }
                }
                while (survive) {
                    t0 = DateTime.Now;
                    //�ݒ莞�ԕύX�m�F
                    var t2= new DateTime(t0.Year, t0.Month, t0.Day, int.Parse(Properties.Settings.Default.Hours), int.Parse(Properties.Settings.Default.Minutes), 0);
                    if (t2.Hour != t1.Hour || t2.Minute != t1.Minute) {
                        t1 = t2;
                        if (Properties.Settings.Default.SettingTime > t1) t1 = t1.AddDays(1);//�ݒ莞����ݒ肵��������葁����΁A�������s����
                    }
                    //�ݒ莞�Ԃ����ݎ��Ԃ������Ă��Ă��A1���Ԉȏ㗣��Ă����ꍇ�͖����ɂ���
                    if (t0 > t1 && (t0 - t1) > new TimeSpan(1, 0, 0)) t1 = t1.AddDays(1);
                    //�ݒ莞�Ԃ𒴂�����V���b�g�_�E��
                    if (t0 >= t1) {
                        if (executeShutdownProcess()) break;
                        else t1 = t1.AddDays(1);
                    }
                    await Task.Delay(60000);
                }
            });

            Application.Run();
        }

        static bool executeShutdownProcess() {
            //�m�F��ʕ\��
            var cform = new ConfirmForm();
            if (cform.ShowDialog() == DialogResult.OK) {
                //���C����ʂ����v���Z�X�̏I��
                waitForCloseWindows();
                //Shutdown����
                shutdown();
                //�I��
                return true;
            } else {
                return false;
            }
        }

        static void waitForCloseWindows() {
            var ignoreProcesses = new[] { "TextInputHost", "ApplicationFrameHost", "SystemSettings" };//���̂��N�����ĂȂ�Calcurator�i�d��j���o�Ă��邪���u
            var closeList = new List<Process>();
            foreach (var p in Process.GetProcesses().Where(pr=>!ignoreProcesses.Contains(pr.ProcessName))) {
                if (p.MainWindowHandle != IntPtr.Zero && !string.IsNullOrEmpty(p.MainWindowTitle)) {
                    //Debug.WriteLine(p.ProcessName);
                    p.CloseMainWindow();
                    closeList.Add(p);
                }
            }
            //5���҂��Ă��I�����Ȃ��ꍇkill��kill�͗ǂ��Ȃ����ۂ�
            var st = DateTime.Now;
            while (closeList.Any(p => p.HasExited)) {
                if (DateTime.Now - st > new TimeSpan(0, 5, 0)) break;
                Task.Delay(1000);
            }
            //foreach (var p in closeList) {
            //    if (!p.HasExited) p.Kill();
            //}
        }
        static void shutdown() {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "shutdown.exe";
            psi.Arguments = "/s /f";
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            Process.Start(psi);
        }
    }
}
