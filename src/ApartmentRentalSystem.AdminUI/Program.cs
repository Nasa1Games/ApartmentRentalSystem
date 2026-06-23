using System;
using System.Windows.Forms;

namespace ApartmentRentalSystem.AdminUI
{
    static class Program
    {
        // Обязательно для WinForms (корректная работа с UI-потоком)
        [STAThread]
        static void Main()
        {
            // Включает современные стили Windows (кнопки, чекбоксы, скроллбары)
            Application.EnableVisualStyles();
            
            // Отключает устаревший рендеринг текста 
            Application.SetCompatibleTextRenderingDefault(false);
            
            //  Запускает главное окно и цикл обработки событий
            Application.Run(new MainForm());
        }
    }
}