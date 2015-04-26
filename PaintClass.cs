using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;


namespace ProyectoCompiladores
{
    class PaintClass
    {
        Grid canvasGrid;

        TablaSimbolos tablaSimbolos;

        public PaintClass(Grid canvasGrid, TablaSimbolos tablaSimbolos)
        {
            this.canvasGrid = canvasGrid;
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
    }
}
