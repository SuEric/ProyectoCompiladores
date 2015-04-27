using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// Librerias usadas aparte
using WinForms = System.Windows.Forms;
using UIOpenFileDialog = System.Windows.Forms.OpenFileDialog;
using System.IO;
using System.Threading;
using Microsoft.Win32;

namespace ProyectoCompiladores
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        String clasePrueba;
        String rutaAchivo;
        String nombreArchivo;
        Analizador_lexico analizador_lexico;
        Analizador_Sintactico analizador_sintactico;
        Analizador_Semantico analizador_semantico;
        public MainWindow()
        {
            InitializeComponent();

            editorRichTextBox.Document.PageWidth = 1000; // Sin esto no hay Scrolling horizontal
        }

        private void editorRichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // Punteros
            TextPointer tp1 = editorRichTextBox.Selection.Start.GetLineStartPosition(0);
            TextPointer tp2 = editorRichTextBox.Selection.Start;

            int columna = tp1.GetOffsetToPosition(tp2); // Columna, posición del cursor
            int linea; // Variable para guardar la línea

            // Posicion de la línea
            editorRichTextBox.Selection.Start.GetLineStartPosition(-int.MaxValue, out linea);

            // Mostrar posición cursor en el Label del StatusBar
            cursorPositionLabel.Content = "Línea " + (-linea + 1) + ", Columna " + (columna);
        }

        private void diagramButton_Click(object sender, RoutedEventArgs e)
        {
            // Cambiar de pestaña a diagramTabItem
            ventanasTabControl.SelectedIndex = 1;
            ventanasTabControl.SelectedItem = diagramTabItem;
            diagramTabItem.IsSelected = true;

            // Pintar diagramas
            PaintClass paintClass = new PaintClass(canvasGrid, analizador_sintactico.Tabla);
            paintClass.PintaClases();
        }

        private void compileButton_Click(object sender, RoutedEventArgs e)
        {
            // Acá se hará todos los procesos de analisis

            if (clasePrueba != null)
            {
                // Creando Analizador Lexico
                analizador_lexico = new Analizador_lexico(@"C:\Users\sueric16\Documents\GitHub\ProyectoCompiladores\Fuentes\automata_1.txt");
                analizador_lexico.cargarAutomata(); // Cargando Automata
                analizador_lexico.procesa(clasePrueba); // Evalua clasePrueba
                
                // Creando Analizador Sintactico
                analizador_sintactico = new Analizador_Sintactico(analizador_lexico);
                
                // Creando un Hilo para Analizador Sintactico
                Thread AS = new Thread(new ThreadStart(analizador_sintactico.ASintactico));
                AS.Start();
                AS.Join();
                analizador_semantico = new Analizador_Semantico(analizador_sintactico);

                // Obtenemos el nombre del archivo
                string[] rutas = rutaAchivo.Split(new Char[] { '.' });
                string[] rutas2 = rutas[0].Split(new Char[] { '\\' });

                nombreArchivo = rutas2[(rutas2.Count() - 1)];

                analizador_semantico.analiza(nombreArchivo);
            }
            else
            {
                MessageBox.Show("Debe escribir código o abrir un archivo primero");
            }

            // Errores
            String errores = analizador_semantico.errores;
            MessageBox.Show(nombreArchivo);
            erroresListBox.Items.Add(errores);
        }

        private void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            UIOpenFileDialog openFileDialog = new UIOpenFileDialog(); // Chooser de archivos

            // Configuración del chooser
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog.Filter = "txt files (*.txt)|*.txt|cs files(*.cs)|*.cs";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                rutaAchivo = openFileDialog.FileName;

                // Objeto lectura de archivo
                FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.OpenOrCreate);

                // Objeto textRange y editorRichTextBox destino
                TextRange textRange = new TextRange(editorRichTextBox.Document.ContentStart, editorRichTextBox.Document.ContentEnd);

                // Carga el archivo al textRange, por ende ya se asignó al editorRichTextBox
                textRange.Load(fileStream, DataFormats.Text);
                clasePrueba = textRange.Text;

                fileStream.Close(); // Cierre del archivo
            }
        }
        // Botón que nos permite guardar cambios.

        private void guardarButton_Click(object sender, RoutedEventArgs e)
        {
            // Se crea un SaveFileDialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // Configuración del chooser
            saveFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            saveFileDialog.Filter = "txt files (*.txt)|*.txt|cs files(*.cs)|*.cs";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == true)
            {
                rutaAchivo = saveFileDialog.FileName; // Se guarda la ruta del Archivo
                nombreArchivo = saveFileDialog.FileNames[0];

                // Se crea un textRange y asignamos el Editor RichTextBox
                TextRange textRange = new TextRange(editorRichTextBox.Document.ContentStart,
                                editorRichTextBox.Document.ContentEnd);
                
                // Creamos FileStream para Salvar.
                FileStream file = new FileStream(saveFileDialog.FileName, FileMode.Create);
                
                textRange.Save(file, DataFormats.Text); // Guardamos el archivo al TextRange
                
                clasePrueba = textRange.Text; // Se Guarda la cadena a Analizar

                file.Close();

            }

        
         

        }
    }
}
