<<<<<<< Updated upstream
﻿using System;
using System.Drawing; 
using System.Windows.Forms;

namespace ApartmentRentalSystem.AdminUI.Forms
{
    public class NotificationLogForm : Form  
    {
        // Объявляем элементы как поля класса
        private DataGridView dataGridViewLogs;
        private Button btnBack;

        public NotificationLogForm()
        {
            InitializeComponent();
            LoadMockData();  // Загружаем тестовые данные для демонстрации
=======
﻿#nullable disable

using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediatR;
using ApartmentRentalSystem.Application.Queries.Notifications;
using ApartmentRentalSystem.Core.Enums;

namespace ApartmentRentalSystem.AdminUI.Forms
{
    public class NotificationLogForm : Form
    {
        private readonly IMediator _mediator;
        
        private DataGridView dataGridViewLogs;
        private Button btnBack;

        public NotificationLogForm(IMediator mediator)
        {
            _mediator = mediator;
            InitializeComponent();
            _ = LoadNotificationLogsAsync();
        }

        private async Task LoadNotificationLogsAsync()
        {
            try
            {
                var query = new GetNotificationLogsQuery();
                var logs = await _mediator.Send(query);

                dataGridViewLogs.Rows.Clear();

                foreach (var log in logs)
                {
                    string icon = log.Channel switch
                    {
                        NotificationChannel.Email => "📧",
                        NotificationChannel.Telegram => "💻",
                    };

                    dataGridViewLogs.Rows.Add(
                        log.SentAt.ToString("dd.MM.yyyy HH:mm"),
                        $"{icon} {log.Channel}",
                        log.Recipient,
                        log.Subject,
                        log.Body
                    );
                }

                if (logs.Count == 0)
                {
                    dataGridViewLogs.Rows.Add("Нет данных", "-", "-", "-", "Уведомления пока не отправлялись");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки логов:\n{ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBack_Click(object? sender, EventArgs e)
        {
            this.Close();
>>>>>>> Stashed changes
        }

        private void InitializeComponent()
        {
<<<<<<< Updated upstream
            // Настройка формы
            Text = "Журнал уведомлений";
=======
            Text = "🔔 Журнал уведомлений";
>>>>>>> Stashed changes
            Size = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 245, 245);

<<<<<<< Updated upstream
            // Заголовок
=======
>>>>>>> Stashed changes
            var lblTitle = new Label
            {
                Text = "История отправленных уведомлений",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };

<<<<<<< Updated upstream
            // Таблица журналов
=======
>>>>>>> Stashed changes
            dataGridViewLogs = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(840, 450),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
<<<<<<< Updated upstream
                ReadOnly = true,  // Нельзя редактировать ячейки вручную
=======
                ReadOnly = true,
>>>>>>> Stashed changes
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                ColumnHeadersDefaultCellStyle = { Font = new Font("Segoe UI", 10, FontStyle.Bold) },
                RowHeadersVisible = false
            };

<<<<<<< Updated upstream
            // Колонки таблицы
=======
>>>>>>> Stashed changes
            dataGridViewLogs.Columns.Add("DateSent", "Дата отправки");
            dataGridViewLogs.Columns.Add("Type", "Тип уведомления");
            dataGridViewLogs.Columns.Add("Recipient", "Получатель");
            dataGridViewLogs.Columns.Add("Subject", "Тема");
            dataGridViewLogs.Columns.Add("Body", "Сообщение");
<<<<<<< Updated upstream
            dataGridViewLogs.Columns["Body"].FillWeight = 40; // Делаем текст сообщения шире

            // Кнопка "Назад"
=======
            dataGridViewLogs.Columns["Body"].FillWeight = 40;

>>>>>>> Stashed changes
            btnBack = new Button
            {
                Text = "⬅️ Назад",
                Location = new Point(20, 520),
                Size = new Size(100, 35),
                Cursor = Cursors.Hand
            };
<<<<<<< Updated upstream
            btnBack.Click += btnBack_Click;  // Привязка события

            // !!!
            var lblNote = new Label
            {
                Text = "!!! Подключи DataSource к сервису уведомлений (IUserServiceNotifications)",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                AutoSize = true,
                Location = new Point(20, 560),
                ForeColor = Color.Gray
            };

            // Добавляем все элементы на форму
            Controls.AddRange(new Control[] 
            { 
                lblTitle, dataGridViewLogs, btnBack, lblNote 
            });
        }

        // Загружаем тестовые данные (замени на реальные!!!)
        private void LoadMockData()
        {
            dataGridViewLogs.Rows.Clear();

            dataGridViewLogs.Rows.Add("15.06.2024 10:00", "Email", "anna_k@mail.ru", "Заявка одобрена", "Ваша заявка #abc123 успешно одобрена.");
            dataGridViewLogs.Rows.Add("15.06.2024 10:05", "Telegram", "@user123", "Напоминание", "Ваша бронь начинается через 2 дня.");
            dataGridViewLogs.Rows.Add("14.06.2024 18:30", "SMS", "+79990000000", "Экстренное оповещение", "Изменение правил проживания в связи с ремонтом.");
        }

        // Обработчик кнопки "Назад"
        private void btnBack_Click(object? sender, EventArgs e)
        {
            this.Close();
        }
=======
            btnBack.Click += btnBack_Click;

            Controls.AddRange(new Control[] 
            { 
                lblTitle, dataGridViewLogs, btnBack 
            });
        }
>>>>>>> Stashed changes
    }
}