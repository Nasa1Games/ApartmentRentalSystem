#nullable disable

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using MediatR;
using ApartmentRentalSystem.Application.Commands.Apartments;
using ApartmentRentalSystem.Application.Queries.Apartments;

namespace ApartmentRentalSystem.AdminUI.Forms
{
    public class EditApartmentForm : Form
    {
        private readonly IMediator _mediator;
        private readonly Guid _apartmentId;
        private TextBox txtUnitNumber;
        private NumericUpDown numEntrance;
        private NumericUpDown numFloor;
        private NumericUpDown numCapacity;
        private NumericUpDown numPrice;
        private TextBox txtDescription;
        private Button btnSave;
        private Button btnCancel;

        public EditApartmentForm(IMediator mediator, Guid apartmentId, string currentUnitNumber)
        {
            _mediator = mediator;
            _apartmentId = apartmentId;
            InitializeComponent();
            _ = LoadApartmentDataAsync();
        }

        private async Task LoadApartmentDataAsync()
        {
            try
            {
                // Загружаем данные через Query
                var query = new GetApartmentByIdQuery(_apartmentId);
                var apartment = await _mediator.Send(query);

                if (apartment != null)
                {
                    txtUnitNumber.Text = apartment.UnitNumber;
                    numEntrance.Value = apartment.Entrance;
                    numFloor.Value = apartment.Floor;
                    numCapacity.Value = apartment.Capacity;
                    numPrice.Value = apartment.BasePricePerNight;
                    txtDescription.Text = apartment.Description;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            Text = "Редактировать квартиру";
            Size = new Size(450, 480);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 245, 245);

            var lblTitle = new Label { Text = "Редактирование", Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };

            txtUnitNumber = new TextBox { Location = new Point(150, 60), Width = 250 };
            var lblUnit = new Label { Text = "Номер квартиры:", AutoSize = true, Location = new Point(20, 63) };

            numEntrance = new NumericUpDown { Location = new Point(150, 90), Width = 60, Minimum = 1, Maximum = 10 };
            var lblEntrance = new Label { Text = "Подъезд:", AutoSize = true, Location = new Point(20, 93) };

            numFloor = new NumericUpDown { Location = new Point(150, 120), Width = 60, Minimum = 1, Maximum = 50 };
            var lblFloor = new Label { Text = "Этаж:", AutoSize = true, Location = new Point(20, 123) };

            numCapacity = new NumericUpDown { Location = new Point(150, 150), Width = 60, Minimum = 1, Maximum = 10 };
            var lblCapacity = new Label { Text = "Вместимость:", AutoSize = true, Location = new Point(20, 153) };

            numPrice = new NumericUpDown { Location = new Point(150, 180), Width = 120, Minimum = 0, Maximum = 100000, DecimalPlaces = 2 };
            var lblPrice = new Label { Text = "Цена/ночь (₽):", AutoSize = true, Location = new Point(20, 183) };

            txtDescription = new TextBox { Location = new Point(20, 220), Width = 380, Height = 80, Multiline = true };
            var lblDesc = new Label { Text = "Описание:", AutoSize = true, Location = new Point(20, 203) };

            btnSave = new Button { Text = "Сохранить", Location = new Point(100, 330), Size = new Size(120, 40), BackColor = Color.FromArgb(241, 196, 15), ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSave.Click += btnSave_Click;

            btnCancel = new Button { Text = "❌ Отмена", Location = new Point(230, 330), Size = new Size(120, 40), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCancel.Click += (s, e) => Close();

            Controls.AddRange(new Control[] { lblTitle, lblUnit, txtUnitNumber, lblEntrance, numEntrance, lblFloor, numFloor, lblCapacity, numCapacity, lblPrice, numPrice, lblDesc, txtDescription, btnSave, btnCancel });
        }

        private async void btnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUnitNumber.Text))
            {
                MessageBox.Show("Введите номер квартиры", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var command = new UpdateApartmentCommand(
                    ApartmentId: _apartmentId,
                    UnitNumber: txtUnitNumber.Text,
                    Entrance: (int)numEntrance.Value,
                    Floor: (int)numFloor.Value,
                    Capacity: (int)numCapacity.Value,
                    BasePricePerNight: (decimal)numPrice.Value,
                    Description: txtDescription.Text
                );

                var success = await _mediator.Send(command);

                if (success)
                {
                    MessageBox.Show("✅ Квартира обновлена", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("❌ Не удалось обновить квартиру", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}