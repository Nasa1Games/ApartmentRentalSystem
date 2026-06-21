using System;
using System.Drawing;  
using System.Windows.Forms;
using ApartmentRentalSystem.AdminUI.Forms;

namespace ApartmentRentalSystem.AdminUI
{
    public class MainForm : Form 
    {
        public MainForm()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            Text = "Админ-панель";
            Size = new Size(600, 450);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(245, 245, 245);

            var lblTitle = new Label
            {
                Text = "Панель администратора",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(150, 40)
            };

            
            var btnApartments = CreateButton("Управление квартирами", new Point(150, 120), 300, 50);
            btnApartments.Click += btnApartments_Click;  // Привязка к методу

            var btnBookings = CreateButton("Заявки на бронирование", new Point(150, 190), 300, 50);
            btnBookings.Click += btnBookings_Click;

            var btnNotifications = CreateButton("Журнал уведомлений", new Point(150, 260), 300, 50);
            btnNotifications.Click += btnNotifications_Click;

            var btnExit = CreateButton("Выход", new Point(225, 340), 150, 40);
            btnExit.BackColor = Color.FromArgb(231, 76, 60);
            btnExit.ForeColor = Color.White;
            btnExit.Click += btnExit_Click;

            Controls.AddRange(new Control[] { lblTitle, btnApartments, btnBookings, btnNotifications, btnExit });
        }

        private Button CreateButton(string text, Point location, int width, int height)
        {
            return new Button
            {
                Text = text,
                Location = location,
                Size = new Size(width, height),
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                Cursor = Cursors.Hand,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
        }

        // Отдельные методы-обработчики, здесь должна быть логика
        private void btnApartments_Click(object? sender, EventArgs e)
        {
            // Здесь открыть форму управления квартирами с реальными данными
            var form = new ApartmentsForm();
            form.Show();
        }

        private void btnBookings_Click(object? sender, EventArgs e)
        {
            //  Здесь открыть форму заявок с подключением к IBookingService
            var form = new BookingsForm();
            form.Show();
        }

        private void btnNotifications_Click(object? sender, EventArgs e)
        {
            //  Здесь открыть журнал уведомлений
            var form = new NotificationLogForm();
            form.Show();
        }

        private void btnExit_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}