using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProyectoCompiladores
{
    class Analizador_Semantico
    {
        Analizador_Sintactico a_sintactico { get; set; }
        static bool error;
        public string errores;
        public bool Error
        {
            get { return error; }
            set { error = value; }
        }
        public Analizador_Semantico(Analizador_Sintactico a)
        {
            a_sintactico = a;
            errores = "";
        }

        public bool analiza(string nombre_archivo)
        {
            return (clase_duplicada() || clase_publica(nombre_archivo) || extendValido() || firmas_duplicadas() || existe_retornofirma() || id_duplicadoenfirma() || variables_duplicadas() || existe_tipovariable() || concuerda_const());
        }
        //recorré la lista de clases buscando Ids repetidos
        public bool clase_duplicada()
        {
            TablaSimbolos tabla = a_sintactico.Tabla;           //Apuntador a la tabla de simbolos
            int count=0;
            foreach(Clase_valores clase in tabla.Clases)        //Recorré la tabla de clases
            {
                for (int i = count+1;i< tabla.Clases.Count ; i++ )//Recorré el resto de las clases
                {
                    if(clase.Id.Equals(tabla.Clases[i].Id))     //Si encuentra un id duplicado
                    {
                        errores += "Id de clase duplicado " + clase.Id + "\n";
                        return true;                        //encontro un simbolo duplicado
                        
                    }
                }
                count++;
            }
            return false;
        }
        //recorré la lista de clases buscando que el Id coincida con el nombre del archivo y que sea publica
        public bool clase_publica(string nombre_clase)
        {
            bool error = true;
            TablaSimbolos tabla = a_sintactico.Tabla;
            foreach (Clase_valores clase in tabla.Clases)   //Recorre todas las clases
            {
                if (clase.Id.Equals(nombre_clase))      //Localiza la clase coincidente con el nombre de archivo
                {
                    if (clase.Mod.Equals("public"))
                    {
                        error = false;                    //Tiene que encontrar la clase coincidente al nombre del archivo y debe ser publica
                    }
                    else
                    {
                        errores += "La clase principal :" + clase.Id + " debe ser Public\n";
                    }
                }
                else
                {
                    if (clase.Mod.Equals("public"))     //Si no es la clase principal y el mod es public, marca error
                    {
                        errores += "La clase no principal :" + clase.Id + " no debe ser Public\n";
                        return true;                    //solo la clase de nombre de archivo debe ser publica;
                    }
                }
            }
            if (error)                                  //No encontro el nombre del archivo como la clase principal
            {
                errores += "La clase :" + nombre_clase + " debe existir\n";
            }
            return error;
        }

        public bool extendValido()
        {

            TablaSimbolos tabla = a_sintactico.Tabla;
            foreach (Clase_valores clase in tabla.Clases)
            {
                if(!clase.Extend.Equals(""))            //Caso en el que contiene extend
                {
                    bool e = true;
                    foreach(Clase_valores clase_2 in tabla.Clases)  //Recorrerá las clases buscando el Id que avale el extend
                    {
                        if(!clase.Equals(clase_2))                  //Revisa que la comparacion sea diferente a la misma clase
                        {
                            if(clase.Extend.Equals(clase_2.Id))     //Encuentra el extend en la lista de Id de clases
                            {
                                e = false;                  //cambia la variable de error a falso
                            }
                        }
                    }
                    if (e)                                  //Caso en el que no encontro el extend dentro de la lista de clases
                    {
                        errores += "La clase "+ clase.Extend +" de la que :" + clase.Id + " se pretende extender no existe\n";
                        return e;
                    }
                }
            }
            return false;
        }

        public bool firmas_duplicadas()
        {
            TablaSimbolos tabla = a_sintactico.Tabla;
            
            foreach (Clase_valores clase in tabla.Clases)        //Recrre todas las clases
            {
                int count = 0;
                foreach (Metodo metod in clase.Metodos)         //Recorre los metodos de todas las clases
                {
                    for (int i = count + 1; i < clase.Metodos.Count; i++) //Recorre los metodos restantes
                    {
                        if (metod.Id.Equals(clase.Metodos[i].Id))   //Encuentra sobrecarga de metodos
                        {
                            if(metod.TiposFirma.Count == clase.Metodos[i].TiposFirma.Count) //misma cantidad de parametros
                            {
                                bool igual = true;
                                for(int j=0; j<metod.TiposFirma.Count;j++)
                                {
                                    if(!metod.TiposFirma[j].Equals(clase.Metodos[i].TiposFirma[j]))
                                    {
                                        igual = false;
                                    }
                                }
                                if(igual)                               //si encontro una firma duplicada
                                {
                                    errores += "La firma :" + metod.Id + " esta duplicada\n";
                                    return true;
                                }
                            }
                        }
                    }
                    count++;
                }
            }
            return false;
        }

        public bool existe_retornofirma()
        {
            TablaSimbolos tabla = a_sintactico.Tabla;

            foreach (Clase_valores clase in tabla.Clases)        //Recrre todas las clases
            {
                foreach (Metodo metod in clase.Metodos)         //Recorre los metodos de todas las clases
                {
                    if (!(metod.Tipo.Equals("int") || metod.Tipo.Equals("float") || metod.Tipo.Equals("char") || metod.Tipo.Equals("void"))) //Si el tipo de retorno no es basico (es de clase)
                    {
                        bool retorno = true;
                        foreach(Clase_valores clase_2 in tabla.Clases) //Recorre a todas las clases buscando el id de retorno
                        {
                            if(metod.Tipo.Equals(clase_2.Id))           //Si encuentra el id en una clase esta bien
                            {
                                retorno = false;
                            }
                        }
                        if(retorno)                                     //En caso de no encontrar el id valido hay error semantico
                        {
                            errores += "El tipo de retorno :" + metod.Tipo + " en la firma "+metod.Id+" no se encuentra en la lista de clases\n";
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool id_duplicadoenfirma()
        {
            TablaSimbolos tabla = a_sintactico.Tabla;

            foreach (Clase_valores clase in tabla.Clases)        //Recrre todas las clases
            {
                foreach (Metodo metod in clase.Metodos)         //Recorre los metodos de todas las clases
                {
                    int count = 0;
                    foreach(string s in metod.IdTipos_firma)    //Recorre todas los id's de las firmas
                    {
                        for (int i = count + 1; i < metod.IdTipos_firma.Count; i++) //Recorre los id's restantes
                        {
                            if (s.Equals(metod.IdTipos_firma[i]))                   //Localiza si existen ids duplicados
                                    {
                                        errores += "La firma :" + metod.Id + " tiene el sig id: "+s+" duplicado\n";
                                        return true;
                                    }
                        }
                        count++;
                    } 
                }
            }
            return false;
        }

        public bool variables_duplicadas()
        {
            TablaSimbolos tabla = a_sintactico.Tabla;
            foreach (Clase_valores clase in tabla.Clases)        //Recrre todas las clases
            {
                int count = 0;
                foreach (Variable var in clase.Variables)         //Recorre las variables de todas las clases
                {
                    for (int i = count + 1; i < clase.Metodos.Count; i++) //Recorre las variables restantes
                    {
                        if (var.Id.Equals(clase.Variables[i].Id))   //Encuentra ids duplicados
                        {
                            errores += "La variable: " +var.Id + " esta duplicada en la Clase "+clase.Id+" \n";
                                    return true;
                        }
                    }
                    count++;
                }
            }
            return false;
        }

        public bool existe_tipovariable()
        {
            TablaSimbolos tabla = a_sintactico.Tabla;

            foreach (Clase_valores clase in tabla.Clases)        //Recrre todas las clases
            {
                foreach (Variable var in clase.Variables)         //Recorre las variables de todas las clases
                {
                    if (!(var.Tipo.Equals("int") || var.Tipo.Equals("float") || var.Tipo.Equals("char"))) //Si el tipo de retorno no es basico (es de clase)
                    {
                        bool retorno = true;
                        foreach(Clase_valores clase_2 in tabla.Clases) //Recorre a todas las clases buscando el id de retorno
                        {
                            if(var.Tipo.Equals(clase_2.Id))           //Si encuentra el id en una clase esta bien
                            {
                                retorno = false;
                            }
                        }
                        if(retorno)                                     //En caso de no encontrar el id valido hay error semantico
                        {
                            errores += "El tipo de retorno "+var.Tipo+" de la variable" + var.Id + " no existe\n";
                            return true;
                        }
                    }
                }
            }
            return false;
        
        }

        public bool concuerda_const()
        {
            TablaSimbolos tabla = a_sintactico.Tabla;
            foreach (Clase_valores clase in tabla.Clases)        //Recrre todas las clases
            {
                foreach (Variable var in clase.Variables)         //Recorre las variables de cada clase
                {
                    if(var.IsConst)                                //Si la variable es de modo constante
                    {
                        if(var.Tipo_valor.Equals("Id"))             //Caso en el que la variable de tipo const tiene asignado otra variable
                        {
                            bool tipo = true;
                            foreach (Variable var_2 in clase.Variables)         //Recorre las variables de la clase
                            {
                                if(!var.Equals(var_2))                          //revisa no ser ella misma
                                {
                                    if(var_2.Id.Equals(var.Valor))                  //Encontro la variable de id igual al tipo valor
                                    {
                                        var.Tipo_valor = var_2.Tipo;
                                        tipo = false;
                                    }
                                }           
                            }
                            if(tipo)                                        //Termino el recorrido y no encontró el Id propio del valor
                            {
                                errores += "El valor que se quiere asignar :" + var.Valor + " a la variable "+var.Id+" no existe\n";
                                return tipo;                                    //en caso de ser valor tipo Id y no ser igual a ninguna variable
                            }
                        }
                        if(!var.Tipo_valor.Equals(var.Tipo))                 //Revisa que concuerde el tipovalor con el tipo de la variable
                        {
                            errores += "El valor que se pretende asignar :" + var.Tipo_valor + " a la variable "+var.Id+" no es del mismo tipo que "+var.Tipo+"\n";
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
