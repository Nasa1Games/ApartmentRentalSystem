<<<<<<< Updated upstream
﻿using System;
using System.Drawing;  
using System.Windows.Forms;

namespace ApartmentRentalSystem.AdminUI.Forms
{
    public class ApartmentsForm : Form  
    {
=======
﻿#nullable disable

using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediatR;
using ApartmentRentalSystem.Application.Queries.Apartments;
using ApartmentRentalSystem.Core.FilterCriteria;
using ApartmentRentalSystem.Core.Enums;

namespace ApartmentRentalSystem.AdminUI.Forms
{
    public class ApartmentsForm : Form
    {
        private readonly IMediator _mediator;
        
>>>>>>> Stashed changes
        private DataGridView dataGridViewApartments;
        private NumericUpDown numericUpDownFloor;
        private NumericUpDown numericUpDownEntrance;
        private NumericUpDown numericUpDownCapacity;
        private Button btnFilter;
<<<<<<< Updated upstream
        private Button btnBack;

        public ApartmentsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Настройка формы
            Text = "Управление квартирами";
            Size = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 245, 245);

            // Заголовок
=======
        private Button btnAddApartment;
        private Button btnEditApartment;
        private Button btnDeleteApartment;
        private Button btnChangeStatus;
        private Button btnBack;

        public ApartmentsForm(IMediator mediator)
        {
            _mediator = mediator;
            InitializeComponent();
            _ = LoadDataAsync();
        }

        // Загрузка данных с Tag для хранения ID
        private async Task LoadDataAsync()
        {
            try
            {
                var criteria = new ApartmentFilterCriteria
                {
                    Status = ApartmentStatus.Available
                };

                var query = new GetApartmentsQuery(criteria);
                var apartments = await _mediator.Send(query);

                dataGridViewApartments.Rows.Clear();

                foreach (var a in apartments)
                {
                    // Добавляем строку с данными
                    int rowIndex = dataGridViewApartments.Rows.Add(
                        a.UnitNumber,
                        a.Floor,
                        a.Entrance,
                        a.Capacity,
                        a.BasePricePerNight.Amount,
                        a.Status.ToString()
                    );
                    
                    dataGridViewApartments.Rows[rowIndex].Tag = a.Id;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnFilter_Click(object? sender, EventArgs e)
        {
            try
            {
                var criteria = new ApartmentFilterCriteria
                {
                    MinFloor = (int)numericUpDownFloor.Value,
                    MaxFloor = 50,
                    MinCapacity = (int)numericUpDownCapacity.Value,
                    Status = ApartmentStatus.Available
                };

                var query = new GetApartmentsQuery(criteria);
                var apartments = await _mediator.Send(query);

                dataGridViewApartments.Rows.Clear();

                foreach (var a in apartments)
                {
                    int rowIndex = dataGridViewApartments.Rows.Add(
                        a.UnitNumber,
                        a.Floor,
                        a.Entrance,
                        a.Capacity,
                        a.BasePricePerNight.Amount,
                        a.Status.ToString()
                    );
                    
                    dataGridViewApartments.Rows[rowIndex].Tag = a.Id;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddApartment_Click(object? sender, EventArgs e)
        {
            using var addForm = new AddApartmentForm(_mediator);
            if (addForm.ShowDialog(this) == DialogResult.OK)
            {
                _ = LoadDataAsync();
            }
        }

        // Редактирование квартиры
        private void btnEditApartment_Click(object? sender, EventArgs e)
        {
            if (dataGridViewApartments.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите квартиру для редактирования", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedRow = dataGridViewApartments.SelectedRows[0];
            
            // Получаем ID из Tag
            if (selectedRow.Tag is not Guid apartmentId)
            {
                MessageBox.Show("Не удалось получить ID квартиры", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var unitNumber = selectedRow.Cells["UnitNumber"]?.Value?.ToString() ?? "";

            using var editForm = new EditApartmentForm(_mediator, apartmentId, unitNumber);
            if (editForm.ShowDialog(this) == DialogResult.OK)
            {
                _ = LoadDataAsync();
            }
        }

        // Удаление квартиры
        private async void btnDeleteApartment_Click(object? sender, EventArgs e)
        {
            if (dataGridViewApartments.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите квартиру для удаления", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Вы уверены, что хотите удалить эту квартиру?\nЭто действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.No)
                return;

            try
            {
                var selectedRow = dataGridViewApartments.SelectedRows[0];
                
                if (selectedRow.Tag is not Guid apartmentId)
                {
                    MessageBox.Show("Не удалось получить ID квартиры", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var command = new ApartmentRentalSystem.Application.Commands.Apartments.DeleteApartmentCommand(apartmentId);
                var success = await _mediator.Send(command);

                if (success)
                {
                    MessageBox.Show("✅ Квартира удалена", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _ = LoadDataAsync();
                }
                else
                {
                    MessageBox.Show("❌ Не удалось удалить квартиру", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Изменение статуса квартиры
        private async void btnChangeStatus_Click(object? sender, EventArgs e)
        {
            if (dataGridViewApartments.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите квартиру", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var selectedRow = dataGridViewApartments.SelectedRows[0];
                
                if (selectedRow.Tag is not Guid apartmentId)
                {
                    MessageBox.Show("Не удалось получить ID квартиры", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var currentStatus = selectedRow.Cells["Status"]?.Value?.ToString() ?? "Available";

                var newStatus = Microsoft.VisualBasic.Interaction.InputBox(
                    "Введите новый статус (Available/Maintenance/Occupied):",
                    "Смена статуса",
                    currentStatus);

                if (string.IsNullOrWhiteSpace(newStatus))
                    return;

                var status = Enum.Parse<ApartmentRentalSystem.Core.Enums.ApartmentStatus>(newStatus);
                var command = new ApartmentRentalSystem.Application.Commands.Apartments.ChangeApartmentStatusCommand(apartmentId, status);
                var success = await _mediator.Send(command);

                if (success)
                {
                    MessageBox.Show("✅ Статус изменён", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _ = LoadDataAsync();
                }
                else
                {
                    MessageBox.Show("❌ Не удалось изменить статус", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBack_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        // Инициализация интерфейса
        private void InitializeComponent()
        {
            Text = "Управление квартирами";
            Size = new Size(1200, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 245, 245);

>>>>>>> Stashed changes
            var lblTitle = new Label
            {
                Text = "Каталог квартир",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };

<<<<<<< Updated upstream
            // Панель фильтров
=======
>>>>>>> Stashed changes
            var lblFilter = new Label
            {
                Text = "Фильтры:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 60),
                AutoSize = true
            };

<<<<<<< Updated upstream
            // Фильтр: Этаж
            numericUpDownFloor = new NumericUpDown
            {
                Location = new Point(100, 58),
                Width = 60,
                Minimum = 1,
                Maximum = 50,
                Value = 1
            };
            var lblFloor = new Label { Text = "Этаж", AutoSize = true, Location = new Point(170, 62) };

            // Фильтр: Подъезд
            numericUpDownEntrance = new NumericUpDown
            {
                Location = new Point(230, 58),
                Width = 60,
                Minimum = 1,
                Maximum = 10,
                Value = 1
            };
            var lblEntrance = new Label { Text = "Подъезд", AutoSize = true, Location = new Point(300, 62) };

            // Фильтр: Вместимость
            numericUpDownCapacity = new NumericUpDown
            {
                Location = new Point(380, 58),
                Width = 60,
                Minimum = 1,
                Maximum = 20,
                Value = 1
            };
            var lblCapacity = new Label { Text = "Вместимость", AutoSize = true, Location = new Point(450, 62) };

            // Кнопка фильтрации
            btnFilter = new Button
            {
                Text = "Применить",
=======
            numericUpDownFloor = new NumericUpDown
            {
                Location = new Point(100, 58), Width = 60, Minimum = 1, Maximum = 50, Value = 1
            };
            var lblFloor = new Label { Text = "Этаж", AutoSize = true, Location = new Point(170, 62) };

            numericUpDownEntrance = new NumericUpDown
            {
                Location = new Point(230, 58), Width = 60, Minimum = 1, Maximum = 10, Value = 1
            };
            var lblEntrance = new Label { Text = "Подъезд", AutoSize = true, Location = new Point(300, 62) };

            numericUpDownCapacity = new NumericUpDown
            {
                Location = new Point(380, 58), Width = 60, Minimum = 1, Maximum = 20, Value = 1
            };
            var lblCapacity = new Label { Text = "Вместимость", AutoSize = true, Location = new Point(450, 62) };

            btnFilter = new Button
            {
                Text = "🔍 Применить",
>>>>>>> Stashed changes
                Location = new Point(570, 55),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
<<<<<<< Updated upstream
            btnFilter.Click += btnFilter_Click;  // Привязка события

            // Таблица квартир
            dataGridViewApartments = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(840, 420),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
=======
            btnFilter.Click += btnFilter_Click;

            btnAddApartment = new Button
            {
                Text = "➕ Добавить",
                Location = new Point(700, 55),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddApartment.Click += btnAddApartment_Click;

            btnEditApartment = new Button
            {
                Text = "Редактировать",
                Location = new Point(830, 55),
                Size = new Size(130, 30),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnEditApartment.Click += btnEditApartment_Click;

            btnChangeStatus = new Button
            {
                Text = "Статус",
                Location = new Point(970, 55),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnChangeStatus.Click += btnChangeStatus_Click;

            btnDeleteApartment = new Button
            {
                Text = "Удалить",
                Location = new Point(1080, 55),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDeleteApartment.Click += btnDeleteApartment_Click;

            dataGridViewApartments = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(1140, 520),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,  
>>>>>>> Stashed changes
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                ColumnHeadersDefaultCellStyle = { Font = new Font("Segoe UI", 10, FontStyle.Bold) }
            };

<<<<<<< Updated upstream
            // Колонки таблицы
            dataGridViewApartments.Columns.Add("UnitNumber", "Номер квартиры");
            dataGridViewApartments.Columns.Add("Floor", "Этаж");
=======
            dataGridViewApartments.Columns.Add("UnitNumber", "Номер квартиры");
            dataGridViewApartments.Columns.Add("Floor", "Этаж");
            dataGridViewApartments.Columns.Add("Entrance", "Подъезд");
>>>>>>> Stashed changes
            dataGridViewApartments.Columns.Add("Capacity", "Вместимость");
            dataGridViewApartments.Columns.Add("Price", "Цена/ночь (₽)");
            dataGridViewApartments.Columns.Add("Status", "Статус");

<<<<<<< Updated upstream
            // Кнопка "Назад"
            btnBack = new Button
            {
                Text = "⬅️ Назад",
                Location = new Point(20, 530),
                Size = new Size(100, 35),
                Cursor = Cursors.Hand
            };
            btnBack.Click += btnBack_Click;  // Привязка события

            // Добавляем все элементы на форму
            Controls.AddRange(new Control[] 
            { 
                lblTitle, lblFilter, lblFloor, numericUpDownFloor, 
                lblEntrance, numericUpDownEntrance, lblCapacity, numericUpDownCapacity, 
                btnFilter, dataGridViewApartments, btnBack 
            });
        }

        // Обработчик кнопки фильтрации
        private void btnFilter_Click(object? sender, EventArgs e)
        {
            // Здесь вызвать сервис с параметрами фильтрации
            // var criteria = new ApartmentFilterCriteria 
            // { 
            //     Floor = (int)numericUpDownFloor.Value,
            //     Entrance = (int)numericUpDownEntrance.Value,
            //     Capacity = (int)numericUpDownCapacity.Value
            // };
            // var apartments = await _apartmentService.GetFilteredAsync(criteria);
            // dataGridViewApartments.DataSource = apartments;

            MessageBox.Show(
                $"Фильтр применён (заглушка)\n" +
                $"Этаж: {numericUpDownFloor.Value}\n" +
                $"Подъезд: {numericUpDownEntrance.Value}\n" +
                $"Вместимость: {numericUpDownCapacity.Value}", 
                "Фильтрация", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information);
        }

        // Обработчик кнопки "Назад"
        private void btnBack_Click(object? sender, EventArgs e)
        {
            this.Close();
        }
=======
            btnBack = new Button
            {
                Text = "⬅️ Назад",
                Location = new Point(20, 630),
                Size = new Size(100, 35),
                Cursor = Cursors.Hand
            };
            btnBack.Click += btnBack_Click;

            Controls.AddRange(new Control[]
            {
                lblTitle, lblFilter, lblFloor, numericUpDownFloor,
                lblEntrance, numericUpDownEntrance, lblCapacity, numericUpDownCapacity,
                btnFilter, btnAddApartment, btnEditApartment, btnChangeStatus, btnDeleteApartment,
                dataGridViewApartments, btnBack
            });
        }
>>>>>>> Stashed changes
    }
}