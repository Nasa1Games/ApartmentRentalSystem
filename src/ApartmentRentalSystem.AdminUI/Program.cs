using System;
<<<<<<< Updated upstream
using System.Windows.Forms;
=======
using System.IO;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MediatR;
using ApartmentRentalSystem.Application;
using ApartmentRentalSystem.Infrastructure;
using ApartmentRentalSystem.AdminUI.Forms;
>>>>>>> Stashed changes

namespace ApartmentRentalSystem.AdminUI
{
    static class Program
    {
<<<<<<< Updated upstream
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
=======
        [STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning);
            });
            
            // Регистрируем слои
            services.AddApplication();
            services.AddInfrastructure(configuration);
            
            // Регистрируем все формы
            services.AddTransient<MainForm>();
            services.AddTransient<ApartmentsForm>();
            services.AddTransient<AddApartmentForm>();
            services.AddTransient<EditApartmentForm>();
            services.AddTransient<BookingsForm>();
            services.AddTransient<BookingDetailsForm>();
            services.AddTransient<NotificationLogForm>();

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var mainForm = scope.ServiceProvider.GetRequiredService<MainForm>();
            
            System.Windows.Forms.Application.Run(mainForm);
>>>>>>> Stashed changes
        }
    }
}