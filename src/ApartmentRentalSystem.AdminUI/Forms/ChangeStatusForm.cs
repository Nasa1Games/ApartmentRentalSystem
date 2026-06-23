#nullable disable

using System;
using System.Drawing;
using System.Windows.Forms;
using MediatR;
using ApartmentRentalSystem.Application.Commands.Apartments;
using ApartmentRentalSystem.Core.Enums;

namespace ApartmentRentalSystem.AdminUI.Forms
{
    public class ChangeStatusForm : Form
    {
        private readonly IMediator _mediator;
        private readonly Guid _apartmentId;
        private ComboBox cmbStatus;
        private Button btnSave;
        private Button btnCancel;

        public ChangeStatusForm(IMediator mediator, Guid apartmentId, ApartmentStatus currentStatus)
        {
            _mediator = mediator;
            _apartmentId = apartmentId;
            InitializeComponent();
            cmbStatus.SelectedItem = currentStatus.ToString();
        }

        private void InitializeComponent()
        {
            Text = "Изменить статус";
            Size = new Size(350, 200);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 245, 245);

            var lblTitle = new Label { Text = "Выберите статус:", Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };

            cmbStatus = new ComboBox { Location = new Point(20, 60), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Available", "Maintenance", "Occupied" });

            btnSave = new Button { Text = "Применить", Location = new Point(40, 110), Size = new Size(120, 40), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSave.Click += btnSave_Click;

            btnCancel = new Button { Text = "❌ Отмена", Location = new Point(180, 110), Size = new Size(120, 40), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCancel.Click += (s, e) => Close();

            Controls.AddRange(new Control[] { lblTitle, cmbStatus, btnSave, btnCancel });
        }

        private async void btnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                var newStatus = Enum.Parse<ApartmentStatus>(cmbStatus.SelectedItem.ToString());
                var command = new ChangeApartmentStatusCommand(_apartmentId, newStatus);
                var success = await _mediator.Send(command);

                if (success)
                {
                    MessageBox.Show("✅ Статус изменён", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("❌ Не удалось изменить статус", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}