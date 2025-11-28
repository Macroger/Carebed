using System;
using System.Windows.Forms;
using Carebed.Infrastructure;
using Carebed.Infrastructure.EventBus;
using Carebed.Managers;

namespace Carebed.src
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // I need a tuple to contain eventBus and a list of IManagers.
            var (eventBus, managers, mainDashboard) = SystemInitializer.Initialize();

            // Optional: global exception hooks for UI/background threads
            Application.ThreadException += (s, e) =>
            {
                // TODO: log e.Exception
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                // TODO: log e.Exception
                e.SetObserved();
            };

            // When the form closes we can shutdown services
            mainDashboard.FormClosed += (s, e) => eventBus.Shutdown();

            Application.Run(mainDashboard);
        }
    }
}