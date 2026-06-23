using System;
<<<<<<< Updated upstream
using System.Drawing; 
using System.Windows.Forms;

namespace ApartmentRentalSystem.AdminUI.Forms
{
    public class BookingDetailsForm : Form 
=======
using System.Drawing;
using System.Windows.Forms;
using ApartmentRentalSystem.Application.Dto;

namespace ApartmentRentalSystem.AdminUI.Forms
{
    public class BookingDetailsForm : Form
>>>>>>> Stashed changes
    {
        private Label lblBookingId;
        private Label lblTenant;
        private Label lblDates;
        private Label lblPrice;
        private Label lblStatus;
        private Button btnClose;

<<<<<<< Updated upstream
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
=======
        public BookingDetailsForm(BookingDetailsDto details)
        {
            InitializeComponent();
            PopulateFromDto(details);
        }

        private void PopulateFromDto(BookingDetailsDto details)
        {
            lblBookingId.Text = $"ID брони: {details.BookingId}";
            lblTenant.Text = $"Пользователь: {details.TenantFullName} (ID: {details.TenantId})";
            
            var nights = (details.CheckOutDate - details.CheckInDate).Days;
            lblDates.Text = $"Даты: {details.CheckInDate:dd.MM.yyyy} — {details.CheckOutDate:dd.MM.yyyy} ({nights} ночей)";
            lblPrice.Text = $"Сумма: {details.TotalPrice:N0} ₽";
            
            lblStatus.Text = $"Статус: {GetStatusText(details.Status)}";
            lblStatus.ForeColor = GetStatusColor(details.Status);
        }

        private string GetStatusText(string status)
        {
            return status?.ToLower() switch
            {
                "pending" => "Ожидает подтверждения",
                "approved" => "Подтверждено",
                "rejected" => "Отклонено",
                "cancelled" => "Отменено",
                "completed" => "Завершено",
                _ => status ?? "Неизвестно"
            };
        }

        private Color GetStatusColor(string status)
        {
            return status?.ToLower() switch
            {
                "pending" => Color.Orange,
                "approved" => Color.FromArgb(46, 204, 113),
                "rejected" => Color.FromArgb(231, 76, 60),
                "cancelled" => Color.Gray,
                "completed" => Color.FromArgb(52, 152, 219),
                _ => Color.Black
            };
        }

        private void btnClose_Click(object? sender, EventArgs e) => this.Close();

        private void InitializeComponent()
        {
            Text = "Детали заявки";
            Size = new Size(500, 400);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
>>>>>>> Stashed changes
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 245, 245);

<<<<<<< Updated upstream
            // Заголовок
=======
>>>>>>> Stashed changes
            var lblTitle = new Label
            {
                Text = "Информация о бронировании",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };

<<<<<<< Updated upstream
            // Поля с данными (вертикальный список)
=======
>>>>>>> Stashed changes
            lblBookingId = new Label
            {
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(30, 70),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

<<<<<<< Updated upstream
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

=======
            lblTenant = new Label { AutoSize = true, Location = new Point(30, 110) };
            lblDates = new Label { AutoSize = true, Location = new Point(30, 140) };
            
>>>>>>> Stashed changes
            lblPrice = new Label
            {
                AutoSize = true,
                Location = new Point(30, 170),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
<<<<<<< Updated upstream
                ForeColor = Color.FromArgb(46, 204, 113)  // Зелёный для цены
            };

            lblStatus = new Label
            {
                AutoSize = true,
                Location = new Point(30, 200),
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };

            // 🔹 Разделитель
=======
                ForeColor = Color.FromArgb(46, 204, 113)
            };

            lblStatus = new Label { AutoSize = true, Location = new Point(30, 200) };

>>>>>>> Stashed changes
            var separator = new Label
            {
                Text = "────────────────────────────",
                AutoSize = true,
                Location = new Point(30, 230),
                ForeColor = Color.LightGray
            };

<<<<<<< Updated upstream
            // Кнопка "Закрыть"
=======
>>>>>>> Stashed changes
            btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(190, 270),
                Size = new Size(120, 40),
<<<<<<< Updated upstream
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
=======
>>>>>>> Stashed changes
                Cursor = Cursors.Hand,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
<<<<<<< Updated upstream
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
=======
            btnClose.Click += btnClose_Click;

            Controls.AddRange(new Control[] 
            { 
                lblTitle, lblBookingId, lblTenant, lblDates, lblPrice, lblStatus, 
                separator, btnClose 
            });
        }
>>>>>>> Stashed changes
    }
}