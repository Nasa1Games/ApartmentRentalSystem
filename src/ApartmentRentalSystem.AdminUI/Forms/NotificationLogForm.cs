using System;
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
        }

        private void InitializeComponent()
        {
            // Настройка формы
            Text = "Журнал уведомлений";
            Size = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 245, 245);

            // Заголовок
            var lblTitle = new Label
            {
                Text = "История отправленных уведомлений",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Таблица журналов
            dataGridViewLogs = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(840, 450),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,  // Нельзя редактировать ячейки вручную
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                ColumnHeadersDefaultCellStyle = { Font = new Font("Segoe UI", 10, FontStyle.Bold) },
                RowHeadersVisible = false
            };

            // Колонки таблицы
            dataGridViewLogs.Columns.Add("DateSent", "Дата отправки");
            dataGridViewLogs.Columns.Add("Type", "Тип уведомления");
            dataGridViewLogs.Columns.Add("Recipient", "Получатель");
            dataGridViewLogs.Columns.Add("Subject", "Тема");
            dataGridViewLogs.Columns.Add("Body", "Сообщение");
            dataGridViewLogs.Columns["Body"].FillWeight = 40; // Делаем текст сообщения шире

            // Кнопка "Назад"
            btnBack = new Button
            {
                Text = "⬅️ Назад",
                Location = new Point(20, 520),
                Size = new Size(100, 35),
                Cursor = Cursors.Hand
            };
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
    }
}