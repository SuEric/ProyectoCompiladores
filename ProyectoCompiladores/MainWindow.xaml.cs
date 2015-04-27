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
        TextRange textRange;

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

            if (analizador_sintactico != null)
            {
                // Pintar diagramas
                PaintClass paintClass = new PaintClass(canvasGrid, analizador_sintactico.Tabla);
                paintClass.PintaClases();
            }
        }

        private void compileButton_Click(object sender, RoutedEventArgs e)
        {

            // Se lee el texto del editorRichTextBox
            textRange = new TextRange(editorRichTextBox.Document.ContentStart, editorRichTextBox.Document.ContentEnd);

            // Entra si hay texto
            if (textRange.Text != null)
            {
                // Entra si hay ruta de archivo
                if (rutaAchivo != null)
                {
                    clasePrueba = textRange.Text;
                    // Creando Analizador Lexico
                    analizador_lexico = new Analizador_lexico(@"C:\Users\Miguel\Documents\ProyectoCompiladores\Fuentes\automata_1.txt");
                    analizador_lexico.cargarAutomata(); // Cargando Automata
                    if (!analizador_lexico.procesa(clasePrueba)) // Evalua clasePrueba
                    {
                        // Creando Analizador Sintactico
                        analizador_sintactico = new Analizador_Sintactico(analizador_lexico);
                        // Creando un Hilo para Analizador Sintactico
                        Thread AS = new Thread(new ThreadStart(analizador_sintactico.ASintactico));
                        AS.Start();
                        AS.Join();
                        if (analizador_sintactico.Errores.Equals(""))
                        {
                            // Creando analizador semántico
                            analizador_semantico = new Analizador_Semantico(analizador_sintactico);
                            analizador_semantico.analiza(nombreArchivo); // Analiza el archivo, necesita saber el nombre del archivo
                            String errores = analizador_semantico.errores; // Se capturan los errores del semántico
                            erroresListBox.Items.Clear(); // Se limpia el ListBox
                            erroresListBox.Items.Add(errores); // Se muestran los errores encontrados
                        }
                        else
                        {
                            erroresListBox.Items.Clear();
                            erroresListBox.Items.Add(analizador_sintactico.Errores);
                        }
                    }
                    else
                    {
                        erroresListBox.Items.Clear();
                        erroresListBox.Items.Add(analizador_lexico.errores);
                    }
                }
                else
                {
                    MessageBox.Show("Tienes que guardar el archivo antes de compilar");
                }
            }
            else
            {
                MessageBox.Show("Debe escribir código o abrir un archivo primero");
            }
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
                textRange = new TextRange(editorRichTextBox.Document.ContentStart, editorRichTextBox.Document.ContentEnd);

                // Carga el archivo al textRange, por ende ya se asignó al editorRichTextBox
                textRange.Load(fileStream, DataFormats.Text);

                fileStream.Close(); // Cierre del archivo
            }
            // Split por . y \ 
            string[] rutas = rutaAchivo.Split(new Char[] { '.' });
            string[] rutas2 = rutas[0].Split(new Char[] { '\\' });

            nombreArchivo = rutas2[(rutas2.Count() - 1)]; // Se captura el nombre del archivo

        }

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

                // Se crea un textRange y asignamos el Editor RichTextBox
                textRange = new TextRange(editorRichTextBox.Document.ContentStart, editorRichTextBox.Document.ContentEnd);
                
                // Creamos FileStream para Salvar.
                FileStream file = new FileStream(saveFileDialog.FileName, FileMode.Create);
                
                textRange.Save(file, DataFormats.Text); // Guardamos el archivo al TextRange

                file.Close();
            }
        }
    }
}
