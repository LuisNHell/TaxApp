using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Threading;
using Windows.Devices.Geolocation;
using System.Device.Location;
using System.Windows.Shapes;
using System.Windows.Media;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Tasks;

namespace TaxApp
{
    public partial class ReporteTaxi : PhoneApplicationPage
    {
        Geolocator geolocator = new Geolocator();

        DispatcherTimer conteo = new DispatcherTimer();
        DispatcherTimer horario = new DispatcherTimer();
        DispatcherTimer RastreoContinuo = new DispatcherTimer();

        bool tracking = false, pulso = false, muestra = false;

        int Intervalo = DateTime.Now.Hour;
        int ContadorContinuo = 0, contarpin = 0;
        int n = 0;

        double banderazo = 0;
        double incremento = 0;
        double distance = 0, distancia = 0;
        double c = 0, a = 0;
        double lat1 = 0, lat2 = 0;
        double lon1 = 0, lon2 = 0;
        double punto1, punto2;

        public const double RadioTierraEquivolumen = 6371; /*Para radio Equivolumen*/
        public const double RadioTierraEcuatorial = 6378; /*Para radio Ecuatorial*/
        public const double RadioTierraPolar = 6357; /*Para radio Polar*/
        public ReporteTaxi()
        {
            InitializeComponent();

            PaginaWeb.Source = new Uri("http://data.semovi.cdmx.gob.mx/wb/stv/Tr%C3%A1mites_y_Servicios.html");

            conteo.Interval = new TimeSpan(0, 0, 0, 1, 1);
            horario.Interval = new TimeSpan(0, 0, 0, 1, 1);
            RastreoContinuo.Interval = new TimeSpan(0, 0, 0, 1, 1);

            conteo.Tick += new EventHandler(conteo_Tick);
            horario.Tick += new EventHandler(horario_Tick);
            RastreoContinuo.Tick += new EventHandler(RastreoContinuo_Tick);

            horario.Start();

            Fecha_Reporte.Text = DateTime.Today.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
        }
        private void horario_Tick(object sender, EventArgs e) /*Evento Timer de Reloj de sistema*/
        {
            Reloj.Text = "TaxApp Fecha: " + DateTime.Today.ToLongDateString() + ",\n";
            Reloj.Text += "Hora: " + DateTime.Now.ToLongTimeString() + " $" + banderazo.ToString("0.00");
        }
        private void conteo_Tick(object sender, EventArgs e) /*Evento Timer de Contador de taximetro por tiempo*/
        {
            //Tarifa.Text = "$" + banderazo.ToString("0.00");
            if (ContadorContinuo > 45)
            {
                banderazo = banderazo + incremento;
                if (Intervalo > 5 && Intervalo < 23)
                {
                    Reloj.Text += "Tarifa 1";
                }

                else
                {
                    Reloj.Text += "Tarifa 2";
                }
                ContadorContinuo = 0;
            }
            Contador.Text = "Tiempo: " + ContadorContinuo.ToString() + " Segundos";
            ContadorContinuo++;
        }
        private void RastreoContinuo_Tick(object sender, EventArgs e) /*Evento Timer de Rastreo Continuo*/
        {
            if (!tracking)
            {
                geolocator.DesiredAccuracy = PositionAccuracy.High; //Obtiene informacion precisa sobre el rastreo
                geolocator.MovementThreshold = 5; //Cada n metros revisa cuando el dispositivo se mueve

                geolocator.StatusChanged += geolocator_StatusChanged;
                geolocator.PositionChanged += geolocator_PositionChanged;

                tracking = true;
                Rastreador.Content = "Detener Rastreo";
            }
            /*else
            {
                geolocator.StatusChanged -= geolocator_StatusChanged;
                geolocator.PositionChanged -= geolocator_PositionChanged;
                //tracking = false;
                Rastreador.Content = "Rastrear";
            }*/
        }

        /*===========================Eventos de Rastreo========================*/

        private void geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            string estado = "";
            switch (args.Status)
            {
                case PositionStatus.Initializing:
                    estado = "Iniciando los Servicios...";
                    break;

                case PositionStatus.Ready:
                    estado = "Servicios Listos...";
                    break;

                case PositionStatus.NotInitialized:
                    estado = "No se obtuvo ningun servicio...";
                    break;

                case PositionStatus.NotAvailable:
                    estado = "Los servicios no estan disponibles...";
                    break;

                case PositionStatus.NoData:
                    estado = "No se encuentra tu Ubicacion...";
                    break;

                case PositionStatus.Disabled:
                    estado = "La ubicacion esta desactivada...";
                    break;
            }
            Dispatcher.BeginInvoke(() =>
            {
                Ubicacion.Text = estado;
            });
        }
        private void geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            Dispatcher.BeginInvoke(() =>
            {
                GeoCoordinate geoposicion = new GeoCoordinate(args.Position.Coordinate.Latitude, args.Position.Coordinate.Longitude);
                /*GeoCoordinate point2 = new GeoCoordinate(geoposicion.Latitude, geoposicion.Longitude);
                GeoCoordinate point1 = new GeoCoordinate(point2.Latitude, point2.Longitude);*/
                GeoCoordinate lat = new GeoCoordinate(geoposicion.Latitude, geoposicion.Longitude);
                GeoCoordinate lon = new GeoCoordinate(geoposicion.Latitude, geoposicion.Longitude);

                Rastreo.Center = geoposicion;
                Rastreo.ZoomLevel = 18;
                AjustarZoom.Value = Rastreo.ZoomLevel;

                Ellipse target1 = new Ellipse();
                target1.Width = 15;
                target1.Height = 15;
                target1.Fill = new SolidColorBrush(Colors.Magenta);

                MapOverlay mapaoverlay = new MapOverlay();
                mapaoverlay.Content = target1;
                mapaoverlay.GeoCoordinate = geoposicion;

                MapLayer PushpinMapLayer = new MapLayer();
                PushpinMapLayer.Add(mapaoverlay);
                Rastreo.Layers.Add(PushpinMapLayer);

                contarpin++;

                if (geoposicion == geoposicion)     /*Obteniendo distancia recorrida*/
                {
                    Contador.Text = "Distancia...";
                    ContadorContinuo = 0;

                    lat2 = geoposicion.Latitude;        /*Asingnando las coordenadas a las variables*/
                    lon2 = geoposicion.Longitude;

                    punto1 = (lat2 - lat1) * (Math.PI / 180);   /*Calculando Distancia*/
                    punto2 = (lon2 - lon1) * (Math.PI / 180);

                    a = Math.Sin(punto1 / 2) * Math.Sin(punto1 / 2) + Math.Cos(lat1 * (Math.PI / 180)) * Math.Cos(lat2 * (Math.PI / 180)) * Math.Sin(punto2 / 2) * Math.Sin(punto2 / 2);
                    c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                    distance = RadioTierraEquivolumen * c;

                    if (contarpin <= 1)
                    {
                        distance = 0;
                        distancia = 0;
                        //distancia = distance; 
                        Ubicacion.Text = "";
                        Distancia.Text = distance.ToString("0.00" + " Metros");
                        lat1 = lat2;
                        lon1 = lon2;
                        n++;
                    }
                    else
                    {
                        distance = distance * 1000;
                        distancia += distance;
                        Distancia.Text = distancia.ToString("0.00" + " Metros");

                        lat1 = lat2;
                        lon1 = lon2;

                        if (distancia < (250 * n))
                        {
                            //Tarifa.Text = banderazo.ToString("$0.00");
                        }
                        else
                        {
                            banderazo += incremento;
                            //Tarifa.Text = banderazo.ToString("$0.00");
                            n++;
                        }
                    }
                    conteo.Start();
                }
                else
                {
                    conteo.Stop();
                }
            });
        }

        /*===============================Eventos Button============================*/
        private void Rastreador_Click(object sender, RoutedEventArgs e)
        {
            if (TaxLib.IsChecked == false && TaxSit.IsChecked == false && TaxRad.IsChecked == false)
            {
                MessageBox.Show("Debe seleccionar una opcion", "¡Alerta!", MessageBoxButton.OK);
            }
            else
            {
                if (pulso == false)
                {
                    RastreoContinuo.Start();
                    pulso = true;
                }

                else
                {
                    geolocator.StatusChanged -= geolocator_StatusChanged;
                    geolocator.PositionChanged -= geolocator_PositionChanged;
                    Rastreador.Content = "Rastrear";
                    RastreoContinuo.Stop();
                    conteo.Stop();
                    contarpin = 0;
                    pulso = false;
                    tracking = false;

                    TaxLib.Visibility = Visibility.Visible;
                    TaxSit.Visibility = Visibility.Visible;
                    TaxRad.Visibility = Visibility.Visible;

                    TaxRad.IsChecked = false;
                    TaxSit.IsChecked = false;
                    TaxLib.IsChecked = false;
                }
            }
        }
        private void Enviar_Informacion_Click(object sender, RoutedEventArgs e)
        {
            var informacion = new EmailComposeTask();
            informacion.Subject = Asunto.Text;      /*Asunto*/
            /*Contenido del correo*/
            informacion.Body = Fecha_Reporte.Text + "\n" + Matricula_Taxi.Text + "\n" + Tipo_Taxi.Text + "\n" + Asunto.Text + "\n" + "\n" + Comentario.Text;
            informacion.To = "";                    /*Destinatario E-Mail principal*/
            informacion.Cc = Mail_Cliente.Text; ;   /*Destinatario con Copia de Carbon*/
            informacion.Bcc = "";                   /*Destinatario con Copia de Carbon Oculta*/
            informacion.Show();                     /*Abre la pantalla de servicio de correo a usar*/

            //Matricula_Taxi.Text = "Placa de Taxi";
            //Tipo_Taxi.Text = "Tipo de Taxi";
            //Cliente.Text = "Cliente";
            //Asunto.Text = "Asunto";
            //Mail_Cliente.Text = "E-Mail del Cliente";
            //Contacto.Text = "Telefono del Cliente";
            //Comentario.Text = "Comentario";
        }

        /*==========================Eventos RadioButton========================*/
        private void TaxLib_Checked(object sender, RoutedEventArgs e)
        {
            TaxRad.Visibility = System.Windows.Visibility.Collapsed;
            TaxSit.Visibility = System.Windows.Visibility.Collapsed;
            if (Intervalo > 5 && Intervalo < 23)    //El horario comprende entre las 6:00:00 am y las 10:59:59 pm
            {
                banderazo = 8.74;
                incremento = 1.07;
                //Reloj.Text += "$" + banderazo.ToString("0.00");
            }

            else
            {
                banderazo = 10.49;//(8.74 * 0.2) + 8.74;
                incremento = 1.28;//(1.07 * 0.2) + 1.07;
                //Reloj.Text += "$" + banderazo.ToString("0.00");
            }
        }
        private void TaxSit_Checked(object sender, RoutedEventArgs e)
        {
            TaxRad.Visibility = System.Windows.Visibility.Collapsed;
            TaxLib.Visibility = System.Windows.Visibility.Collapsed;
            if (Intervalo > 5 && Intervalo < 23)
            {
                banderazo = 13.1;
                incremento = 1.30;
                //Reloj.Text += "$" + banderazo.ToString("0.00");
            }

            else
            {
                banderazo = 15.72; //(13.1 * 0.2) + 13.1;
                incremento = 1.56; //(1.30 * 0.2) + 1.30;
                //Reloj.Text += "$" + banderazo.ToString("0.00");
            }
        }
        private void TaxRad_Checked(object sender, RoutedEventArgs e)
        {
            TaxLib.Visibility = System.Windows.Visibility.Collapsed;
            TaxSit.Visibility = System.Windows.Visibility.Collapsed;
            if (Intervalo > 5 && Intervalo < 23)
            {
                banderazo = 27.3;
                incremento = 1.84;
                //Reloj.Text += "$" + banderazo.ToString("0.00");
            }

            else
            {
                banderazo = 32.76; //(27.30 * 0.2) + 27.30;
                incremento = 2.21; //(1.84 * 0.2) + 1.84;
                //Reloj.Text += "$" + banderazo.ToString("0.00");
            }
        }

        /*==================================Evento Slider=====================*/
        private void AjustarZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (AjustarZoom != null)
            {
                Rastreo.ZoomLevel = Convert.ToInt32(AjustarZoom.Value);
                AjustarZoom.Value = Rastreo.ZoomLevel;
            }
        }

        /*================================Eventos AppBar Button==============================*/
        private void Activar_Wifi_Click(object sender, EventArgs e)
        {
            ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
            connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.WiFi;
            connectionSettingsTask.Show();
        }
        private void Activar_Datos_Click(object sender, EventArgs e)
        {
            ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
            connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.Cellular;
            connectionSettingsTask.Show();
        }
        private void Activar_Avion_Click(object sender, EventArgs e)
        {
            ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
            connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.AirplaneMode;
            connectionSettingsTask.Show();
        }
        private void Activar_Blue_Click(object sender, EventArgs e)
        {
            ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
            connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.Bluetooth;
            connectionSettingsTask.Show();
        }
        private void Texto_Denuncia_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Denuncia.xaml", UriKind.Relative));
            if (muestra == false)
            {
                //MapaTaxi.Visibility = Visibility.Collapsed;
                muestra = true;
            }
            else
            {
                //MapaTaxi.Visibility = Visibility.Visible;
                muestra = false;
            }
        }

        /*=========================Eventos AppBar IconButton===========================*/
        private void Road_Click(object sender, EventArgs e)
        {
            Rastreo.CartographicMode = MapCartographicMode.Road;
        }
        private void Aerial_Click(object sender, EventArgs e)
        {
            Rastreo.CartographicMode = MapCartographicMode.Aerial;
        }
        private void Hybrid_Click(object sender, EventArgs e)
        {
            Rastreo.CartographicMode = MapCartographicMode.Hybrid;
        }
        private void Terrain_Click(object sender, EventArgs e)
        {
            Rastreo.CartographicMode = MapCartographicMode.Terrain;
        }

        /*=====================Eventos MouseEnter de Textbox para borrar texto en un pulso======================*/
        private void Matricula_Taxi_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Matricula_Taxi.Text = "";
        }
        private void Comentario_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Comentario.Text = "";
        }
        private void Cliente_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Cliente.Text = "";
        }
        private void Contacto_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Contacto.Text = "";
        }
        private void Mail_Cliente_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Mail_Cliente.Text = "";
        }
        private void Asunto_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Asunto.Text = "";
        }
        private void Tipo_Taxi_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Tipo_Taxi.Text = "";
        }

        /*===============================Eventos Hyperlink Button=============================*/
        private void Licencia_de_Uso_Click(object sender, RoutedEventArgs e)
        {
            PaginaWeb.Source = new Uri("http://privacy.microsoft.com/es-mx/privacystatement");
        }
        private void Condiciones_de_Uso_Click(object sender, RoutedEventArgs e)
        {
            PaginaWeb.Source = new Uri("https://www.microsoft.com/es-mx/movil/privacy-landing/");
        }
        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            PaginaWeb.Source = new Uri("http://www.transparencia.df.gob.mx/wb/vut/fraccion_x_informacion_de_utilidad_para_setravi");
        }
    }
}