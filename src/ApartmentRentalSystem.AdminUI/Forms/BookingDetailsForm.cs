using System;
using System.Drawing; 
using System.Windows.Forms;

namespace ApartmentRentalSystem.AdminUI.Forms
{
    public class BookingDetailsForm : Form 
    {
        private Label lblBookingId;
        private Label lblTenant;
        private Label lblDates;
        private Label lblPrice;
        private Label lblStatus;
        private Button btnClose;

        public BookingDetailsForm(string bookingId)
        {
            InitializeComponent();
            // Заполняем данные (заглушки для проверки, нужно привязать!!!)
            lblBookingId.Text = $"ID брони: {bookingId ?? "N/A"}";
            lblTenant.Text = "Пользователь: Анна К. (ID: 12345)";
            lblDates.Text = "Даты: 15.06.2024 — 20.06.2024 (5 ночей)";
            lblPrice.Text = "Сумма: 17 500 ₽";
            lblStatus.Text = "Статус: Ожидает подтверждения";
            lblStatus.ForeColor = Color.Orange;
        }

        private void InitializeComponent()
        {
            // Настройка формы
            Text = "Детали заявки";
            Size = new Size(500, 400);
            StartPosition = FormStartPosition.CenterParent;  // По центру родительской формы
            FormBorderStyle = FormBorderStyle.FixedDialog;   // Нельзя менять размер
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 245, 245);

            // Заголовок
            var lblTitle = new Label
            {
                Text = "Информация о бронировании",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Поля с данными (вертикальный список)
            lblBookingId = new Label
            {
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(30, 70),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            lblTenant = new Label
            {
                AutoSize = true,
                Location = new Point(30, 110),
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };

            lblDates = new Label
            {
                AutoSize = true,
                Location = new Point(30, 140),
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };

            lblPrice = new Label
            {
                AutoSize = true,
                Location = new Point(30, 170),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113)  // Зелёный для цены
            };

            lblStatus = new Label
            {
                AutoSize = true,
                Location = new Point(30, 200),
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };

            // 🔹 Разделитель
            var separator = new Label
            {
                Text = "────────────────────────────",
                AutoSize = true,
                Location = new Point(30, 230),
                ForeColor = Color.LightGray
            };

            // Кнопка "Закрыть"
            btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(190, 270),
                Size = new Size(120, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Cursor = Cursors.Hand,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += btnClose_Click;  // Привязка события

            // !!!
            var lblNote = new Label
            {
                Text = "Здесь загрузить реальные данные из IBookingService!!!",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                AutoSize = true,
                Location = new Point(30, 330),
                ForeColor = Color.Gray
            };

            // Добавляем все элементы на форму
            Controls.AddRange(new Control[] 
            { 
                lblTitle, lblBookingId, lblTenant, lblDates, lblPrice, lblStatus, 
                separator, btnClose, lblNote 
            });
        }

        // Обработчик кнопки "Закрыть"
        private void btnClose_Click(object? sender, EventArgs e)
        {
            this.Close();
        }
    }
}