using System;
using System.Drawing;  
using System.Windows.Forms;

namespace ApartmentRentalSystem.AdminUI.Forms
{
    public class ApartmentsForm : Form  
    {
        private DataGridView dataGridViewApartments;
        private NumericUpDown numericUpDownFloor;
        private NumericUpDown numericUpDownEntrance;
        private NumericUpDown numericUpDownCapacity;
        private Button btnFilter;
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
            var lblTitle = new Label
            {
                Text = "Каталог квартир",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Панель фильтров
            var lblFilter = new Label
            {
                Text = "Фильтры:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 60),
                AutoSize = true
            };

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
                Location = new Point(570, 55),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnFilter.Click += btnFilter_Click;  // Привязка события

            // Таблица квартир
            dataGridViewApartments = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(840, 420),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                ColumnHeadersDefaultCellStyle = { Font = new Font("Segoe UI", 10, FontStyle.Bold) }
            };

            // Колонки таблицы
            dataGridViewApartments.Columns.Add("UnitNumber", "Номер квартиры");
            dataGridViewApartments.Columns.Add("Floor", "Этаж");
            dataGridViewApartments.Columns.Add("Capacity", "Вместимость");
            dataGridViewApartments.Columns.Add("Price", "Цена/ночь (₽)");
            dataGridViewApartments.Columns.Add("Status", "Статус");

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
                $"🚧 Фильтр применён (заглушка)\n" +
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
    }
}