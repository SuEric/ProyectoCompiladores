using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using WinForms = System.Windows.Forms;
using UIOpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace ProyectoCompiladores
{
    public class Analizador_lexico
    {
        string TextPrueba;
        int[,] tabla_transiciones; // tabla de estados
        string[] lexema; // Lexema
        string archivo; // Archivo a analizar
        int columnas; // Número de columnas
        int filas; // Número de filas
        Dictionary<char, int> simbolos; // Alfabeto
        List<string> palabras_reservadas; // Palabras reservadas
        public static int count;
        List<string> tokens; // Tokens encontrados
        static bool errores;
        public Analizador_lexico (string archivo)
        {
            this.archivo = archivo;
            columnas = 0;
            filas = 0;
            count = 0;
            // Cambiar palabras reservadas, hacerlo desde archivo
            palabras_reservadas = new List<string> {"if", "begin", "for", "while","end"};

            // Lista tokens vacía
            tokens = new List<string>();
        }

        public void showReservedWords()
        {
            foreach(string palabra in palabras_reservadas)
            {
                MessageBox.Show(palabra);
            }
            
        }
        public int Count { get; set; }
        public string is_reservedId(string valor)
        {
            foreach(string reserved in palabras_reservadas)
            {
                if (valor.Equals(reserved))
                {
                    return "PalabraReservada";
                }
            }
            return "Id";
        }
        public int getReservedWordsLength() {
            return palabras_reservadas.ToArray().Length;
        }

        public string getReservedWord(int index) {
            return palabras_reservadas[index];
        }

        public void addToken(string token) {
            tokens.Add(token);
        }

        public void clearTokens() {
            tokens.Clear();
        }

        public List<string> getFoundTokens() {
            return tokens;
        }

        public void countStatesInFile() //cuenta el numero de estados
        {
            StreamReader file = new StreamReader(@archivo); //Abre un stream para lectura del archivo
            string line = file.ReadLine(); //Lee una linea
            while ((line = file.ReadLine()) != null)//Mientras la linea leida no sea nula
            {
                while (line.Split(new Char[] { ' ' })[0] == "") //Si encuentra una linea vacia
                {
                    line = file.ReadLine();                     //Avanza a la siguiente linea
                }
                if ((line.Split(new Char[] { ' ' }).Length == 1))
                {
                    break;
                }
                filas++;                                        //Aumenta el contador de filas
            }
             file.Close();
            
        }

        public void cargarAutomata() {
            int counter = 0;
            countStatesInFile(); // Número de estados en autómata
            StreamReader file = new StreamReader(archivo); //Abre un stream para lectura del archivo
            string line = file.ReadLine(); //Lee una linea

            // Lectura de primera línea (Alfabeto)
            string[] arreglo = line.Split(new Char []{' '});
            
            columnas = arreglo.Length + 2; // Número de columnas
            
            // Inicializado tamaño tabla de estados
            tabla_transiciones = new int[filas, columnas - 1];

            lexema = new string[filas]; // Lexema

            // Construcción alfabeto desde el archivo
            simbolos = new Dictionary<char, int>();
            for (int i = 0; i < columnas - 2; i++)
                simbolos.Add(Convert.ToChar(arreglo[i]), i);

            /********* Símbolos a ignorar *********/
            simbolos.Add(' ', columnas - 3);
            simbolos.Add('\n', columnas - 3);
            simbolos.Add('\t', columnas - 3);
            simbolos.Add('\r', columnas - 3);
            /********* Símbolos a ignorar *********/

            // Leer todo el autómata
            while ((line = file.ReadLine()) != null)
            {
                // Leer línea por línea
                arreglo = line.Split(new Char[] { ' ' });
                // Saltar líneas vacías
                while (arreglo[0] == "")
                {
                    line = file.ReadLine();
                    arreglo = line.Split(new Char[] { ' ' });
                }
                if (arreglo.Length == 1) //guarda las palabras reservadas
                {
                    palabras_reservadas.Add(arreglo[0]);
                }
                else
                {
                    // Guardar estados leidos del archivo en la tabla
                    for (int i = 0; i < columnas - 1; i++)
                    {
                        tabla_transiciones[counter, i] = Convert.ToInt16(arreglo[i]);
                    }
                    // Lexema (Tipos)
                    lexema[counter] = arreglo[columnas - 1];

                    counter++; // Incrementa número de estado
                }
            }

            file.Close();
        }

        public bool isFinalState(int state)
        {
            if (tabla_transiciones[state, columnas - 2] == -1)
                return false;

            return true;
        }

        public bool hasBackspace(int state)
        {
            if (tabla_transiciones[state, columnas - 2] == 1)
                return true;
            return false;
        }

        public string getLexema(int state)
        {
            return lexema[state];
        }

        public bool isLetra(char c)
        {
            int ascii = c;
            if ((ascii >= 65 && ascii <= 90) || (ascii >= 97 && ascii <= 122))
                return true;
            return false;
        }

        public bool isDigit(char c)
        {
            int ascii = c;
            if (ascii >= 48 && ascii <= 57)
            {
                return true;
            }
            return false;
        }

        public int getState(int state, char c)
        {
            int value;
            // Es letra?
            if (isLetra(Convert.ToChar(c)))
            {
                return tabla_transiciones[state, simbolos['L']];
            }
            // Es Digito?
            if (isDigit(Convert.ToChar(c)))
            {
                return tabla_transiciones[state, simbolos['N']];
            }
            // Sino está en el alfabeto, regresar error
            if (!simbolos.TryGetValue(c, out value))
            {
                return -999;
            }
            // Regresa estado siguiente
            return tabla_transiciones[state, simbolos[c]];
        }

        public string sigToken()
        {
            if (count < tokens.ToArray().Length)
            {
                return tokens[count++];
            }
            else
            {
                count = 0;
                return "$";
            }
        }
        public void procesa(String TextPrueba)
        {
            string textTest = TextPrueba + " "; //Se agrega un espacio al final del archivo a leer para que finalice las lecturas
            int state = 0;                           //Se asigna estado inicial 0
            string valor = "";                      //Va almacenando los caracteres del lexema, se guaradara en una lista al final junto con su tipo

            for (int i = 0; i < textTest.Length; i++)//Se lee caracter a caracter el texto a revisar
            {
                char c = textTest[i];    //Extrae caracter   
                int ascii = c;          //Guarda el valor ascii del rocken encontrado

                if ((ascii == 32 || ascii == 13 || ascii == 9 || ascii == 10) && state == 0)
                {
                    continue;            //En caso de encontrar espacio en blanco, tabulador, nueva linea o retorno de carro estando en el estado inicial lee el sig caracter
                }
                valor = valor + c;
                state = getState(state, c); //Se mueve al siguiente estado de acuerdo con el caracter leido
                if (state == -999)
                {
                    errores = true;
                    state = 0;
                    MessageBox.Show("Token no reconocido: '"+valor+"'");     //Error de tocken no reconocido
                    continue;
                }
                              //En caso de ser un tocken compuesto de mas de un caracter almacena el siguiente caracter
                if (state == -1)
                {
                    errores = true;
                    state = 0;
                    MessageBox.Show("error lexico en " + valor);
                    break;
                }
                if (isFinalState(state))   //Pregunta si el estado actual ya es un estado final para proceder a almacenarlo
                {
                    if (hasBackspace(state)) //Se verifica si tiene retroceso
                    {
                        valor = valor.Remove(valor.Length - 1); //Se quita el ultimo caracter del valor almacenado
                        i--;                       //Retrocede para la lectura
                    }

                    if (getLexema(state).Equals("Id")) //Censa si el estado es de un Id
                    {
                        addToken(is_reservedId(valor) + " " + valor); //Agrega el lexema separado por su valor,si es una palabra reservada ella misma será el lexema
                    }
                    else
                    {
                        addToken(getLexema(state) + " " + valor);                     //Agrega el lexema separado por su valor
                    }
                   // MessageBox.Show("Lexema: " + getLexema(state) + "\nValor: " + valor + " " + is_reservedId(valor)); //Muestra el lexema y su valor
                    valor = "";                                                             //Se limpia la variable valor para empezar a guardar una nueva cadena
                    state = 0;                                                              //El estado se vuelve el inicial
                }
            }
        }
    }
}

