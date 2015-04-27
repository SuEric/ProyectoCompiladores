using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;


namespace ProyectoCompiladores
{
    class PaintClass
    {
        Grid canvasGrid;

        TablaSimbolos tablaSimbolos;
        int contadorClases; 

        public PaintClass(Grid canvasGrid, TablaSimbolos tablaSimbolos)
        {
            this.canvasGrid = canvasGrid;
            contadorClases = 0;
            this.tablaSimbolos = tablaSimbolos;
        }

        public void pintarClase(String nombreClase, int numAtributos, int numMetodos)
        {
            Grid claseGrid = new Grid();

            RowDefinition rowClase = new RowDefinition();
            rowClase.Height = new GridLength(15, GridUnitType.Star);
            
            RowDefinition rowAtributos = new RowDefinition();
            rowAtributos.Height = new GridLength(numAtributos * 15, GridUnitType.Star);

            RowDefinition rowMetodos = new RowDefinition();
            rowMetodos.Height = new GridLength(numMetodos * 15, GridUnitType.Star);

            claseGrid.RowDefinitions.Add(rowClase);
            claseGrid.RowDefinitions.Add(rowAtributos);
            claseGrid.RowDefinitions.Add(rowMetodos);

            Label nombreClaseLabel = new Label();
            nombreClaseLabel.Content = nombreClase;

            Label atributosLabel = new Label();
            String texto = "";
            for (int i = 0; i < numAtributos; i++)
            {
 
            }
            atributosLabel.Content = texto;


            posicionarElemento(nombreClaseLabel, 0, 0);

            claseGrid.Children.Add(nombreClaseLabel);

            posicionarElemento(claseGrid, 0, 2);
            canvasGrid.Children.Add(claseGrid);
        }

        public void posicionarElemento(UIElement element, int fila, int columna)
        {
            Grid.SetRow(element, fila);
            Grid.SetColumn(element, columna);
        }

        public void PintaClases()
        {
            int columna = 0;
            foreach (Clase_valores clase in tablaSimbolos.Clases)
            {
                // Obtenemos Número de Atributos y Número de Métodos
                int numAtributos = clase.Variables.Count();
                int numMetodos = clase.Metodos.Count();
                String mod = "protected"; // Obteniendo el Modificador de la clase

                // Creamos un Grid para Seccionar Nombre de Clase, Atributos, Métodos
                Grid claseGrid = new Grid();

                // Fila y configuracion de Tamaño para el Nombre de la clase 
                RowDefinition rowClase = new RowDefinition();
                rowClase.Height = new GridLength(25, GridUnitType.Star);
                // Fila y Configuración para los Atributos de la Clase
                RowDefinition rowAtributos = new RowDefinition();
                rowAtributos.Height = new GridLength(numAtributos * 15, GridUnitType.Star);
                // Fila y Configuración para los Métodos de la Clase
                RowDefinition rowMetodos = new RowDefinition();
                rowMetodos.Height = new GridLength(numMetodos * 15, GridUnitType.Star);

                // Agregando las Filas al Grid de la Clase
                claseGrid.RowDefinitions.Add(rowClase);
                claseGrid.RowDefinitions.Add(rowAtributos);
                claseGrid.RowDefinitions.Add(rowMetodos);
                

                Label nombreClaseLabel = new Label();
                switch (clase.Mod)
                {
                    case "public":
                        mod = "+";
                        break;   
                    case "private":
                        mod = "-";
                        break;   
                    case "protected":
                        mod = "&";
                        break;      
                }

                string extends = "";
                if (clase.Extend != "") extends = "extends";

                nombreClaseLabel.Content = mod + " " + clase.Id + " " + extends + clase.Extend;

                Label atributosLabel = new Label();
                String texto = "";

                int contador = 0;
                foreach (Variable variable in clase.Variables)
                {
                    string constante = "", igual = "";
                    if (variable.IsConst) { constante = "const"; igual = " = "; }

                    texto = texto + variable.Mod + " " + constante + " " + variable.Tipo + " " + variable.Id + igual + variable.Valor + ";";

                    if (contador < numAtributos) texto = texto + "\n";
                    contador++;
                }

                atributosLabel.Content = texto;

                // METODOS

                Label metodosLabel = new Label();
                texto = "";
                contador = 0;
                foreach (Metodo metodo in clase.Metodos)
                {
                    texto = metodo.Mod + " " + metodo.Id + "(" + ")" + ";";

                    if (contador < numAtributos) texto = texto + "\n";
                    contador++;
                }

                metodosLabel.Content = texto;

                posicionarElemento(nombreClaseLabel, 0, 0);
                posicionarElemento(atributosLabel, 1, 0);
                posicionarElemento(metodosLabel, 2, 0);

                claseGrid.Children.Add(nombreClaseLabel);
                claseGrid.Children.Add(atributosLabel);
                claseGrid.Children.Add(metodosLabel);


                int row = contadorClases / 6; // Obtenemos fila

                if (contadorClases == 0) {
                    columna = 0;
                }
                else {
                    columna += 2;
                }

                if (contadorClases % 6 == 0 && contadorClases > 5) columna = 0;

                Line line = new Line();
                line.X1 = 0;
                line.Y1 = 100;
                line.X2 = 100;
                line.Y2 = 100;

                // Create a red Brush
                SolidColorBrush redBrush = new SolidColorBrush();
                redBrush.Color = Colors.Red;

                // Set Line's width and color
                line.StrokeThickness = 5;
                line.Stroke = redBrush;

                // Add line to the Grid.
                
                posicionarElemento(claseGrid, row, columna);
                posicionarElemento(line, row, columna+1);
                canvasGrid.Children.Add(claseGrid);

                canvasGrid.Children.Add(line);
                contadorClases++; // Aumentar contador de clases
            }
        }
    }
}
