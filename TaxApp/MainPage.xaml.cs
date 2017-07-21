using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TaxApp.Resources;
using Microsoft.Phone.Tasks;
using Windows.System;

namespace TaxApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        bool pulso = false;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Código de ejemplo para traducir ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private void TaxApp_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ReporteTaxi.xaml", UriKind.Relative));
        }

        private void Informacion_Click(object sender, RoutedEventArgs e)
        {
            if (pulso == false)
            {
                Info.Visibility = System.Windows.Visibility.Visible;
                pulso = true;
                var Enlace1 = Launcher.LaunchUriAsync(new Uri("http://www.semovi.cdmx.gob.mx/tramites-y-servicios/transporte-de-pasajeros/nuevas-tarifas-de-transporte-publico-vigentes"));
                //var Enlace2 = Launcher.LaunchUriAsync(new Uri("http://data.semovi.cdmx.gob.mx/wb/stv/Tr%C3%A1mites_y_Servicios.html"));
            }
            else
            {
                Info.Visibility = System.Windows.Visibility.Collapsed;
                pulso = false;
            }
        }

        // Código de ejemplo para compilar una ApplicationBar traducida
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Establecer ApplicationBar de la página en una nueva instancia de ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Crear un nuevo botón y establecer el valor de texto en la cadena traducida de AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Crear un nuevo elemento de menú con la cadena traducida de AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}