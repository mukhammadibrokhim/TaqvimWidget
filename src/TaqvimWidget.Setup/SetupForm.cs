using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using TaqvimWidget.Shared;

namespace TaqvimWidget.Setup;

internal sealed class SetupForm : Form
{
    private readonly CheckBox _autostart;
    private readonly CheckBox _desktop;
    private readonly CheckBox _launch;
    private readonly Button _install;
    private readonly Button _cancel;

    public SetupForm()
    {
        Text = AppIdentity.Name + " - o'rnatish";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        AutoScaleDimensions = new SizeF(96f, 96f);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(470, 300);
        Font = new Font("Segoe UI", 9.5f);
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        Controls.Add(new Label { Text = AppIdentity.Name, Font = new Font("Segoe UI Semibold", 17f, FontStyle.Bold), Location = new Point(20, 16), AutoSize = true });
        Controls.Add(new Label
        {
            Text = "Internet tezligi, trafik hisobi, namoz vaqtlari, kalendar, eslatmalar, tizim holati va pomodoro taymer.",
            Location = new Point(22, 58),
            Size = new Size(430, 52),
        });
        Controls.Add(new Label
        {
            Text = "O'rnatiladigan joy:\r\n" + AppIdentity.InstallDirectory,
            ForeColor = SystemColors.GrayText,
            Location = new Point(22, 114),
            Size = new Size(430, 40),
        });

        _autostart = AddCheckBox("Windows bilan birga ishga tushsin", 164, true);
        _desktop = AddCheckBox("Ish stolida yorliq yaratilsin", 192, false);
        _launch = AddCheckBox("O'rnatgandan so'ng ishga tushirilsin", 220, true);

        _install = new Button { Text = "O'rnatish", Size = new Size(120, 34), Location = new Point(210, 254) };
        _install.Click += (_, _) => Install();
        _cancel = new Button { Text = "Bekor qilish", Size = new Size(120, 34), Location = new Point(338, 254) };
        _cancel.Click += (_, _) => Close();
        Controls.Add(_install);
        Controls.Add(_cancel);
        AcceptButton = _install;
        CancelButton = _cancel;
    }

    private CheckBox AddCheckBox(string text, int y, bool value)
    {
        var box = new CheckBox { Text = text, Checked = value, Location = new Point(22, y), AutoSize = true };
        Controls.Add(box);
        return box;
    }

    private void Install()
    {
        SetBusy(true);
        try
        {
            string executable = Installer.Install(_autostart.Checked, _desktop.Checked);
            MessageBox.Show(this,
                AppIdentity.Name + " o'rnatildi.\r\n\r\nWidgetni ochish uchun soat yonidagi tezlik yozuvini yoki tray ikonkasini bosing.",
                AppIdentity.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (_launch.Checked) Process.Start(executable);
            Close();
        }
        catch (Exception ex)
        {
            // Foydalanuvchiga har qanday o'rnatish xatosi ko'rsatilishi kerak
            MessageBox.Show(this, "O'rnatishda xato: " + ex.Message, AppIdentity.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        _install.Enabled = !busy;
        _cancel.Enabled = !busy;
        _install.Text = busy ? "O'rnatilmoqda..." : "O'rnatish";
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }
}
