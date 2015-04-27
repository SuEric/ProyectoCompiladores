using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace ProyectoCompiladores
{
    class Analizador_Sintactico
    {
        // Atributos
        String [] token = new String[2]; //Guarda los tockens actuales
        Analizador_lexico analizador_lexico; //Instancia del Analizador lexico que se usa para obtener los tockens
        Clase_valores nueva_clase;          // Instancia de clase que guardará los metodos y variables que contiene
        Variable nueva_variable;            //Guarda la lista de variables de una clase
        Metodo nuevo_metodo;                //Guarda la lista de metodos de una clase
        TablaSimbolos tabla;                //Contiene la lista de clases del archivo
        static Boolean errores;             //Indica la existencia de errores en caso de ser true
        List<string> vars;                  //lista las variables que encuentra en declaracion de const
        string aux_mod , aux_tipo;

        public TablaSimbolos Tabla
        {
            get { return tabla; }
            //set { clases = value; }
        }

        // Constructor inicializado con el Analizador Léxico y primer Token
        public Analizador_Sintactico(Analizador_lexico analizador_lexico)
        {
            tabla = new TablaSimbolos();
            errores = false;
            nueva_clase = new Clase_valores();
            nueva_variable = new Variable();
            nuevo_metodo = new Metodo();
            this.analizador_lexico = analizador_lexico;
            String aux = analizador_lexico.sigToken();
            token = aux.Split(null);
            vars = new List<string>();
        }
        // Funcion Inicial
        public void ASintactico()
        {
            //Simbolo inicial
            Clase();
        }
        public bool Clase()
        {
            // Si no es Lambda
            if (!token[0].Equals("$"))
            {
                // Predict ( Clase -> Mod class Id Ext { Cont } Clase )
                // =  { public, protected, private, class }
                nueva_clase.Mod = (token[1].Equals("class")) ? "protected" : token[1]; //Si Mod es lambda el default es protected
                Mod(); // Llamada a Mod para consumir el modo
                Consumir("class", 1);                                                   
                nueva_clase.Id = token[1];                                              //Guarda el Id de la clase
                Consumir("Id", 0);
                Ext();                                                                  //Entra a consumir extend y su id
                tabla.Clases.Add(nueva_clase);                                          //La información necesaria de la clase se guarda en la tabla
                Consumir("{", 1);
                Cont();                                                                 //Entra a revisar el contenido de la clase
                Consumir("}", 1);
                nueva_clase = new Clase_valores();
                Clase();                                                                // Recursión
            }
            else
            {
                Consumir("$",0);                                                        //Llega al final
                MessageBox.Show("El analizador sintactico terminó su ejecución");
            }
            return errores;
        }
        //Consume el extend y asigna el valor a la clase
        
        public bool Ext()
        {
            // Predict ( Ext -> extends Id )
            // =  { extends }
            if (token[1].Equals("extends"))
            {
                Consumir("extends", 1);
                nueva_clase.Extend = token[1];      //Asigna a la clase el Id de quien es extendido
                Consumir("Id", 0);
            }
            if (!(token[1].Equals("{")))
            {
                errores = true;
                MessageBox.Show("Error: Se esperaba alguno de 'extends'");
                // ES LAMBDA, no hacer nada
                return false;
            }
            return true;
        }
        public void Cont()
        {
            // Predict ( Cont -> Mod M' )
            // =  { public, protected, private, int, float, char, const, void, Id }
            if (token[0].Equals("Id") || token[1].Equals("public") || token[1].Equals("private") || token[1].Equals("protected") || token[1].Equals("int") || token[1].Equals("float") || token[1].Equals("char") || token[1].Equals("const") || token[1].Equals("void"))
            {
                aux_mod = token[1];                                 //Guarda el posible Mod 
                if(Mod())                                       //Intenta consumir el Mod    
                {
                    nueva_variable.Mod = aux_mod;
                    nuevo_metodo.Mod = aux_mod;//En caso de Consumir correctamente el Mod lo asigna a metodo y a variable
                }
                M1();                                           //Avanza a la primera recursión donde puede haber variables o firmas
            }
            else
            {
                // Predict ( Cont -> Lambda )
                // =  { '}' }
                if (!token[1].Equals("}"))      //Pregunta por el predict de la produccion de lambda 
                {
                    MessageBox.Show("Error: Se esperaba alguno de 'public', 'protected', 'private', 'int', 'float', 'char', 'const', 'void'"); //No encontro ningun token esperado
                }
            }
        }
        //Funcion que consume los Modos básicos  public private 
        public bool Mod()
        {
            // Predict ( Mod -> public | private | protected)
            // =  { public } {private} {protected}
            if (token[1].Equals("public") | token[1].Equals("private") | token[1].Equals("protected"))
            {
                Consumir(token[1], 1);
                return true;
            }
            else 
            {
                        // Predict ( Mod -> LAMBDA )
                        // =  { class, int, float, char, const, void, Id } }
                        if (!(token[1].Equals("class") || token[1].Equals("int") || token[1].Equals("float") || token[1].Equals("char") || token[1].Equals("const") || token[1].Equals("void") || token[0].Equals("Id")))
                        {
                            errores = true;
                            MessageBox.Show("Error: Se esperaba alguno de 'public', 'private', 'protected'");
                            // ES LAMBDA, no hacer nada
                            return false;
                        }       
            }
            return false;
        }
        //consume valores int,char,float o Id
        public void Tipo1()
        {
            // Predict ( Tipo' -> Tipo )
            // =  { int, float , char }
            if (token[1].Equals("int") || token[1].Equals("float") || token[1].Equals("char"))
            {
                Tipo();             //Pasa a Consumir tipo int, char o float
            }
            else
            {
                // Predict ( Tipo' -> Id )
                // =  { Id }
                if (token[0].Equals("Id"))
                {
                    Consumir("Id", 0); //Consume el "tipo" Id
                }
                else
                {
                    errores = true;                                                                 //Marca la varible de errores detectados
                    MessageBox.Show("Error: Se esperaba alguno de 'int', 'float', 'char', 'Id'");   //Mensaje de error
                    Thread.CurrentThread.Abort();                                                   //Termina la ejecución del hilo
                }
            }

        }
        //Funcion que consume los tipos básicos int, char o float
        public bool Tipo()
        {
            // Predict ( Tipo -> int | float | char )
            // =  { int } {float} {char}
            if (token[1].Equals("int") | token[1].Equals("float") | token[1].Equals("char"))
            {
                Consumir(token[1], 1);
                return true;
            }
            else 
            {
                errores = true;
                MessageBox.Show("Error: Se esperaba alguno de 'int', 'float', 'char'"); //Mensaje de tokens esperados
                Thread.CurrentThread.Abort();
                return false;
            }
        }
        //Produce sentencias que contienen atributos o metodos
        public bool M1()
        {
           
            // Predict ( M' -> Tipo' Id M'' )
            // =  { int, float, char,Id }
            if (token[0].Equals("Id") || token[1].Equals("int") || token[1].Equals("float") || token[1].Equals("char"))
            {
                nueva_variable.Tipo = token[1];
                nuevo_metodo.Tipo = token[1]; //Guarda el valor de tipo para variable o metodo
                Tipo1();
                nueva_variable.Id = token[1];
                nuevo_metodo.Id = token[1];     //Guarda el Id para variable o metodo
                Consumir("Id", 0);
                M2();                                               //Se dirije a consumir la variable o el metodo
            }
            else
            {
                // Predict ( M' -> const Tipo Id C ;Cont )
                // =  { const }
                if (token[1].Equals("const"))
                {
                    Consumir("const", 1);
                    nueva_variable.Tipo = token[1];             //Guarda el tipo de las variables en la sentencia const 
                    Tipo();                                     //Consume solamente int, float o char
                    vars.Add(token[1]);                         //Guarda el valor de la primera variable const (puede ser la unica)
                    Consumir("Id", 0);
                    C();                                        //Hace Recursion para obtener mas variables y el valor asignado a const
                    Consumir(";", 1);
                    nueva_variable = new Variable();                  //Reestablece los valores de default para la siguiente variable a leer
                    Cont();                                      //Hace recursion para sacar mas contenido ya sea variables o metodos
                }   
                else
                {
                    // Predict ( M' -> void Id ( Firma ) ; G')
                    // =  { void }
                    if (token[1].Equals("void"))                //Encuentra la primera declaracion de firma void
                    {
                        nuevo_metodo.Tipo = token[1];            //Guarda el void como Tipo del metodo
                        Consumir("void", 1);
                        nuevo_metodo.Id = token[1];             //Guarda el Id para el nuevo metodo
                        Consumir("Id", 0);
                        Consumir("(", 1);
                        Firma();                                //Entra a la rutina que buscará en caso de existir la lista de atributos para guardar sus tipos
                        Consumir(")", 1);
                        Consumir(";", 1);
                        nueva_clase.Metodos.Add(nuevo_metodo); //Con la informacion recopilada agrega el metodo a la clase que pertence
                        nuevo_metodo = new Metodo();                  //Restaura los valores por default del metodo
                        G1();
                    }
                    else
                    {
                        errores = true;                         //Marca de error
                        MessageBox.Show("Error: Se esperaba alguno de 'int', 'float', 'char', 'const', 'void'");//Mensaje con los tokens esperados
                        Thread.CurrentThread.Abort();           //Termina la ejecucion del analizador sintactico por error
                        return false;
                    }
                }
            }
            return true;
        }
        public bool M2()
        {
            // Predict ( M'' -> Var ; Cont )
            // =  { ; | , }

            
            if (token[1].Equals(";") || token[1].Equals(","))
            {
                aux_mod = nueva_variable.Mod;
                aux_tipo = nueva_variable.Tipo;
                nueva_clase.Variables.Add(nueva_variable);      //Agraga la primera variable de una sentencia con varias
                nueva_variable = new Variable();
                nueva_variable.Tipo = aux_tipo;
                nueva_variable.Mod = aux_mod;
                Var();                                          //Recorre buscando mas variables o el fin de la lista
                Consumir(";", 1);
                Cont();                                           //Retorna por la siguiente sentencia de tipo variable o metodo
            }
            else 
                // Predict ( M'' -> ( Firma ); G' )
                // =  { ( }
                if (token[1].Equals("("))                       //Encuentra el principio de una firma
                {
                    Consumir("(", 1);
                    Firma();                                    //Busca si es que existe la lista de atributos de una firma
                    nueva_clase.Metodos.Add(nuevo_metodo);      //Agrega el metodo recopilado a la clase que pertence
                    nuevo_metodo = new Metodo();                       //Reestablece los valores por default
                    Consumir(")", 1);
                    Consumir(";", 1);
                    G1();                                       //Entra a la rutina G1 que busca si existen mas metodos a capturar
                }
                else
                {
                    errores = true;
                    MessageBox.Show("Error: Se esperaba alguno de ';' | ',' | '('"); //Mensaje de error de token esperados
                    Thread.CurrentThread.Abort();                                   //Interrumpe el Analizador Sintactico
                    return false;
                }
            return true;
        }       
        public bool Var()
        {
            // Predict ( Var -> , Id Var )
            // =  { , }
            if (token[1].Equals(","))
            {
                Consumir(",", 1);
                nueva_variable.Id = token[1];           //Guarda el Id de una nueva variable
                Consumir("Id", 0);
                aux_mod = nueva_variable.Mod;
                aux_tipo = nueva_variable.Tipo;
                nueva_clase.Variables.Add(nueva_variable);//Guarda la variable en la clase a la que corresponde
                nueva_variable = new Variable();
                nueva_variable.Tipo = aux_tipo;
                nueva_variable.Mod = aux_mod;
                Var();                                  //Recursion para buscar más variables en la sentencia
            }
            else {
                // Predict ( Var -> Lambda )
                // =  { ; }
                if (!token[1].Equals(";"))
                {
                    errores = true;
                    MessageBox.Show("Error: Se esperaba alguno de ','"); //Mensaje de token esperado
                    return false;
                }
            }
            return true;
        }        
        public void C()
        {
            // Predict ( C -> D = Valor )
            // =  { '='  ',' }
                D();
                Consumir("=", 1);       //Consume el igual para denotar asignacion
                Valor();                //Entra a buscar un valor valido para la asignacion
        }
        //Busca una lista de Id's
        public bool D() {
            // Predict ( D -> , Id D  )
            // =  { , }
            if (token[1].Equals(","))
            {
                Consumir(",", 1);
                vars.Add(token[1]);         //Agrega el id a la lista de variables de la sentencia const
                Consumir("Id",0);
                D();                        //Recursion para buscar mas elementos en la sentencia
            }
            else {
                // Predict ( D -> Lambda )
                // =  { = }
                if(!token[1].Equals("=")) 
                {
                    errores = true;
                    MessageBox.Show("Error: Se esperaba alguno de ','"); //Mensaje de tocken esperado
                    return false;
                }
            }
            return true;
        }
        //Rutina que evalua un token como valor valido ID, Num entero, realsigno o numero entero con signo
        public bool Valor()
        {
            //string aux_mod;
            //string aux_tipo;
            // Predict ( Valor -> Id | NumEntero | RealSigno | NumEnteroSigno )
            // =  { Id } { NumENtero } {RealSigno} {NumEnteroSigno}
            if (token[0].Equals("Id", 0) || token[0].Equals("NumEntero") || token[0].Equals("RealSigno") || token[0].Equals("NumEnteroSigno"))
            {
                string t_valor= "Id";
                if (!token[0].Equals("Id"))
                {
                    t_valor = token[0].Equals("RealSigno") ? "float" : "int";
                }
                foreach(string v in vars)                       //Ha concluido la recopilacion de variables en sentencia const
                {
                    aux_mod = nueva_variable.Mod;
                    aux_tipo = nueva_variable.Tipo;
                    nueva_variable.Tipo_valor = t_valor;
                    nueva_variable.Id = v;                      //Asigna el Id guardado a una nueva variable
                    nueva_variable.asigna_const(token[1]);      //Asigna el valor encontrado en la sentencia
                    nueva_clase.Variables.Add(nueva_variable);  //Agrega la variable a la clase que corresponde
                    nueva_variable = new Variable();
                    nueva_variable.Tipo = aux_tipo;
                    nueva_variable.Mod = aux_mod;
                }
                vars.Clear();                                   //Limpia la lista de variables const
                Consumir(token[0], 0); //agregar valor a la variable const
                return true;
            }
            else
            {
                errores = true;
                MessageBox.Show("Error: Se esperaba alguno de 'Id', 'NumEntero', 'RealSigno', 'NumEnteroSigno'"); //Mensaje de tokens esperados
                return false;
            }
        }
        public bool G1()
        {
            // Predict ( G' ->  Mod G )
            // =  { public , protected , private, void, int, char, float,id }
            if (token[1].Equals("public") || token[1].Equals("protected") || token[1].Equals("private"))
            {
                nuevo_metodo.Mod = token[1];    //Guarda el mod del metodo nuevo
                Mod();                          // Llamada a Mod para consumir el modo
            }
            else
            {
                // Predict (G' -> lambda)
                // = { '}' }
                if(token[1].Equals("}"))
                {
                    return true;
                }
                if (!(token[1].Equals("int") || token[1].Equals("char") || token[1].Equals("float") || token[0].Equals("Id") || token[1].Equals("}") || token[1].Equals("void")))
                {
                    errores = true;
                    MessageBox.Show("Error: Se esperaba alguno de:  public , protected , private, void, int, char, float,id");
                    return false;
                }
            }
            G();                                 //Ingresa a la rutina de captura de Firma
            return true;
            
        }
        public bool G()
        {
            // Predict ( G -> void Id ( Firma ); G1 )
            // =  { void }
            if (token[1].Equals("void"))
            {
                nuevo_metodo.Tipo = token[1];                //Asigna al tipo de la firma void
                Consumir("void", 1);            
                                      
            }
            else {
                // Predict ( G -> Tipo' Id ( Firma ) ; G1 )
                // =  { int, float, char, Id }
                if (token[0].Equals("Id") || token[1].Equals("int") || token[1].Equals("float") || token[1].Equals("char"))
                {
                    nuevo_metodo.Tipo = token[1];          //Asigna a la firma su tipo
                    Tipo1();                               //Consume el token de tipo
                }
                else
                {
                    if (!token[1].Equals("}"))
                    {
                        errores = true;                 //Marca error
                        MessageBox.Show("Error: Se esperaban alguno de 'int', 'float', 'char', 'void'"); //Mensaje con los tokens esperados
                        return false;
                    }
                }
            }
            nuevo_metodo.Id = token[1];                 //Guarda el Id de la firma
            Consumir("Id", 0);
            Consumir("(", 1);
            Firma();                                    //Busca si es que existe la lista de atributos de una firma
            nueva_clase.Metodos.Add(nuevo_metodo);      //Agrega el metodo recopilado a la clase que pertence
            nuevo_metodo = new Metodo();                       //Reestablece los valores por default
            Consumir(")", 1);
            Consumir(";", 1);
            G1();                                       //Entra a la rutina G1 que busca si existen mas metodos a capturar
            return true;
        }
        public bool Firma()
        {
            // Predict ( Firma -> Tipo' Id Firma' )
            // =  { int, float, char, Id }
            if (token[1].Equals("int") || token[1].Equals("float") || token[1].Equals("char") || token[0].Equals("Id")) 
            {
                nuevo_metodo.TiposFirma.Add(token[1]);  //Asigna el tipo  correspondiete al atributo
                Tipo1();                                 //Consume el tipo
                nuevo_metodo.IdTipos_firma.Add(token[1]);
                Consumir("Id", 0);                      //Consume el id, no se almacena
                Firma1();                               //Recursion para bucar mas atributos en la firma
            }
            else {
                // Predict ( Firma -> Lambda )
                // =  { ) }
                if (!token[1].Equals(")")) 
                {
                    errores = true;
                    MessageBox.Show("Error: Se esperaba alguno de 'int', 'float', 'char', Id"); //Mensaje de tokens esperados
                    return false;
                }
            }
            return true;
        }
        public bool Firma1()
        { 
            // Predict ( Firma' -> , Firma )
            // =  { , }
            if (token[1].Equals(",")) 
            {
                Consumir(",", 1);   //Consume la coma
                Firma();            //Recursion para consumir el atributo
            }
            else {
                // Predict ( Firma' -> Lambda )
                // =  { ) }
                if (!token[1].Equals(")")) 
                {
                    errores = true;
                    MessageBox.Show("Error: Se esperaba alguno de ','");//Mensaje de tokens esperados
                    return false;
                }
            }
            return true;
        } 

        // Funcion de Consumir tokens desde el Sintáctico
        public bool Consumir(String a, int indice)
        {
            // Si el token a consumir es el correcto
            if (token[indice].Equals(a))
            {
                // Pedir sig token
                String aux = analizador_lexico.sigToken();
                token = aux.Split(null);
                return true; // Regresar verdadero, consumo correcto
            }
            errores = true;
            MessageBox.Show("Error se esperaba '" + a +"'");
            Thread.CurrentThread.Abort();

            // El token a consumir no es el correcto
            return false; // Regresar falso, consumo incorrecto
        }
    }
}