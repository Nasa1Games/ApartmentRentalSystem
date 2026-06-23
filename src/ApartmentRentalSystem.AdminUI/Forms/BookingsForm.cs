using System;
<<<<<<< Updated upstream
using System.Drawing;  
using System.Linq;
using System.Windows.Forms;

namespace ApartmentRentalSystem.AdminUI.Forms
{
    public class BookingsForm : Form  
    {
=======
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediatR;
using ApartmentRentalSystem.Application.Queries.Bookings;
using ApartmentRentalSystem.Application.Commands.Bookings;
using ApartmentRentalSystem.Application.Dto;

namespace ApartmentRentalSystem.AdminUI.Forms
{
    public class BookingsForm : Form
    {
        private readonly IMediator _mediator;
>>>>>>> Stashed changes
        private DataGridView dataGridViewBookings;
        private DateTimePicker dtpDateFrom;
        private DateTimePicker dtpDateTo;
        private Button btnMassApprove;
        private Button btnMassReject;
        private Button btnDetails;
        private Button btnBack;

<<<<<<< Updated upstream
        public BookingsForm()
        {
            InitializeComponent();
            LoadMockData();  // Загружаем тестовые данные для демонстрации
        }

        private void InitializeComponent()
        {
            // Настройка формы
=======
        public BookingsForm(IMediator mediator)
        {
            _mediator = mediator;
            InitializeComponent();
            _ = LoadPendingBookingsAsync();
        }

        private async Task LoadPendingBookingsAsync()
        {
            try
            {
                var query = new GetPendingBookingsQuery();
                var bookings = await _mediator.Send(query);
                
                dataGridViewBookings.DataSource = bookings.Select(b => new
                {
                    IdCol = b.BookingId.ToString(),
                    chkSelect = false,
                    Tenant = b.TenantFullName,
                    Apartment = b.ApartmentUnitNumber,
                    Dates = $"{b.CheckInDate:dd.MM} — {b.CheckOutDate:dd.MM}",
                    Nights = (b.CheckOutDate - b.CheckInDate).Days,
                    Total = b.TotalPrice.ToString("N0"),
                    Status = b.Status
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок:\n{ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnMassApprove_Click(object? sender, EventArgs e)
        {
            var selectedIds = dataGridViewBookings.Rows
                .Cast<DataGridViewRow>()
                .Where(row => row.Cells["chkSelect"].Value is bool isChecked && isChecked)
                .Select(row => row.Cells["IdCol"].Value?.ToString())
                .Where(id => Guid.TryParse(id, out _))
                .Select(Guid.Parse)
                .ToList();

            if (!selectedIds.Any())
            {
                MessageBox.Show("Выберите заявки для одобрения!", "Внимание", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                foreach (var id in selectedIds)
                {
                    var command = new ApproveBookingCommand(id);
                    await _mediator.Send(command);
                }
                
                MessageBox.Show($"✅ Одобрено заявок: {selectedIds.Count}", "Успех", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                await LoadPendingBookingsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при одобрении:\n{ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnMassReject_Click(object? sender, EventArgs e)
        {
            var selectedIds = dataGridViewBookings.Rows
                .Cast<DataGridViewRow>()
                .Where(row => row.Cells["chkSelect"].Value is bool isChecked && isChecked)
                .Select(row => row.Cells["IdCol"].Value?.ToString())
                .Where(id => Guid.TryParse(id, out _))
                .Select(Guid.Parse)
                .ToList();

            if (!selectedIds.Any())
            {
                MessageBox.Show("⚠️ Выберите заявки для отклонения!", "Внимание", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var reason = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите причину отклонения:", "Отклонение заявки", "К сожалению, квартира недоступна...");

            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("❌ Причина отклонения не может быть пустой", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                foreach (var id in selectedIds)
                {
                    var command = new RejectBookingCommand(id, reason);
                    await _mediator.Send(command);
                }
                
                MessageBox.Show($"❌ Отклонено заявок: {selectedIds.Count}\nПричина: {reason}", 
                    "Отклонено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                await LoadPendingBookingsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отклонении:\n{ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnDetails_Click(object? sender, EventArgs e)
        {
            if (dataGridViewBookings.SelectedRows.Count == 0)
            {
                MessageBox.Show("⚠️ Выберите строку для просмотра деталей", "Внимание", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var bookingIdStr = dataGridViewBookings.SelectedRows[0].Cells["IdCol"].Value?.ToString();
            
            if (!Guid.TryParse(bookingIdStr, out var bookingId))
            {
                MessageBox.Show("❌ Неверный формат ID брони", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var query = new GetBookingDetailsQuery(bookingId);
                var details = await _mediator.Send(query);
                
                if (details == null)
                {
                    MessageBox.Show("Бронь не найдена", "Ошибка", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                var detailsForm = new BookingDetailsForm(details);
                detailsForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки деталей:\n{ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBack_Click(object? sender, EventArgs e) => this.Close();

        private void InitializeComponent()
        {
>>>>>>> Stashed changes
            Text = "Заявки на бронирование";
            Size = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 245, 245);

<<<<<<< Updated upstream
            // Заголовок
=======
>>>>>>> Stashed changes
            var lblTitle = new Label
            {
                Text = "Управление заявками",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };

<<<<<<< Updated upstream
            // Панель фильтров по датам
=======
>>>>>>> Stashed changes
            var lblDateFilter = new Label
            {
                Text = "Фильтр по датам:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 60)
            };

            dtpDateFrom = new DateTimePicker
            {
<<<<<<< Updated upstream
                Location = new Point(150, 58),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };

            var lblTo = new Label { Text = "—", AutoSize = true, Location = new Point(305, 62) };

            dtpDateTo = new DateTimePicker
            {
                Location = new Point(320, 58),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(30)
=======
                Location = new Point(150, 58), Width = 150, Format = DateTimePickerFormat.Short, Value = DateTime.Today
            };
            var lblTo = new Label { Text = "—", AutoSize = true, Location = new Point(305, 62) };
            dtpDateTo = new DateTimePicker
            {
                Location = new Point(320, 58), Width = 150, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(30)
>>>>>>> Stashed changes
            };

            var btnFilterDates = new Button
            {
                Text = "Фильтр",
                Location = new Point(480, 55),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
<<<<<<< Updated upstream
            btnFilterDates.Click += (_, _) => MessageBox.Show("Фильтрация по датам", "Фильтр!!!");

            // Кнопки массовых действий
            btnMassApprove = new Button
            {
                Text = "Одобрить выбранные",
=======
            btnFilterDates.Click += async (_, _) => await FilterByDatesAsync();

            btnMassApprove = new Button
            {
                Text = "✅ Одобрить выбранные",
>>>>>>> Stashed changes
                Location = new Point(650, 55),
                Size = new Size(160, 30),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
<<<<<<< Updated upstream
            btnMassApprove.Click += btnMassApprove_Click;  // Привязка события

            btnMassReject = new Button
            {
                Text = "Отклонить выбранные",
=======
            btnMassApprove.Click += btnMassApprove_Click;

            btnMassReject = new Button
            {
                Text = "❌ Отклонить выбранные",
>>>>>>> Stashed changes
                Location = new Point(820, 55),
                Size = new Size(160, 30),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
<<<<<<< Updated upstream
            btnMassReject.Click += btnMassReject_Click;  // Привязка события

            // Таблица заявок
=======
            btnMassReject.Click += btnMassReject_Click;

>>>>>>> Stashed changes
            dataGridViewBookings = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(960, 500),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
<<<<<<< Updated upstream
                MultiSelect = true,  // Можно выделять несколько строк
=======
                MultiSelect = true,
>>>>>>> Stashed changes
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                ColumnHeadersDefaultCellStyle = { Font = new Font("Segoe UI", 10, FontStyle.Bold) },
                RowHeadersVisible = false
            };

<<<<<<< Updated upstream
            // Колонка чекбокса (для массовых действий)
=======
>>>>>>> Stashed changes
            var checkCol = new DataGridViewCheckBoxColumn
            {
                HeaderText = "✓",
                Name = "chkSelect",
                Width = 40,
                ReadOnly = false
            };
            dataGridViewBookings.Columns.Add(checkCol);
<<<<<<< Updated upstream

            // Остальные колонки
            dataGridViewBookings.Columns.Add("IdCol", "ID");      // ← Имя "IdCol" для кода
=======
            dataGridViewBookings.Columns.Add("IdCol", "ID");
>>>>>>> Stashed changes
            dataGridViewBookings.Columns.Add("Tenant", "Пользователь");
            dataGridViewBookings.Columns.Add("Apartment", "Квартира");
            dataGridViewBookings.Columns.Add("Dates", "Даты");
            dataGridViewBookings.Columns.Add("Nights", "Ночей");
            dataGridViewBookings.Columns.Add("Total", "Сумма (₽)");
            dataGridViewBookings.Columns.Add("Status", "Статус");

<<<<<<< Updated upstream
            // Кнопки под таблицей
=======
>>>>>>> Stashed changes
            btnDetails = new Button
            {
                Text = "Просмотр деталей",
                Location = new Point(20, 620),
                Size = new Size(180, 40),
                Cursor = Cursors.Hand,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
<<<<<<< Updated upstream
            btnDetails.Click += btnDetails_Click;  // Привязка события
=======
            btnDetails.Click += btnDetails_Click;
>>>>>>> Stashed changes

            btnBack = new Button
            {
                Text = "⬅️ Назад",
                Location = new Point(880, 620),
                Size = new Size(100, 40),
                Cursor = Cursors.Hand
            };
<<<<<<< Updated upstream
            btnBack.Click += btnBack_Click;  // Привязка события

            // напарнику!!!
            var lblNote = new Label
            {
                Text = "Привяжи DataSource к IBookingService.GetPendingBookingsAsync()",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                AutoSize = true,
                Location = new Point(20, 670),
                ForeColor = Color.Gray
            };

            // Добавляем все элементы на форму
            Controls.AddRange(new Control[] 
            { 
                lblTitle, lblDateFilter, dtpDateFrom, lblTo, dtpDateTo, btnFilterDates,
                btnMassApprove, btnMassReject, dataGridViewBookings, btnDetails, btnBack, lblNote 
            });
        }

        // Загружаем тестовые данные (Андрей замени на реальные!!!)
        private void LoadMockData()
        {
            // Очищаем таблицу
            dataGridViewBookings.Rows.Clear();

            // Добавляем заглушки
            dataGridViewBookings.Rows.Add(false, "abc123", "Анна К.", "101", "15.06 — 20.06", "5", "17 500", "⏳ Ожидает");
            dataGridViewBookings.Rows.Add(false, "def456", "Иван П.", "205", "18.06 — 22.06", "4", "14 000", "⏳ Ожидает");
            dataGridViewBookings.Rows.Add(false, "ghi789", "Мария С.", "310", "01.07 — 10.07", "9", "31 500", "⏳ Ожидает");
        }

        // Обработчик: Массовое одобрение
        private void btnMassApprove_Click(object? sender, EventArgs e)
        {
            // Собираем ID выбранных строк
            var selectedIds = dataGridViewBookings.Rows
                .Cast<DataGridViewRow>()
                .Where(row => row.Cells["chkSelect"].Value is bool isChecked && isChecked)
                .Select(row => row.Cells["IdCol"].Value?.ToString())
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();

            if (!selectedIds.Any())
            {
                MessageBox.Show("Выберите заявки для одобрения!", "Внимание", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Здесь вызвать реальный сервис
            // var ids = selectedIds.Select(Guid.Parse).ToList();
            // await _bookingService.ApproveMultipleAsync(ids, CancellationToken.None);
            
            MessageBox.Show(
                $"Одобрено заявок: {selectedIds.Count}\n" +
                $"!!! Здесь вызов IBookingService.ApproveMultipleAsync()\n" +
                $"ID: {string.Join(", ", selectedIds)}", 
                "Успех", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information);

            // Для демо: снимаем галочки
            foreach (var row in dataGridViewBookings.Rows.Cast<DataGridViewRow>())
                row.Cells["chkSelect"].Value = false;
        }

        // Обработчик: Массовое отклонение
        private void btnMassReject_Click(object? sender, EventArgs e)
        {
            var selectedIds = dataGridViewBookings.Rows
                .Cast<DataGridViewRow>()
                .Where(row => row.Cells["chkSelect"].Value is bool isChecked && isChecked)
                .Select(row => row.Cells["IdCol"].Value?.ToString())
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();

            if (!selectedIds.Any())
            {
                MessageBox.Show("Выберите заявки для отклонения!", "Внимание", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // !!! Здесь запросить причину + вызвать сервис
            // var reason = Microsoft.VisualBasic.Interaction.InputBox("Причина отказа:", "Отклонение");
            // await _bookingService.RejectMultipleAsync(ids, reason, CancellationToken.None);
            
            MessageBox.Show(
                $"Отклонено заявок: {selectedIds.Count}\n" +
                $"!!!Здесь вызов IBookingService.RejectMultipleAsync()\n" +
                $"(нужно добавить ввод причины отказа)", 
                "Отклонено", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information);

            // Для демо: снимаем галочки
            foreach (var row in dataGridViewBookings.Rows.Cast<DataGridViewRow>())
                row.Cells["chkSelect"].Value = false;
        }

        // Обработчик: Просмотр деталей
        private void btnDetails_Click(object? sender, EventArgs e)
        {
            if (dataGridViewBookings.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите строку для просмотра деталей", "Внимание", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Получаем ID из первой выделенной строки
            var bookingId = dataGridViewBookings.SelectedRows[0].Cells["IdCol"].Value?.ToString();
            
            // Открываем модальное окно деталей
            var detailsForm = new BookingDetailsForm(bookingId);
            detailsForm.ShowDialog(this);  // ← Модально: блокирует родительскую форму
        }

        // Обработчик: Кнопка "Назад"
        private void btnBack_Click(object? sender, EventArgs e)
        {
            this.Close();
=======
            btnBack.Click += btnBack_Click;

            Controls.AddRange(new Control[] 
            { 
                lblTitle, lblDateFilter, dtpDateFrom, lblTo, dtpDateTo, btnFilterDates,
                btnMassApprove, btnMassReject, dataGridViewBookings, btnDetails, btnBack 
            });
        }

        private async Task FilterByDatesAsync()
        {
            try
            {
                await LoadPendingBookingsAsync();
                MessageBox.Show($"Фильтр: {dtpDateFrom.Value:dd.MM.yyyy} — {dtpDateTo.Value:dd.MM.yyyy}", 
                    "Фильтр по датам", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации:\n{ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
>>>>>>> Stashed changes
        }
    }
}